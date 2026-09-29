using Logic.Model;
using Logic.Model.Auth;

namespace Logic.BusinessLogicInterface
{
    public interface IAuthService
    {
        Task<ResultModel<LoginResponse>> RegisterAsync(RegisterRequest request, Func<string, string, string> buildVerificationLink);
        Task<ResultModel<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
