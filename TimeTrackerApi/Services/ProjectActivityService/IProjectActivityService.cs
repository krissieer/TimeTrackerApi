using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectActivityService
{
    public interface IProjectActivityService
    {
        Task<ProjectActivity> AddProjectActivity(int activityId, int projectId);

        Task<bool> DeleteProjectActivity(int activityId, int projectId);

        Task<List<ProjectActivity>> GetActivitiesByProjectId(int projectId);

        Task<List<ProjectActivity>> GetProjectsByActivityId(int activityId);
    }
}
