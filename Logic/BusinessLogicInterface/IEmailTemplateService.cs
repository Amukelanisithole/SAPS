namespace Logic.BusinessLogicInterface
{
    public interface IEmailTemplateService
    {
        Task SendAsync(long templateId, string toEmail, IDictionary<string, string> placeholders, CancellationToken ct = default);
    }
}
