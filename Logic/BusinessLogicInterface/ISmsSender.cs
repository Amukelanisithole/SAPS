namespace Logic.BusinessLogicInterface
{
    public interface ISmsSender
    {
        Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default);
    }
}
