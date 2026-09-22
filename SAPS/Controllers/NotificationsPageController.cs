using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;

namespace SAPS.Controllers
{
    // "Page" suffix avoids colliding with the existing api/Notification JSON controller.
    [Route("Notifications")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class NotificationsPageController : Controller
    {
        private readonly INotificationLogic _notificationLogic;

        public NotificationsPageController(INotificationLogic notificationLogic)
        {
            _notificationLogic = notificationLogic;
        }

        private const int PageSize = 10;

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, bool? readStatus = null)
        {
            var notifications = await _notificationLogic.GetMinePagedAsync(User.GetUserId(), page, PageSize, readStatus);
            ViewBag.ReadStatus = readStatus;
            return View(notifications);
        }

        [HttpPost("{id}/Read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            await _notificationLogic.MarkAsReadAsync(id, User.GetUserId());
            return RedirectToAction(nameof(Index));
        }
    }
}
