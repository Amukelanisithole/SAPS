using Logic.Model;
using Logic.Model.Auth;

namespace Logic.BusinessLogicInterface
{
    public interface IPasswordResetLogic
    {
        Task<ResultModel> RequestResetAsync(string email, Func<string, string, string> buildResetLink, CancellationToken ct = default);

        Task<ResultModel> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
