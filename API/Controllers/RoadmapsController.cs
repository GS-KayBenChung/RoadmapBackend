using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Domain.Dtos;
using Serilog;
using System.Text.Json;

namespace API.Controllers
{
    public class RoadmapsController : BaseApiController
    {
        private readonly IConfiguration _config;

        public RoadmapsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardData()
        {
            var result = await Mediator.Send(new DashboardList.Query());
            if (result == null) return NoContent();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedRoadmapResult<Roadmap>>> GetRoadmaps(
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

            Log.Information("Received request for GetRoadmaps with params: filter={Filter}, search={Search}, date={Date}, pageNumber={PageNumber}, pageSize={PageSize}, sortBy={SortBy}, asc={Asc}",
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
            sortBy ??= paginationDefaults.GetValue<string>("DefaultSortByRoadmap");
            asc ??= paginationDefaults.GetValue<int>("DefaultAsc");

            return await Mediator.Send(new List.Query
            {
                Filter = filter,
                Search = search,
                CreatedAfter = date,
                PageNumber = pageNumber.Value,
                PageSize = pageSize.Value,
                SortBy = sortBy,
                Asc = asc.Value
            });
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapDetails(Guid id)
        {
            return await Mediator.Send(new GetDetails.Query { Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoadmap([FromBody] CreateRoadmapDto roadmapDto)
        {
            var command = new Create.Command { RoadmapDto = roadmapDto };
            await Mediator.Send(command);
            return Ok(new { Message = "Roadmap created successfully" });
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> PublishRoadmap(Guid id)
        {
            var command = new Publish.Command { Id = id };
            await Mediator.Send(command);
            return Ok(new { Message = "Roadmap Published successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(Guid id)
        {
            var command = new Delete.Command { Id = id };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }


        [HttpPatch("{id}/roadmap")]
        public async Task<IActionResult> PatchRoadmap(Guid id, [FromBody] RoadmapUpdateDto updateDto)
        {
            await Mediator.Send(new PatchRoadmap.Command { RoadmapId = id, UpdateDto = updateDto });
            return Ok(new { message = "Roadmap and related entities updated successfully." });
        }
        
        [HttpPut("checkboxes/{id}")]
        public async Task<IActionResult> UpdateCheckboxes(Guid id, [FromBody] UpdateCheckedBoxes.Command command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPatch("{entityType}/completion")]
        public async Task<IActionResult> PatchCompletionStatus(string entityType, [FromBody] CompletionStatusDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("The request payload is invalid or missing.");
            }

            try
            {
                await Mediator.Send(new PatchCompletionStatus.Command
                {
                    EntityType = entityType.ToLower(),
                    UpdateDto = updateDto
                });

                return Ok(new { message = $"{entityType} completion status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}