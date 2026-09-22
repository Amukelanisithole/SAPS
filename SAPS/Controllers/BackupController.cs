using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using System.Text.Json;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    // FR-10 / NFR-08: recovery evidence — who ran a backup, when, and whether
    // restoration from it has been tested. System Administrator only.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdministrator")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupLogic _backupLogic;

        public BackupController(IBackupLogic backupLogic)
        {
            _backupLogic = backupLogic;
        }

        [HttpPost]
        public async Task<IActionResult> PerformBackup()
        {
            var result = await _backupLogic.PerformBackupAsync(User.GetUserId());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var backups = await _backupLogic.GetAllAsync();
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(backups, ApiJsonOptions.SafeSerialize) });
        }

        [HttpPut("{id}/RestorationTested")]
        public async Task<IActionResult> MarkRestorationTested(long id)
        {
            var result = await _backupLogic.MarkRestorationTestedAsync(id, User.GetUserId());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }
    }
}
