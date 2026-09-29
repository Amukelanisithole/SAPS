using Logic.BusinessLogicInterface;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Logic.BusinessLogic
{
    public class PoliceStationLookupService : IPoliceStationLookupService
    {
        private const string OverpassUrl = "https://overpass-api.de/api/interpreter";
        private const string NominatimReverseUrl = "https://nominatim.openstreetmap.org/reverse";

        private readonly HttpClient _httpClient;

        public PoliceStationLookupService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SAPS-CrimePreReportingSystem/1.0");
            }
        }

        public async Task<IEnumerable<NearbyPoliceStation>> FindNearbyAsync(double latitude, double longitude, double radiusKm)
        {
            try
            {
                var radiusMeters = (int)(radiusKm * 1000);
                var lat = latitude.ToString(CultureInfo.InvariantCulture);
                var lon = longitude.ToString(CultureInfo.InvariantCulture);
                var query = $"[out:json][timeout:25];node[\"amenity\"=\"police\"](around:{radiusMeters},{lat},{lon});out body;";

                using var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
                using var response = await _httpClient.PostAsync(OverpassUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    return Enumerable.Empty<NearbyPoliceStation>();
                }

                var result = await response.Content.ReadFromJsonAsync<OverpassResponse>();
                if (result?.Elements == null)
                {
                    return Enumerable.Empty<NearbyPoliceStation>();
                }

                return result.Elements
                    .Where(e => !string.IsNullOrWhiteSpace(e.Tags?.Name))
                    .Select(e => new NearbyPoliceStation(e.Id, e.Tags!.Name!, e.Lat, e.Lon, e.Tags.Phone))
                    .ToList();
            }
            catch (Exception)
            {
                return Enumerable.Empty<NearbyPoliceStation>();
            }
        }

        public async Task<AddressLookupResult?> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var lat = latitude.ToString(CultureInfo.InvariantCulture);
                var lon = longitude.ToString(CultureInfo.InvariantCulture);
                var url = $"{NominatimReverseUrl}?format=json&addressdetails=1&lat={lat}&lon={lon}";

                var result = await _httpClient.GetFromJsonAsync<NominatimReverseResponse>(url);
                if (result == null)
                {
                    return null;
                }

                return new AddressLookupResult(result.DisplayName ?? "Address unavailable", result.Address?.State);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class OverpassResponse
        {
            [JsonPropertyName("elements")]
            public List<OverpassElement>? Elements { get; set; }
        }

        private class OverpassElement
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("lat")]
            public double Lat { get; set; }

            [JsonPropertyName("lon")]
            public double Lon { get; set; }

            [JsonPropertyName("tags")]
            public OverpassTags? Tags { get; set; }
        }

        private class OverpassTags
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("phone")]
            public string? Phone { get; set; }
        }

        private class NominatimReverseResponse
        {
            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }

            [JsonPropertyName("address")]
            public NominatimAddress? Address { get; set; }
        }

        private class NominatimAddress
        {
            [JsonPropertyName("state")]
            public string? State { get; set; }
        }
    }
}
