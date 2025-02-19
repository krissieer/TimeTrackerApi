namespace TimeTrackerApi.Models
{
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        //public int CreatorId { get; set; } //id пользователя - создателя проекта

        public ICollection<ProjectActivity> ProjectActivities { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; }
    }
}
