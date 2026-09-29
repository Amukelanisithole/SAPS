namespace Logic.BusinessLogicInterface
{
    public interface IDocketFollowUpLogic
    {
        Task<int> SendOverdueAlertsAsync(CancellationToken ct = default);
    }
}
