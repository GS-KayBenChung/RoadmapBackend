
using Application.AuditActivities;
using Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

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
            [FromQuery] string filter,
            [FromQuery] string search,
            [FromQuery] DateTime? date,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string sortBy,
            [FromQuery] int asc)

        {

            var paginationDefaults = _config.GetSection("PaginationDefaults");

            pageNumber = pageNumber <= 0 ? paginationDefaults.GetValue<int>("DefaultPageNumber") : pageNumber;
            pageSize = pageSize <= 0 ? paginationDefaults.GetValue<int>("DefaultPageSize") : pageSize;
            sortBy = string.IsNullOrEmpty(sortBy) ? paginationDefaults.GetValue<string>("DefaultSortByAudit") : sortBy;
            asc = asc == 0 ? paginationDefaults.GetValue<int>("DefaultAsc") : asc;


            return await Mediator.Send(new GetLogs.Query
            {
                Filter = filter,
                Search = search,
                CreatedOn = date,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Asc = asc
            });
        }

        [HttpPost("Createlogs")]
        public async Task<IActionResult> CreateLogs([FromBody] RoadmapLogsDto roadmapLogsDto)
        {
            var command = new CreateLogs.Command
            {
                RoadmapLogsDto = roadmapLogsDto
            };
            await Mediator.Send(command);

            return Ok(new { message = "Log created successfully." });
        }
    }
}