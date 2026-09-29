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
    public class DocketController : ControllerBase
    {
        private readonly IDocketLogic _docketLogic;

        public DocketController(IDocketLogic docketLogic)
        {
            _docketLogic = docketLogic;
        }

        [HttpPost("Convert")]
        [Authorize(Roles = "SapsOfficer")]
        public async Task<IActionResult> Convert([FromBody] DocketConvertDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _docketLogic.ConvertAsync(User.GetUserId(), dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var docket = await _docketLogic.GetByIdAsync(id, User.GetUserId(), HttpContext.Connection.RemoteIpAddress?.ToString());
            if (docket == null)
            {
                return NotFound(new ResultModel { ResultId = EResultValue.Fail, Message = $"No docket found for id {id}." });
            }

            var isOwner = docket.NavReport != null && docket.NavReport.ComplainantId == User.GetUserId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            var isOwnStationStaff = User.GetRoleName() is "SapsOfficer" or "StationCommander" && docket.StationId == User.GetStationId();
            if (!isOwner && !isAdmin && !isOwnStationStaff)
            {
                return Forbid();
            }

            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(docket, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("Mine")]
        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> GetMine()
        {
            var dockets = await _docketLogic.GetForComplainantAsync(User.GetUserId());
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(dockets, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("Station")]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> GetForMyStation()
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                return BadRequest(new ResultModel { ResultId = EResultValue.Fail, Message = "Your account is not assigned to a station." });
            }

            var dockets = await _docketLogic.GetForStationAsync(stationId.Value);
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(dockets, ApiJsonOptions.SafeSerialize) });
        }

        [HttpPut("{id}/Status")]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] DocketStatusUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _docketLogic.UpdateStatusAsync(id, User.GetUserId(), dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }
    }
}
