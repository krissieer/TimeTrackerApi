using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
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

        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await projectService.GetProjects() ?? new List<Project>();

            if (!projects.Any())
            {
                return Ok(new List<Project>());
            }

            var result = projects.Select(a => new ProjectRequest
            {
                ProjectId = a.Id,
                ProjectName = a.Name
            });

            return Ok(result);
        }

        /// <summary>
        /// Добавить проект
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProject([FromBody] ProjectRequest dto)
        {
            var project = await projectService.AddProject(dto.ProjectId, dto.ProjectName);
            if (project == null)
            {
                return Conflict("Project already exists.");
            }
            var result = new ProjectRequest
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
            };
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectRequest dto)
        {
            var project = await projectService.UpdateProject(dto.ProjectId, dto.ProjectName);
            if (project == null)
            {
                return NotFound($"Project with ID {dto.ProjectId} not found.");
            }
            var result = new ProjectRequest
            {
                ProjectId = project.Id,
                ProjectName = project.Name
            };
            return Ok(result);
        }

        [HttpDelete("{projectId}")]
        [Authorize]
        public async Task<ActionResult> DeleteProject(string projectId)
        {
            var project = await projectService.CheckProjectIdExistence(projectId);
            if (!project)
                return NotFound($"Project with ID {projectId} not found.");

            var success = await projectService.DeleteProject(projectId);
            if (!success)
                StatusCode(500, "Failed to delete project due to server error.");
            return NoContent();
        }
    }
}

public class ProjectRequest
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
}