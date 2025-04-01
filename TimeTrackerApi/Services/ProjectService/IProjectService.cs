using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectService
{
    public interface IProjectService
    {
        Task<List<Project>> GetProjects();

        Task<Project> GetProjectById(int Id);

        Task<Project> AddProject(string name);

        string GenerateKey();

        Task<Project> UpdateProject(int id, string newName);

        Task<bool> DeleteProject(int id);
    }
}
