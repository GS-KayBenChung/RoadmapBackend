using Microsoft.Extensions.Hosting;

public class GracefulShutdownService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<GracefulShutdownService> _logger;

    public GracefulShutdownService(IHostApplicationLifetime lifetime, ILogger<GracefulShutdownService> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStopping.Register(OnStopping);
        return Task.CompletedTask;
    }

    private void OnStopping()
    {
        _logger.LogInformation("Application is shutting down...");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
