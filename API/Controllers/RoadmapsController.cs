using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Domain.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using Application.Validator;
using Microsoft.EntityFrameworkCore;

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
            var response = await Mediator.Send(new DashboardList.Query());
            return Ok(response); 
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedRoadmapResult<Roadmap>>> GetRoadmaps(
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
            sortBy = string.IsNullOrEmpty(sortBy) ? paginationDefaults.GetValue<string>("DefaultSortByRoadmap") : sortBy;
            asc = asc == 0 ? paginationDefaults.GetValue<int>("DefaultAsc") : asc;

            return await Mediator.Send(new List.Query 
            {
                Filter = filter,
                Search = search,
                CreatedAfter = date,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Asc = asc
            });
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapDetails(Guid id)
        {
            return await Mediator.Send(new GetDetails.Query { Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoadmap([FromBody] RoadmapDto roadmapDto)
        {
            var validator = new RoadmapValidatorDto();
            var validationResult = await validator.ValidateAsync(roadmapDto);

            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }

                var errorResponse = new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ),
                    traceId = HttpContext.TraceIdentifier
                };

                return BadRequest(errorResponse);
            }
            var command = new Create.Command { RoadmapDto = roadmapDto };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }
      
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(Guid id)
        {
            var command = new Delete.Command { Id = id };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> PublishRoadmap(Guid id)
        {
            var command = new Publish.Command { Id = id };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }

        [HttpPatch("{id}/roadmap")]
        public async Task<IActionResult> PatchRoadmap(Guid id, [FromBody] RoadmapUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Invalid update payload.");
            }

            try
            {
                await Mediator.Send(new PatchRoadmap.Command { RoadmapId = id, UpdateDto = updateDto });
                return Ok(new { message = "Roadmap and related entities updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
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