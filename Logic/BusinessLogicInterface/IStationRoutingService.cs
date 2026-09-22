namespace Logic.BusinessLogicInterface
{
    // Shared by any feature that needs to route a complainant's submission to
    // their nearest SAPS station (PreliminaryReport, Affidavit, ...) so the
    // lookup/fallback behavior only lives in one place.
    public interface IStationRoutingService
    {
        Task<long?> FindOrCreateNearestStationIdAsync(double latitude, double longitude);
    }
}
