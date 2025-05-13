using System.Diagnostics;

namespace TimeTrackerApi.Models;

public class User
{
    public int Id { get; set; }
    public long ChatId { get; set; } = 0;
    public string Name { get; set; }
    public string PasswordHash { get; set; }

    public ICollection<Activity> Activities { get; set; }
    public ICollection<ProjectUser> ProjectUsers { get; set; }
    public ICollection<ActivityPeriod> ActivityPeriods { get; set; }
}
