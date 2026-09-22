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
    // The super admin's own capability: provisioning every other account
    // (SAPS Officer, Station Commander, System Administrator, or Complainant).
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdministrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementLogic _userManagementLogic;

        public UsersController(IUserManagementLogic userManagementLogic)
        {
            _userManagementLogic = userManagementLogic;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManagementLogic.CreateUserAsync(User.GetUserId(), dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManagementLogic.GetAllAsync();
            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(users, ApiJsonOptions.SafeSerialize) });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var user = await _userManagementLogic.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ResultModel { ResultId = EResultValue.Fail, Message = $"No user found for id {id}." });
            }

            return Ok(new ResultModel { ResultId = EResultValue.Success, Message = "Successful", Data = JsonSerializer.Serialize(user, ApiJsonOptions.SafeSerialize) });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManagementLogic.UpdateUserAsync(User.GetUserId(), id, dto);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/Deactivate")]
        public async Task<IActionResult> Deactivate(long id)
        {
            var result = await _userManagementLogic.SetActiveStatusAsync(User.GetUserId(), id, isActive: false);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/Activate")]
        public async Task<IActionResult> Activate(long id)
        {
            var result = await _userManagementLogic.SetActiveStatusAsync(User.GetUserId(), id, isActive: true);
            return result.ResultId == EResultValue.Success ? Ok(result) : BadRequest(result);
        }
    }
}
