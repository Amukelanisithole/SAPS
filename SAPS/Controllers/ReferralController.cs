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
    public class ReferralController : ControllerBase
    {
        private readonly IReferralLogic _referralLogic;

        public ReferralController(IReferralLogic referralLogic)
        {
            _referralLogic = referralLogic;
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        public async Task<IActionResult> Create([FromBody] ReferralDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _referralLogic.CreateAsync(User.GetUserId(), dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("Docket/{docketId}")]
        [Authorize(Roles = "SapsOfficer,StationCommander,SystemAdministrator")]
        public async Task<IActionResult> GetByDocket(long docketId)
        {
            var referrals = (await _referralLogic.GetByDocketIdAsync(docketId)).ToList();

            if (User.GetRoleName() != "SystemAdministrator" && referrals.Count > 0
                && !referrals.Any(r => r.FromStationId == User.GetStationId() || r.ToStationId == User.GetStationId()))
            {
                return Forbid();
            }

            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(referrals, ApiJsonOptions.SafeSerialize) });
        }
    }
}
