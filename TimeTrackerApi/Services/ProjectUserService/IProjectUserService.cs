using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectUserService
{
    public interface IProjectUserService
    {
        Task<ProjectUser> AddProjectUser(int userId, string projectId);

        Task<bool> CheckProjectIdExistence(string id);

        Task<bool> CheckProjectUser(int userId, string projectId);

        Task<bool> DeleteProjectUser(int userId, string projectId);

        Task<List<ProjectUser>> GetProjectsByUserId(int userId);

        Task<List<ProjectUser>> GetUsersByProjectId(string projectId);
    }
}
