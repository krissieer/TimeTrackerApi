using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.UserService;

public interface IUserService
{
    Task<List<User>> GetUsers();

    Task<User> GetUserById(int id);

    Task<User> GetUserByChatId(long chatId);

    Task<bool> CheckUserNameExistence(string name);

    Task<string> Registration(string name, string password, int chatId = 0);

    Task<string> Login(string name, string password);

    Task<string> LoginByChatId(long chatId);

    Task<bool> UpdateUser(int userId, string? newName = null, string? newPassword = null);

    Task<bool> DeleteUser(int id);
}
