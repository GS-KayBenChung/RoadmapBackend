using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.AuditActivities;
using Application.Dtos;
using System.Security.Claims;
using Application.Dto;
using Domain.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

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
            //var userIdFormatted = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (!Guid.TryParse(userIdFormatted, out Guid userId))
            //{
            //    return Unauthorized("Unauthorized User");
            //}

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

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateRoadmap(Guid id, [FromBody] UpdateRoadmap.Command command)
        //{
        //    var validator = new UpdateRoadmapValidator();
        //    var validationResult = await validator.ValidateAsync(command);

        //    if (!validationResult.IsValid)
        //    {
        //        foreach (var failure in validationResult.Errors)
        //        {
        //            ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
        //        }

        //        var errorResponse = new
        //        {
        //            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        //            title = "One or more validation errors occurred.",
        //            status = 400,
        //            errors = ModelState.ToDictionary(
        //                kvp => kvp.Key,
        //                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        //            ),
        //            traceId = HttpContext.TraceIdentifier
        //        };

        //        return BadRequest(errorResponse);
        //    }

        //    try
        //    {
        //        command.Id = id;

        //        await Mediator.Send(command);

        //        return Ok(new { message = "Roadmap updated successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        //            title = "An unexpected error occurred.",
        //            status = 500,
        //            detail = ex.Message,
        //            traceId = HttpContext.TraceIdentifier
        //        });
        //    }
        //}

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateRoadmap(Guid id, [FromBody] JsonPatchDocument<Edit.Command> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest(new { message = "Invalid patch document." });
            }

            Console.WriteLine($"Patch Doc From Body: {JsonSerializer.Serialize(patchDocument)}");


            var roadmap = await Mediator.Send(new GetDetails.Query { Id = id });

            if (roadmap == null)
            {
                return NotFound(new { message = $"Roadmap with ID '{id}' not found." });
            }

            var updateCommand = new Edit.Command
            {
                Id = id,
                Title = roadmap.Title,
                Description = roadmap.Description,
                IsDraft = roadmap.IsDraft,
                Milestones = roadmap.Milestones
            };


            Console.WriteLine($"Update Command Before: {JsonSerializer.Serialize(updateCommand)}");

            patchDocument.ApplyTo(updateCommand, error =>
            {
                Console.WriteLine($"Patch Error: {error.ErrorMessage} at {error.AffectedObject}");
                ModelState.AddModelError(error.AffectedObject.ToString(), error.ErrorMessage);
            });

            Console.WriteLine($"Update Command After: {JsonSerializer.Serialize(updateCommand)}");


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Mediator.Send(updateCommand);

            return Ok(new { message = "Roadmap updated successfully." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(Guid id)
        {
            var command = new Delete.Command { Id = id };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }

        [HttpPut("checkboxes/{id}")]
        public async Task<IActionResult> UpdateCheckboxes(Guid id, [FromBody] UpdateCheckedBoxes.Command command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return Ok();
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> PublishRoadmap(Guid id)
        {
            var command = new Publish.Command { Id = id };
            StatusDto status = await Mediator.Send(command);
            return Ok(status);
        }




    }
}