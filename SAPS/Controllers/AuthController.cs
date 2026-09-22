using Logic.BusinessLogicInterface;
using Logic.Model.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request, BuildVerificationLink);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        private string BuildVerificationLink(string email, string token) =>
            Url.Action("VerifyEmail", "Account", new { email, token }, Request.Scheme)!;

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);
            return result.ResultId == EResultValue.Success ? Ok(result) : Unauthorized(result);
        }
    }
}
