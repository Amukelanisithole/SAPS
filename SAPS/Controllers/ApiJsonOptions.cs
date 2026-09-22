using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPS.Controllers
{
    // Shared options for the manual JsonSerializer.Serialize(...) calls controllers
    // use to build ResultModel.Data. EF entities returned straight from these
    // endpoints carry bidirectional navigation properties (e.g. Docket <-> PreliminaryReport)
    // — serializing the raw entity would walk that cycle and throw "possible object
    // cycle detected". IgnoreCycles stops that by omitting the second occurrence.
    public static class ApiJsonOptions
    {
        public static readonly JsonSerializerOptions SafeSerialize = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
    }
}
