using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Route("Admin/Reports")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminReportsController : Controller
    {
        private readonly IAdminReportLogic _adminReportLogic;
        private readonly ICsvExportService _csvExportService;
        private readonly IPdfExportService _pdfExportService;

        public AdminReportsController(
            IAdminReportLogic adminReportLogic,
            ICsvExportService csvExportService,
            IPdfExportService pdfExportService)
        {
            _adminReportLogic = adminReportLogic;
            _csvExportService = csvExportService;
            _pdfExportService = pdfExportService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(string type, string format)
        {
            var table = await _adminReportLogic.GetReportAsync(type);
            if (table == null)
            {
                return NotFound();
            }

            var fileNameBase = $"{table.Title.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = _csvExportService.Generate(table);
                return File(bytes, "text/csv", $"{fileNameBase}.csv");
            }

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = _pdfExportService.Generate(table);
                return File(bytes, "application/pdf", $"{fileNameBase}.pdf");
            }

            return BadRequest("Unsupported export format. Use 'csv' or 'pdf'.");
        }
    }
}
