using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityPeriodService
{
    public interface IActivityPeriodService
    {
        Task<ActivityPeriod?> GetActivityPeriodById(int activityPeriodId);

        Task<ActivityPeriod> AddActivityPeriod(int activityId, int userId);

        Task<List<ActivityPeriod>> UpdateActivityPeriod(int activityPeriodId, int userId, DateTime? data1 = null, DateTime? date2 = null);

        Task<ActivityPeriod> StartTracking(int activityId, int userId);

        Task<List<ActivityPeriod>> SetStopTime(int activityId, int userId);
       
        Task<DateTime> GetStartTimeById(int activityId, int userId);

        Task<List<ActivityPeriod>> StopTracking(int activityId, int userId);

        Task<bool> DeleteActivityPeriod(int activityPeriodId);

        Task<List<ActivityPeriod>> GetStatistic(int activityId = 0, int userId = 0, DateTime? firstdata = null, DateTime? seconddata = null);
    }
}
