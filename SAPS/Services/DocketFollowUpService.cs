using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.Extensions.Options;

namespace SAPS.Services
{
    public class DocketFollowUpService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DocketFollowUpService> _logger;
        private readonly TimeSpan _interval;

        public DocketFollowUpService(IServiceScopeFactory scopeFactory, ILogger<DocketFollowUpService> logger, IOptions<DocketSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = TimeSpan.FromMinutes(Math.Max(1, settings.Value.CheckIntervalMinutes));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            using var timer = new PeriodicTimer(_interval);
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var logic = scope.ServiceProvider.GetRequiredService<IDocketFollowUpLogic>();
                    var flagged = await logic.SendOverdueAlertsAsync(stoppingToken);
                    if (flagged > 0)
                    {
                        _logger.LogInformation("Sent overdue-feedback alerts for {Count} docket(s).", flagged);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Docket overdue-feedback check failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
