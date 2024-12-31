using Microsoft.AspNetCore.Mvc;
using Serilog;

public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("This is an informational message");
        return Ok("Hello World!");
    }
}