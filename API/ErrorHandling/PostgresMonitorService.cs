using Microsoft.Extensions.Diagnostics.HealthChecks;

public class PostgresMonitorService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public PostgresMonitorService(
        HealthCheckService healthCheckService,
        IHostApplicationLifetime applicationLifetime)
    {
        _healthCheckService = healthCheckService;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var report = await _healthCheckService.CheckHealthAsync(stoppingToken);

            if (report.Status == HealthStatus.Unhealthy)
            {
                Console.WriteLine("Postgres is down. Initiating shutdown...");
                _applicationLifetime.StopApplication();
                break;
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
