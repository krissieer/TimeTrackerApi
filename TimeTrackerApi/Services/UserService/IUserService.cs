using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.UserService;

public interface IUserService
{
    Task<List<User>> GetUsers();

    Task<bool> CheckUserNameExistence(string name);

    Task<string> Registration(string name, string password, int chatId = 0);

    Task<string> Login(string name, string password);
}
