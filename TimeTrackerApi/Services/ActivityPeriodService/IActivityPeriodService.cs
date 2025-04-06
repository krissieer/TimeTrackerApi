using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityPeriodService
{
    public interface IActivityPeriodService
    {
        Task<ActivityPeriod?> GetActivityPeriodById(int activityPeriodId);

        Task<ActivityPeriod> AddActivityPeriod(int activityId);

        Task<List<ActivityPeriod>> UpdateActivityPeriod(int activityPeriodId, DateTime? data1 = null, DateTime? date2 = null);

        Task<ActivityPeriod> StartTracking(int activityId);

        Task<List<ActivityPeriod>> SetStopTime(int activityId);
       
        Task<DateTime> GetStartTimeById(int activityId);

        Task<List<ActivityPeriod>> StopTracking(int activityId);

        Task<bool> DeleteActivityPeriod(int activityPeriodId);

        Task<List<ActivityPeriod>> GetStatistic(int activityId, DateTime? firstdata = null, DateTime? seconddata = null);
    }
}
