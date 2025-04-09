namespace TimeTrackerApi.Models;

public class ActivityPeriod
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int ExecutorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public TimeSpan? TotalTime { get; set; }

    public Activity Activity { get; set; }
    public User User { get; set; }
}
