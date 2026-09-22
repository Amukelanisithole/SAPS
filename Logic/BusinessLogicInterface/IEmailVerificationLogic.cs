using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IEmailVerificationLogic
    {
        // Generates a fresh token and (re)sends the combined Welcome/Verify
        // email — used both right after registration and for a "resend"
        // action. A no-op if the account is already verified.
        Task SendVerificationEmailAsync(long userId, Func<string, string, string> buildVerificationLink, CancellationToken ct = default);

        Task<ResultModel> VerifyEmailAsync(string email, string token, CancellationToken ct = default);
    }
}
