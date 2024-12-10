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

        [HttpPost] // api/roadmaps
        public async Task<IActionResult> CreateRoadmap([FromBody] RoadmapDto roadmapDto)
        {
            if (roadmapDto == null)
            {
                return BadRequest("Invalid roadmap data.");
            }

            // Send the DTO to MediatR to create the roadmap and related entities
            await Mediator.Send(new Create.Command { RoadmapDto = roadmapDto });

            return Ok();
        }
        //[HttpPost]
        //public async Task<IActionResult> CreateRoadmap(Roadmap roadmap)
        //{
        //    await Mediator.Send(new Create.Command { Roadmap = roadmap });
        //    return Ok();
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> EditRoadmap(Guid id, Roadmap roadmap)
        {
            roadmap.RoadmapId = id;
            await Mediator.Send(new Edit.Command { Roadmap = roadmap });
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