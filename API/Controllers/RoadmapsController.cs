using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.AuditActivities;
using Application.Dtos;

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
        public async Task<ActionResult<DashboardStats>> GetDashboardData()
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


        [HttpGet("{id}")] 
        public async Task<ActionResult<Roadmap>> GetRoadmap(Guid id)
        {
            return await Mediator.Send(new Details.Query{ Id = id});
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
            await Mediator.Send(command);
            return Ok(new { message = "Roadmap created successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoadmap(Guid id, [FromBody] UpdateRoadmap.Command command)
        {
            var validator = new UpdateRoadmapValidator();
            var validationResult = await validator.ValidateAsync(command);

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

            try
            {
                command.Id = id;

                await Mediator.Send(command);

                return Ok(new { message = "Roadmap updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "An unexpected error occurred.",
                    status = 500,
                    detail = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(Guid id)
        {
            try
            {
                var command = new Delete.Command { Id = id };
                await Mediator.Send(command);
                return Ok(new { message = "Roadmap deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the roadmap." + ex });
            }
        }

        [HttpPut("checkboxes/{id}")]
        public async Task<IActionResult> UpdateCheckboxes(Guid id, [FromBody] UpdateCheckedBoxes.Command command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
        }


    }
}