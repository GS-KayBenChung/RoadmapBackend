using Application.RoadmapActivities;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;

namespace API.Controllers
{
    public class RoadmapsController : BaseApiController
    {
        [HttpGet] //api/roadmaps
        public async Task<ActionResult<List<Roadmap>>> GetRoadmaps()
        {
            return await Mediator.Send(new List.Query());
        }
        
        [HttpGet("{id}")] //api/roadmaps/id
        public async Task<ActionResult<Roadmap>> GetRoadmap(Guid id)
        {
            return await Mediator.Send(new Details.Query{ Id = id});
        }

        [HttpGet("details/{id}")] // api/roadmaps/details/{id} NEW GET DETAILS
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapDetails(Guid id)
        {
            return await Mediator.Send(new GetDetails.Query { Id = id });
        }

        [HttpPost] // api/roadmaps
        public async Task<IActionResult> CreateRoadmap([FromBody] RoadmapDto roadmapDto)
        {
            if (roadmapDto == null)
            {
                return BadRequest("Invalid roadmap data.");
            }

            await Mediator.Send(new Create.Command { RoadmapDto = roadmapDto });

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditRoadmap(Guid id, [FromBody] RoadmapDto roadmapDto)
        {
            if (roadmapDto == null)
            {
                return BadRequest("Invalid roadmap data.");
            }
            await Mediator.Send(new Edit.Command { RoadmapId = id, RoadmapDto = roadmapDto });
            return Ok();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(Guid id)
        {
            await Mediator.Send(new Delete.Command { Id = id });
            return Ok();
        }

    }
}