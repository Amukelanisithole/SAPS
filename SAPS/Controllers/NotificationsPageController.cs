using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;

namespace SAPS.Controllers
{
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

        [HttpGet("{id}/Open")]
        public async Task<IActionResult> Open(long id)
        {
            var notification = await _notificationLogic.GetMineByIdAsync(id, User.GetUserId());
            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.ReadStatus)
            {
                await _notificationLogic.MarkAsReadAsync(id, User.GetUserId());
            }

            return notification.DocketId != null
                ? RedirectToAction("Details", "Dockets", new { id = notification.DocketId })
                : RedirectToAction(nameof(Index));
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
