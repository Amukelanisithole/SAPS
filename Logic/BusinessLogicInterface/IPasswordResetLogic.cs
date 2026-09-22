using Logic.Model;
using Logic.Model.Auth;

namespace Logic.BusinessLogicInterface
{
    public interface IPasswordResetLogic
    {
        // buildResetLink receives (email, rawToken) and must return the full
        // absolute URL the user clicks — kept as a delegate so this layer
        // never has to know about MVC routing.
        Task<ResultModel> RequestResetAsync(string email, Func<string, string, string> buildResetLink, CancellationToken ct = default);

        Task<ResultModel> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    }
}
