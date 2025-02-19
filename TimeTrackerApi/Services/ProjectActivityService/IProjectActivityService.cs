using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectActivityService
{
    public interface IProjectActivityService
    {
        Task<ProjectActivity> AddProjectActivity(int activityId, string projectId);

        Task<bool> CheckProjectActivity(int activityId, string projectId);

        Task<bool> DeleteProjectActivity(int activityId, string projectId);

        Task<List<ProjectActivity>> GetActivitiesByProjectId(string projectId);

        Task<List<ProjectActivity>> GetProjectsByActivityId(int activityId);
    }
}
