
using Application.AuditActivities;
using Application.DTOs;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/roadmaps")]
    public class AuditController : BaseApiController
    {
        [HttpGet("logs")]
        public async Task<ActionResult<PaginatedLogResult<RoadmapLogsDto>>> GetLogs(
            [FromQuery] string filter,
            [FromQuery] string search,
            [FromQuery] DateTime? date,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "UpdatedAt",
            [FromQuery] int asc = 1)

        {
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
    }
}