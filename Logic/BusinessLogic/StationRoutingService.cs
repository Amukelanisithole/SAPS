using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Microsoft.EntityFrameworkCore;

namespace Logic.BusinessLogic
{
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
