using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Microsoft.EntityFrameworkCore;

namespace Logic.BusinessLogic
{
    // Extracted out of PreliminaryReportLogic so Affidavit (and any future
    // station-routed feature) shares the exact same nearest-station
    // resolution and fallback behavior instead of a second copy of it.
    public class StationRoutingService : IStationRoutingService
    {
        private static readonly double[] LookupRadiiKm = { 10, 25, 50 };

        private readonly ApplicationDbContext _context;
        private readonly IPoliceStationLookupService _policeStationLookupService;

        public StationRoutingService(ApplicationDbContext context, IPoliceStationLookupService policeStationLookupService)
        {
            _context = context;
            _policeStationLookupService = policeStationLookupService;
        }

        // Resolves the real-world SAPS station nearest the complainant via OpenStreetMap
        // (widening the search radius if nothing is tagged nearby yet), reusing a
        // previously auto-created Station row for that same real station where one
        // already exists rather than creating a duplicate. Falls back to whatever
        // stations an admin has manually configured if the external lookup fails.
        public async Task<long?> FindOrCreateNearestStationIdAsync(double latitude, double longitude)
        {
            foreach (var radiusKm in LookupRadiiKm)
            {
                var nearby = await _policeStationLookupService.FindNearbyAsync(latitude, longitude, radiusKm);
                var nearest = nearby
                    .Select(s => new { Station = s, Distance = GeoDistanceHelper.HaversineDistanceKm(latitude, longitude, s.Latitude, s.Longitude) })
                    .OrderBy(s => s.Distance)
                    .FirstOrDefault();

                if (nearest == null) continue;

                var existing = await _context.Stations.FirstOrDefaultAsync(s => s.OsmNodeId == nearest.Station.OsmNodeId);
                if (existing != null) return existing.Id;

                var addressLookup = await _policeStationLookupService.ReverseGeocodeAsync(nearest.Station.Latitude, nearest.Station.Longitude);

                var station = new Station
                {
                    StationName = nearest.Station.Name,
                    Province = addressLookup?.Province ?? "Unknown",
                    Address = addressLookup?.DisplayName ?? "Address unavailable",
                    ContactNumber = nearest.Station.Phone ?? "Not available",
                    Latitude = nearest.Station.Latitude,
                    Longitude = nearest.Station.Longitude,
                    OsmNodeId = nearest.Station.OsmNodeId
                };

                _context.Stations.Add(station);
                await _context.SaveChangesAsync();
                return station.Id;
            }

            return await FindNearestManuallyConfiguredStationIdAsync(latitude, longitude);
        }

        // Safety net for when the external lookup is unreachable or OpenStreetMap has
        // no police stations tagged in the area — matches against whatever an admin
        // has manually configured (e.g. for onboarding officers before any report exists).
        private async Task<long?> FindNearestManuallyConfiguredStationIdAsync(double latitude, double longitude)
        {
            var candidates = await _context.Stations
                .Where(s => s.Latitude != null && s.Longitude != null)
                .Select(s => new { s.Id, s.Latitude, s.Longitude })
                .ToListAsync();

            if (candidates.Count == 0) return null;

            return candidates
                .Select(s => new { s.Id, Distance = GeoDistanceHelper.HaversineDistanceKm(latitude, longitude, s.Latitude!.Value, s.Longitude!.Value) })
                .OrderBy(s => s.Distance)
                .First().Id;
        }
    }
}
