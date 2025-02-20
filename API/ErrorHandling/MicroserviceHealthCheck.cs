using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public class MicroserviceHealthCheck : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<MicroserviceHealthCheck> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly string[] _microserviceUrls;

    public MicroserviceHealthCheck(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<MicroserviceHealthCheck> logger,
        IHostApplicationLifetime appLifetime)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));

        _microserviceUrls = _config.GetSection("Microservices:HealthChecks").Get<string[]>() ?? Array.Empty<string>();

        if (_microserviceUrls.Length == 0)
        {
            //_logger.LogWarning("No microservices configured for health checks. The service will continue running.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_microserviceUrls.Length == 0)
        {
            //_logger.LogInformation("Skipping microservice health checks as no URLs are configured.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var url in _microserviceUrls)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, stoppingToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Microservice {url} is DOWN. Initiating shutdown.");
                        _appLifetime.StopApplication();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to reach {url}. Error: {ex.Message}");
                    _appLifetime.StopApplication();
                    return;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
