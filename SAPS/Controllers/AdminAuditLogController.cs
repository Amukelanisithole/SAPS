using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPS.Controllers
{
    [Route("Admin/AuditLogs")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminAuditLogController : Controller
    {
        private const int PageSize = 25;

        private readonly IAuditLogQueryLogic _auditLogQueryLogic;

        public AdminAuditLogController(IAuditLogQueryLogic auditLogQueryLogic)
        {
            _auditLogQueryLogic = auditLogQueryLogic;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var result = await _auditLogQueryLogic.GetPagedAsync(page, PageSize, search);
            ViewBag.Search = search;
            return View(result);
        }
    }
}
