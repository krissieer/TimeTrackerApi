using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityService
{
    public interface IActivityService
    {
        Task<List<Activity>> GetActivities(int userId, bool activeOnly = true, bool archivedOnly = false);
       
        Task<bool> AddDefaultActivities(int userId);

        Task<Activity> AddActivity(int userId, string name);

        Task<bool> CheckActivityNameExistence(int userId, string name);

        Task<bool> UpdateActivityName(int activityId, string newname);

        Task<bool> DeleteActivity(int activityId);

        Task<int> GetStatusById(int activityId);

        Task<bool> PutActivityInArchive(int activityId);

        Task<bool> RecoverActivity(int activityId);

        Task<bool> ChangeStatus(int activityId, int newStatusId);
    }
}
