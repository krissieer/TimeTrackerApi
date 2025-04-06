using System.Diagnostics;

namespace TimeTrackerApi.Models;

public class Status
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Activity> Activities { get; set; }
}
