namespace TimeTrackerApi.Models;

public class Project
{
    public string Id { get; set; }
    public string Name { get; set; }

    public ICollection<ProjectActivity> ProjectActivities { get; set; }
    public ICollection<ProjectUser> ProjectUsers { get; set; }
}
