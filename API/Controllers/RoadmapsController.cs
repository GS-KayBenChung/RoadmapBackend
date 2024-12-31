using Application.RoadmapActivities;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace API.Controllers
{
    public class RoadmapsController : BaseApiController
    {
        [HttpGet] 
        public async Task<ActionResult<List<Roadmap>>> GetRoadmaps([FromQuery] string filter, [FromQuery] string search, [FromQuery] DateTime? createdAfter)
        {
            return await Mediator.Send(new List.Query { Filter = filter, Search = search, CreatedAfter = createdAfter });
        }
        [HttpGet("{id}")] 
        public async Task<ActionResult<Roadmap>> GetRoadmap(Guid id)
        {
            return await Mediator.Send(new Details.Query{ Id = id});
        }

        [HttpGet("logs")]
        public async Task<ActionResult<List<RoadmapLogsDto>>> GetLogs([FromQuery] string filter, [FromQuery] string search)
        {
            return await Mediator.Send(new GetLogs.Query { Filter = filter, Search = search });
        }

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