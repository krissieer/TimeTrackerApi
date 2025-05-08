using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityService
{
    public interface IActivityService
    {
        Task<List<Activity>> GetActivities(int userId, bool activeOnly = true, bool inProcessOnly = false, bool archivedOnly = false);

        Task<Activity?> GetActivityById(int activityId);

        Task<Activity> AddActivity(int userId, string name);

        Task<bool> CheckActivityNameExistence(int userId, string name);

        Task<bool> UpdateActivityName(int activityId, string newname);

        Task<bool> DeleteActivity(int activityId);

        Task<bool> ChangeStatus(int activityId, int newStatusId);
    }
}
