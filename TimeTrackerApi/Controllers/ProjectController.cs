using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ProjectService;

namespace TimeTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService projectService;

        public ProjectController(IProjectService _projectService)
        {
            projectService = _projectService;
        }

        [HttpPost("add/{id}/{name}")]
        [Authorize]
        public async Task<IActionResult> AddProject(string id, string name)
        {
            var project = await projectService.AddProject(id, name);
            if (project == null)
            {
                return BadRequest("Project with the given ID already exists.");
            }
            return Ok(project);
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProject(string id, string newName)
        {
            var project = await projectService.UpdateProject(id, newName);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            return Ok(project);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(string id)
        {
            var success = await projectService.DeleteProject(id);
            if (!success)
            {
                return NotFound("Project not found.");
            }
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProjectNameById(string id)
        {
            var projectName = await projectService.GetProjectNameById(id);
            if (string.IsNullOrEmpty(projectName))
                return NotFound("Project not found.");

            return Ok(projectName);
        }
    }
}
