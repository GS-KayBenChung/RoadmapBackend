using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.AuditActivities;

namespace API.Controllers
{
    public class RoadmapsController : BaseApiController
    {

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
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "UpdatedAt",
            [FromQuery] int asc = 1)

        {
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

        //[HttpGet("logs")]
        //public async Task<ActionResult<PaginatedLogResult<AuditLog>>> GetLogs(
        //    [FromQuery] string filter,
        //    [FromQuery] string search,
        //    [FromQuery] DateTime? date,
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string sortBy = "UpdatedAt",
        //    [FromQuery] int asc = 1)

        //{
        //    return await Mediator.Send(new GetLogs.Query
        //    {
        //        Filter = filter,
        //        Search = search,
        //        CreatedAfter = date,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize,
        //        SortBy = sortBy,
        //        Asc = asc
        //    });
        //}

        [HttpGet("details/{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapDetails(Guid id)
        {
            return await Mediator.Send(new GetDetails.Query { Id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoadmap([FromBody] RoadmapDto roadmapDto)
        {
            try
            {
                var command = new Create.Command { RoadmapDto = roadmapDto };
                await Mediator.Send(command);
                return Ok(new { message = "Roadmap created successfully." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoadmap(Guid id, UpdateRoadmap.Command command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return NoContent();
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
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the roadmap." });
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