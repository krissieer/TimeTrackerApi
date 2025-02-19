using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ProjectService;

namespace TimeTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService projectService;

        public ProjectsController(IProjectService _projectService)
        {
            projectService = _projectService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await projectService.GetProjects();

            if (projects is null)
                return NotFound("Projects not found.");

            return Ok(projects);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProject(string id, string name)
        {
            var project = await projectService.AddProject(id, name);
            if (project == null)
            {
                return BadRequest("Project already exists.");
            }
            return Ok(project);
        }

        [HttpPut]
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

        [HttpDelete]
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
    }
}
