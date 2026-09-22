using Logic.Model;
using Logic.Model.Auth;

namespace Logic.BusinessLogicInterface
{
    public interface IAuthService
    {
        // buildVerificationLink receives (email, rawToken) and must return the
        // full absolute URL the user clicks to verify their email — kept as a
        // delegate so this layer never has to know about MVC routing.
        Task<ResultModel<LoginResponse>> RegisterAsync(RegisterRequest request, Func<string, string, string> buildVerificationLink);
        Task<ResultModel<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
