namespace Logic.BusinessLogicInterface
{
    public interface IStationRoutingService
    {
        Task<long?> FindOrCreateNearestStationIdAsync(double latitude, double longitude);
    }
}
