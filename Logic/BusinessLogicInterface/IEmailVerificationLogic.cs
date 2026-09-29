using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IEmailVerificationLogic
    {
        Task SendVerificationEmailAsync(long userId, Func<string, string, string> buildVerificationLink, CancellationToken ct = default);

        Task<ResultModel> VerifyEmailAsync(string email, string token, CancellationToken ct = default);
    }
}
