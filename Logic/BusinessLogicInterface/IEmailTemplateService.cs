namespace Logic.BusinessLogicInterface
{
    public interface IEmailTemplateService
    {
        // Fetches the template row by id, replaces every {Key} in its
        // Subject and Body with the matching value from placeholders, and
        // sends the result via IEmailSender. A missing/inactive template
        // is logged and skipped rather than thrown — a broken template row
        // should never take down the business action that triggered it.
        Task SendAsync(long templateId, string toEmail, IDictionary<string, string> placeholders, CancellationToken ct = default);
    }
}
