using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityPeriodService
{
    public interface IActivityPeriodService
    {
        Task<ActivityPeriod> AddActivityPeriod(int activityId);

        Task<ActivityPeriod> UpdateActivityPeriod(int activityPeriodId, DateTime? data1 = null, DateTime? date2 = null);

        Task<int> StartTracking(int activityId);
        
        Task<ActivityPeriod> SetStopTime(int activityId);
       
        Task<DateTime> GetStartTimeById(int activityId);

        //Task<bool> StopTracking(int activityId);

        Task<ActivityPeriod> StopTracking(int activityId);

        Task<bool> DeleteActivityPeriod(int activityPeriodId);
       
        Task<TimeSpan> GetStatistic(int activityId, DateTime? firstdata = null, DateTime? seconddata = null);
    }
}
