using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers
{
    public class RoadmapsController : BaseApiController
    {
        private readonly DataContext _context;
        public RoadmapsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet] //api/roadmaps
        public async Task<ActionResult<List<Roadmap>>> GetRoadmaps()
        {
            return await _context.Roadmaps.ToListAsync();
        }

        [HttpGet("{id}")] //api/roadmaps/id
        public async Task<ActionResult<Roadmap>> GetRoadmap(Guid id)
        {
            return await _context.Roadmaps.FindAsync(id);
        }
    

    }
}