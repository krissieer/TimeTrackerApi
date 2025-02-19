using System.Diagnostics;

namespace TimeTrackerApi.Models
{
    public class ProjectActivity
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string ProjectId { get; set; }

        public Project Project { get; set; }
        public Activity Activity { get; set; }
    }
}
