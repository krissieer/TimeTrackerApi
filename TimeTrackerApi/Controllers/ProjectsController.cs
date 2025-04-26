using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Security.Claims;
using TimeTrackerApi.Controllers;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ProjectActivityService;
using TimeTrackerApi.Services.ProjectService;
using TimeTrackerApi.Services.ProjectUserService;
using TimeTrackerApi;

namespace TimeTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService projectService;
        private readonly IProjectUserService projectUserService;
        private readonly IProjectActivityService projectActivityService;

        public ProjectsController(IProjectService _projectService, IProjectUserService _projectUserService, IProjectActivityService _projectActivityService)
        {
            projectService = _projectService;
            projectUserService = _projectUserService;
            projectActivityService = _projectActivityService;
        }

        /// <summary>
        /// Получить все проекты
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetProjects([FromQuery] bool current = true)
        {
            var projects = await projectService.GetProjects(current) ?? new List<Project>();

            if (!projects.Any())
                return Ok(new List<Project>());

            var result = projects.Select(a => new ProjectDto
            {
                projectId = a.Id,
                projectName = a.Name,
                projectKey = a.AccessKey,
                creationDate = a.CreationDate,
                finishDate = a.FinishDate,
            });

            return Ok(result);
        }

        /// <summary>
        /// Получить данные по проекту
        /// </summary>
        /// <returns></returns>
        [HttpGet("{projectId}")]
        [Authorize]
        public async Task<ActionResult> GetProjectById(int projectId)
        {
            var project = await projectService.GetProjectById(projectId);

            if (project is null)
                return NotFound($"Project with ID {projectId} not found.");

            var result = new ProjectDto
            {
                projectId = project.Id,
                projectName = project.Name,
                projectKey = project.AccessKey,
                creationDate = project.CreationDate,
                finishDate = project.FinishDate,
            };

            return Ok(result);
        }

        /// <summary>
        /// Добавить проект
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddProject([FromBody] AddProjectDto dto)
        {
            var USER = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(USER))
                return Unauthorized("User not authenticated.");
            int user_Id = int.Parse(USER);

            var project = await projectService.AddProject(dto.projectName); //добавление пользователя в Projects
            if (project == null)
            {
                return BadRequest();
            }
            var projectUser = await projectUserService.AddProjectUser(user_Id, project.Id, true); //Добавление пользователя в ProjectUsers
            if (projectUser == null)
            {
                return Conflict("Project does not exist or the user is already assigned to this project.");
            }

            return Ok(new { AccessKey = project.AccessKey });
        }

        /// <summary>
        /// Обновление проекта: обновить название, закрыть проект
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{projectId}")]
        [Authorize]
        public async Task<ActionResult> UpdateProject([FromBody] EditProjectDto dto, int projectId)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            var isCreator = await projectUserService.IsCreator(userId, projectId);
            if (!isCreator)
                return Conflict("You don't have access to edit this project");

            if (dto.closeProject)
            {
                bool closed = await projectService.CloseProject(projectId);
                if (closed)
                    return Ok(closed);
                else
                    return BadRequest("Failed to close project");
            }

            if (dto.updateName)
            {
                var project = await projectService.UpdateProject(projectId, dto.projectName);
                if (project == null)
                {
                    return NotFound($"Project with ID {projectId} not found.");
                }
                var result = new ProjectDto
                {
                    projectId = project.Id,
                    projectName = project.Name,
                    projectKey = project.AccessKey,
                    creationDate = project.CreationDate,
                    finishDate = project.FinishDate,
                };
                return Ok(result);
            }
            return BadRequest("No update action was specified.");
        }

        /// <summary>
        /// Удаление проекта
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpDelete("{projectId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            var isCreator = await projectUserService.IsCreator(userId, projectId);
            if (!isCreator)
                return Conflict("Only the project creator can delete the project.");
           
            var success = await projectService.DeleteProject(projectId);
            if (!success)
                StatusCode(500, "Failed to delete project due to server error.");
            return NoContent();
        }

        /// <summary>
        /// Получить пользователй проекта
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/users")]
        [Authorize]
        public async Task<ActionResult> GetProjectUsers(int projectId)
        {
            var users = await projectUserService.GetUsersByProjectId(projectId);
            if (!users.Any())
            {
                return NotFound($"No users found for project with ID {projectId}");
            }
            var result = users.Select(a => new ProjectUserDto
            {
                id = a.Id,
                projectId = a.ProjectId,
                userId = a.UserId,
                isCreator = a.Creator
            });

            return Ok(result);
        }

        /// <summary>
        /// Добавить пользователя в проект
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <param name="isCreator"></param>
        /// <returns></returns>
        [HttpPost("user")]
        [Authorize]
        public async Task<ActionResult> AddProjectUser([FromBody] AddProjectUserDto dto)
        {
            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUser))
                return Unauthorized("User not authenticated.");
            int authorizedUserId = int.Parse(currentUser);

            var isCreator = await projectUserService.IsCreator(authorizedUserId, dto.projectId);
            if (!isCreator && dto.userId != authorizedUserId)
                return Conflict("You don't have access to add user in project.");

            var user = await projectUserService.AddProjectUser(dto.userId, dto.projectId, false);
            if (user == null)
            {
                return Conflict("Project does not exist or the user is already assigned to this project.");
            }
            return Ok(new ProjectUserDto
            {
                id = user.Id,
                userId = user.UserId,
                projectId = user.ProjectId,
            });
        }

        /// <summary>
        /// Удалить пользователя из проекта
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpDelete("user/{projectId}/{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProjectUser(int projectId, int userId)
        {
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(user))
                    return Unauthorized("User not authenticated.");
                int authorizedUserId = int.Parse(user);

                var isCreator = await projectUserService.IsCreator(authorizedUserId, projectId);
                if (!isCreator && userId != authorizedUserId)
                    return Conflict("You don't have access to delete this user.");

                var success = await projectUserService.DeleteProjectUser(userId, projectId);
                if (!success)
                    return StatusCode(500, "Failed to delete activity from project due to server error.");
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }


        /// <summary>
        /// Получить активности опредленного проекта
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/activities")]
        [Authorize]
        public async Task<ActionResult> GetProjectActivities(int projectId)
        {
            var activities = await projectActivityService.GetActivitiesByProjectId(projectId);
            if (!activities.Any() || activities == null)
            {
                return NotFound("No activities found for this project ");
            }
            var result = activities.Select(a => new ProjectActivityDto
            {
                id = a.Id,
                activityId = a.ActivityId,
                projectId = a.ProjectId
            });
            return Ok(result);
        }

        /// <summary>
        /// Добавить Активность в Проект
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost("activity")]
        [Authorize]
        public async Task<ActionResult> AddProjectActivity([FromBody] AddProjectActivityDto dto)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            var isCreator = await projectUserService.IsCreator(userId, dto.projectId);
            if (!isCreator)
                return Conflict("You don't have access to edit this project");

            var result = await projectActivityService.AddProjectActivity(dto.activityId, dto.projectId);
            if (result == null)
                return Conflict("Activity already exists for this project or project does not exist");

            var response = new ProjectActivityDto
            {
                id = result.Id,
                activityId = result.ActivityId,
                projectId = result.ProjectId
            };
            return Ok(response);
        }

        [HttpDelete("activity/{projectId}/{activityId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProjectActivity(int projectId, int activityId)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
                return Unauthorized("User not authenticated.");
            int userId = int.Parse(userID);

            var isCreator = await projectUserService.IsCreator(userId, projectId);
            if (!isCreator)
                return Conflict("You don't have access to delete this activity.");

            var result = await projectActivityService.DeleteProjectActivity(activityId, projectId);
            if (!result)
                return StatusCode(500, "Failed to delete activity from project due to server error.");
            return NoContent();
        }

    }
}

public class ProjectDto
{
    public int projectId { get; set; }
    public string projectName { get; set; }
    public string projectKey { get; set; }
    public DateTime? creationDate { get; set; }
    public DateTime? finishDate { get; set; }
}

public class EditProjectDto
{
    public bool closeProject { get; set; } = false;
    public bool updateName { get; set; } = false;
    public string projectName { get; set; }
}

public class AddProjectDto
{
    public string projectName { get; set; }
}

public class ProjectUserDto
{
    public int id { get; set; }
    public int userId { get; set; }
    public int projectId { get; set; }
    public bool isCreator { get; set; }
}

public class AddProjectUserDto
{
    public int userId { get; set; }
    public int projectId { get; set; }
}

public class ProjectActivityDto
{
    public int id { get; set; }
    public int activityId { get; set; }
    public int projectId { get; set; }
}

public class AddProjectActivityDto
{
    public int activityId { get; set; }
    public int projectId { get; set; }
}
