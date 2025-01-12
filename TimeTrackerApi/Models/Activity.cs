namespace TimeTrackerApi.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ActiveFrom { get; set; }
        public int UserId { get; set; }
        public int StatusId { get; set; }

        public User User { get; set; }
        public ICollection<ActivityPeriod> ActivityPeriods { get; set; }
        public Status Status { get; set; }
        public ICollection<ProjectActivity> ProjectActivities { get; set; }
    }
}
