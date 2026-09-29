using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StationController : ControllerBase
    {
        private readonly IStationLogic _stationLogic;

        public StationController(IStationLogic stationLogic)
        {
            _stationLogic = stationLogic;
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdministrator")]
        public async Task<IActionResult> Create([FromBody] StationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _stationLogic.CreateAsync(dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stations = await _stationLogic.GetAllAsync();
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(stations, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var station = await _stationLogic.GetByIdAsync(id);
            if (station == null)
            {
                return NotFound(new ResultModel { ResultId = EResultValue.Fail, Message = $"No station found for id {id}." });
            }

            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(station, ApiJsonOptions.SafeSerialize) });
        }
    }
}
