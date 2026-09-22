namespace Logic.BusinessLogicInterface
{
    public record NearbyPoliceStation(long OsmNodeId, string Name, double Latitude, double Longitude, string? Phone);

    public record AddressLookupResult(string DisplayName, string? Province);

    // Looks up real-world police stations from OpenStreetMap (Overpass API) and
    // resolves human-readable addresses (Nominatim) — both free, no API key.
    public interface IPoliceStationLookupService
    {
        Task<IEnumerable<NearbyPoliceStation>> FindNearbyAsync(double latitude, double longitude, double radiusKm);
        Task<AddressLookupResult?> ReverseGeocodeAsync(double latitude, double longitude);
    }
}
