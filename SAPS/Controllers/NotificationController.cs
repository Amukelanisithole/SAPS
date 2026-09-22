using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using System.Text.Json;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationLogic _notificationLogic;

        public NotificationController(INotificationLogic notificationLogic)
        {
            _notificationLogic = notificationLogic;
        }

        [HttpGet("Mine")]
        public async Task<IActionResult> GetMine()
        {
            var notifications = await _notificationLogic.GetMineAsync(User.GetUserId());
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(notifications, ApiJsonOptions.SafeSerialize) });
        }

        [HttpPut("{id}/Read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var result = await _notificationLogic.MarkAsReadAsync(id, User.GetUserId());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }
    }
}
