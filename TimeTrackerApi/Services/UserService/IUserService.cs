using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.UserService;

public interface IUserService
{
    //Task<List<User>> GetUsersAsync();

    //Task<int> GetUsersNumber();

    Task<List<User>> GetUsers();

    Task<User> GetUserById(int userId);

    Task<User> GetUserByChatId(long chatId);

    Task<bool> CheckUserNameExistence(string name);

    Task<string> Registration(string name, string password, int chatId = 0);

    Task<string> Login(string name, string password);
}
