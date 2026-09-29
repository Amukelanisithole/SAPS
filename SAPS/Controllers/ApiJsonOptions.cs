using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPS.Controllers
{
    public static class ApiJsonOptions
    {
        public static readonly JsonSerializerOptions SafeSerialize = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
    }
}
