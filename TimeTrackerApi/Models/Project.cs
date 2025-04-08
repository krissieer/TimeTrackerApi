namespace TimeTrackerApi.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AccessKey { get; set; }
    public DateTime CreationDate{ get; set; }
    public DateTime? FinishDate { get; set; } = null;

    public ICollection<ProjectActivity> ProjectActivities { get; set; }
    public ICollection<ProjectUser> ProjectUsers { get; set; }
}
