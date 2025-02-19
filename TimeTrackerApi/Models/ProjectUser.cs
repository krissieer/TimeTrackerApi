
namespace TimeTrackerApi.Models;

public class ProjectUser
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ProjectId { get; set; }
    public bool Creator { get; set; }

    public Project Project { get; set; }
    public User User { get; set; }
}
