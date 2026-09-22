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
    public class EscalationController : ControllerBase
    {
        private readonly IEscalationLogic _escalationLogic;

        public EscalationController(IEscalationLogic escalationLogic)
        {
            _escalationLogic = escalationLogic;
        }

        // FR-07: a complainant escalates unresolved or poorly handled matters.
        [HttpPost]
        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Create([FromBody] EscalationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _escalationLogic.CreateAsync(User.GetUserId(), dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("Mine")]
        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> GetMine()
        {
            var escalations = await _escalationLogic.GetMineAsync(User.GetUserId());
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(escalations, ApiJsonOptions.SafeSerialize) });
        }

        // FR-09: Station Commanders monitor escalations raised against their station.
        [HttpGet("Station")]
        [Authorize(Roles = "StationCommander")]
        public async Task<IActionResult> GetForMyStation()
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                return BadRequest(new ResultModel { ResultId = EResultValue.Fail, Message = "Your account is not assigned to a station." });
            }

            var escalations = await _escalationLogic.GetForStationAsync(stationId.Value);
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(escalations, ApiJsonOptions.SafeSerialize) });
        }

        [HttpPut("{id}/Status")]
        [Authorize(Roles = "StationCommander")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] EscalationStatusUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _escalationLogic.UpdateStatusAsync(id, User.GetUserId(), dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }
    }
}
