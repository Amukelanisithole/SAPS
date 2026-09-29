using Logic.BusinessLogicInterface;
using Logic.DTO;
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
    public class PreliminaryReportController : ControllerBase
    {
        private readonly IPreliminaryReportLogic _preliminaryReportLogic;

        public PreliminaryReportController(IPreliminaryReportLogic preliminaryReportLogic)
        {
            _preliminaryReportLogic = preliminaryReportLogic;
        }

        [HttpPost("Submit")]
        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Submit([FromBody] PreliminaryReportDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _preliminaryReportLogic.SubmitAsync(User.GetUserId(), dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var report = await _preliminaryReportLogic.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound(new ResultModel { ResultId = EResultValue.Fail, Message = $"No preliminary report found for id {id}." });
            }

            var isOwner = report.ComplainantId == User.GetUserId();
            var isStaff = User.GetRoleName() is "SapsOfficer" or "StationCommander" or "SystemAdministrator";
            if (!isOwner && !isStaff)
            {
                return Forbid();
            }

            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(report, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("Mine")]
        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> GetMine()
        {
            var reports = await _preliminaryReportLogic.GetMineAsync(User.GetUserId());
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(reports, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("ForReview")]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> GetForReview()
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                return BadRequest(new ResultModel { ResultId = EResultValue.Fail, Message = "Your account is not assigned to a station." });
            }

            var reports = await _preliminaryReportLogic.GetForReviewAsync(stationId.Value);
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(reports, ApiJsonOptions.SafeSerialize) });
        }
    }
}
