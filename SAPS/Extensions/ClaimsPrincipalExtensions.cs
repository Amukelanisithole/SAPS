using System.Security.Claims;

namespace SAPS.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public static string GetRoleName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static long? GetStationId(this ClaimsPrincipal user)
        {
            var stationIdClaim = user.FindFirst("StationId")?.Value;
            return long.TryParse(stationIdClaim, out var stationId) ? stationId : null;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirst("FullName")?.Value ?? string.Empty;
        }

        public static bool GetEmailVerified(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("EmailVerified")?.Value;
            return claim != "false";
        }
    }
}
