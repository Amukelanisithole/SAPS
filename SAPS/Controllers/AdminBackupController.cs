using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Route("Admin/Backups")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminBackupController : Controller
    {
        private readonly IBackupLogic _backupLogic;

        public AdminBackupController(IBackupLogic backupLogic)
        {
            _backupLogic = backupLogic;
        }

        [HttpPost("Perform")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perform()
        {
            var result = await _backupLogic.PerformBackupAsync(User.GetUserId());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost("{id}/MarkRestorationTested")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRestorationTested(long id)
        {
            var result = await _backupLogic.MarkRestorationTestedAsync(id, User.GetUserId());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
