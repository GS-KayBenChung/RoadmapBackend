
using System.Text.Json;
using Application.AuditActivities;
using Domain.Dtos;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers
{
    [Route("api/roadmaps")]
    public class AuditController : BaseApiController
    {
        private readonly IConfiguration _config;

        public AuditController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("logs")]
        public async Task<ActionResult<PaginatedLogResult<RoadmapLogsDto>>> GetLogs(
            [FromQuery] Dictionary<string, string> queryParams,
            [FromQuery] string filter,
            [FromQuery] string search,
            [FromQuery] DateTime? date,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string sortBy,
            [FromQuery] int? asc)

        {
            var allowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "filter", "search", "date", "pagenumber", "pagesize", "sortby", "asc"
            };

            var invalidParams = queryParams.Keys
                                           .Where(key => !allowedKeys.Contains(key))
                                           .ToList();

            if (invalidParams.Any())
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = new { InvalidParams = new[] { $"Unexpected query parameter(s): {string.Join(", ", invalidParams)}" } }
                });
            }

            Log.Information("Received request for GetLogs with params: filter={Filter}, search={Search}, date={Date}, pageNumber={PageNumber}, pageSize={PageSize}, sortBy={SortBy}, asc={Asc}",
                filter, search, date, pageNumber, pageSize, sortBy, asc);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(ms => ms.Value.Errors.Count > 0)
                                       .ToDictionary(
                                           kvp => kvp.Key,
                                           kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                                        );

                Log.Warning("Validation failed: {Errors}", JsonSerializer.Serialize(errors));

                return BadRequest(new
                {
                    message = "Validation failed",
                    errors
                });
            }

            var paginationDefaults = _config.GetSection("PaginationDefaults");

            pageNumber ??= paginationDefaults.GetValue<int>("DefaultPageNumber");
            pageSize ??= paginationDefaults.GetValue<int>("DefaultPageSize");
            sortBy ??= paginationDefaults.GetValue<string>("DefaultSortByAudit");
            asc ??= paginationDefaults.GetValue<int>("DefaultAsc");

            return await Mediator.Send(new GetLogs.Query
            {
                Filter = filter,
                Search = search,
                CreatedOn = date,
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                SortBy = sortBy,
                Asc = asc.Value
            });
        }

        [HttpPost("Createlogs")]
        public async Task<IActionResult> CreateLogs([FromBody] RoadmapLogsDto roadmapLogsDto)
        {
            var command = new CreateLogs.Command { RoadmapLogsDto = roadmapLogsDto };
            await Mediator.Send(command);
            return Ok(new { message = "Log created successfully."});
        }
    }
}