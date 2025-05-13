using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectUserService;

public interface IProjectUserService
{
    Task<ProjectUser> AddProjectUser(int userId, int projectId, bool isCreator);

    Task<ProjectUser> ConnectToProject(int userId, string accessKey);

    Task<bool> DeleteProjectUser(int userId, int projectId);

    Task<List<ProjectUser>> GetProjectsByUserId(int userId);

    Task<List<ProjectUser>> GetUsersByProjectId(int projectId);

    Task<bool> IsCreator(int userId, int projectId);
}
