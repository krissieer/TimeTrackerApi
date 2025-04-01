﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TimeTrackerApi.Controllers;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectActivityService;
using TimeTrackerApi.Services.ProjectService;
using TimeTrackerApi.Services.ProjectUserService;

namespace TimeTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService projectService;
        private readonly  IProjectUserService projectUserService;

        public ProjectsController(IProjectService _projectService, IProjectUserService _projectUserService)
        {
            projectService = _projectService;
            projectUserService = _projectUserService;
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
                return Ok(new List<Project>());

            var result = projects.Select(a => new ProjectRequest
            {
                ProjectId = a.Id,
                ProjectName = a.Name,
                ProjectKey = a.AccessKey
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
            var USER = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(USER))
                return Unauthorized("User not authenticated.");
            int user_Id = int.Parse(USER);

            var project = await projectService.AddProject(dto.ProjectName); //добавление пользователя в Projects
            if (project == null)
            {
                return Conflict("Project already exists.");
            }
            var projectUser = await projectUserService.AddProjectUser(user_Id, project.Id, true); //Добавление пользователя в ProjectUsers
            if (projectUser == null)
            {
                return Conflict("Project does not exist or the user is already assigned to this project.");
            }

            return Ok(new { AccessKey = project.AccessKey });
        }

        /// <summary>
        /// Обновление названия проекта
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectRequest dto)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            var isCreator = await projectUserService.IsCreator(userId, dto.ProjectId);
            if (!isCreator)
                return Conflict("You don't have access to edit this project");

            var project = await projectService.UpdateProject(dto.ProjectId, dto.ProjectName);
            if (project == null)
            {
                return NotFound($"Project with ID {dto.ProjectId} not found.");
            }
            var result = new ProjectRequest
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectKey = project.AccessKey
            };
            return Ok(result);
        }

        /// <summary>
        /// Удаление проекта
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpDelete("{projectId}")]
        [Authorize]
        public async Task<ActionResult> DeleteProject(int projectId)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            //var project = await projectService.GetProjectById(projectId);
            //if (project is null)
            //    return NotFound($"Project with ID {projectId} not found.");

            var isCreator = await projectUserService.IsCreator(userId, projectId);
            if (!isCreator)
                return Conflict("Only the project creator can delete the project.");
           
            var success = await projectService.DeleteProject(projectId);
            if (!success)
                StatusCode(500, "Failed to delete project due to server error.");
            return NoContent();
        }
        //Удалить и реадктировать проект может только его создатель
    }
}

public class ProjectRequest
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectKey { get; set; }
}