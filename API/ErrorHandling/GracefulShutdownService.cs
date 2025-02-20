public class GracefulShutdownService : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<GracefulShutdownService> _logger;

    public GracefulShutdownService(IHostApplicationLifetime appLifetime, ILogger<GracefulShutdownService> logger)
    {
        _appLifetime = appLifetime;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _appLifetime.ApplicationStopping.Register(OnShutdown);
        return Task.CompletedTask;
    }

    private void OnShutdown()
    {
        _logger.LogWarning("Application is shutting down gracefully...");
        Console.WriteLine("Application is shutting down gracefully...");
    }
}