using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAPS.Extensions;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    // "Admin" prefix avoids colliding with the existing api/Users JSON controller.
    [Route("Admin/Users")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminUsersController : Controller
    {
        private readonly IUserManagementLogic _userManagementLogic;
        private readonly IStationLogic _stationLogic;

        public AdminUsersController(IUserManagementLogic userManagementLogic, IStationLogic stationLogic)
        {
            _userManagementLogic = userManagementLogic;
            _stationLogic = stationLogic;
        }

        private const int PageSize = 10;

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, long? roleId = null, long? stationId = null, bool? isActive = null)
        {
            var users = await _userManagementLogic.GetAllPagedAsync(page, PageSize, roleId, stationId, isActive);
            ViewBag.RoleId = roleId;
            ViewBag.StationId = stationId;
            ViewBag.IsActive = isActive;
            ViewBag.Stations = await _stationLogic.GetAllAsync();
            return View(users);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new CreateUserDTO());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(dto);
            }

            var result = await _userManagementLogic.CreateUserAsync(User.GetUserId(), dto);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await PopulateDropdownsAsync();
                return View(dto);
            }

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(long id)
        {
            var user = await _userManagementLogic.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = id;
            await PopulateDropdownsAsync();
            return View(new UpdateUserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                StationId = user.StationId
            });
        }

        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                await PopulateDropdownsAsync();
                return View(dto);
            }

            var result = await _userManagementLogic.UpdateUserAsync(User.GetUserId(), id, dto);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                await PopulateDropdownsAsync();
                return View(dto);
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}/Deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(long id)
        {
            var result = await _userManagementLogic.SetActiveStatusAsync(User.GetUserId(), id, isActive: false);
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}/Activate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(long id)
        {
            var result = await _userManagementLogic.SetActiveStatusAsync(User.GetUserId(), id, isActive: true);
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Roles = new List<SelectListItem>
            {
                new(nameof(ERole.Complainant), ERole.Complainant.ToString()),
                new(nameof(ERole.SapsOfficer), ERole.SapsOfficer.ToString()),
                new(nameof(ERole.StationCommander), ERole.StationCommander.ToString()),
                new(nameof(ERole.SystemAdministrator), ERole.SystemAdministrator.ToString())
            };

            var stations = await _stationLogic.GetAllAsync();
            ViewBag.Stations = stations.Select(s => new SelectListItem(s.StationName, s.Id.ToString())).ToList();
        }
    }
}
