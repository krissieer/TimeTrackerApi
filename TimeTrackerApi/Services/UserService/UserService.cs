using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using TimeTrackerApi.Models;
using TimeTrackerApi.Token;

namespace TimeTrackerApi.Services.UserService;

public class UserService:IUserService
{
    private readonly TimeTrackerDbContext context;

    public UserService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Получть список пользователй
    /// </summary>
    /// <returns></returns>
    public async Task<List<User>> GetUsers()
    {
        return await context.Users.ToListAsync();
    }

    /// <summary>
    /// Проверка имени на существование
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> CheckUserNameExistence(string name)
    {
        return await context.Users.AnyAsync(u => u.Name.Equals(name));
    }

    /// <summary>
    /// Получить пользователя по его ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User> GetUserById(int id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return null;
        return user;
    }

    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="chatId"></param>
    /// <returns></returns>
    public async Task<string> Registration(string name, string password, int chatId = 0)
    {
        var hashedPassword = PasswordHasher.HashPassword(password);
        bool isExist = await CheckUserNameExistence(name); 
        if (!isExist)
        {
            try
            {
                var user = new User
                {
                    Name = name,
                    PasswordHash = hashedPassword,
                    ChatId = chatId,
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return TokenGeneration.GenerateToken(user.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"registration error: {ex.Message}");
            }
           
        }
        return string.Empty;
    }

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<string> Login(string name, string password)
    {
        try
        {
            var userInBD = await context.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (userInBD == null)
                return string.Empty;

            if (!PasswordHasher.VerifyPassword(password, userInBD.PasswordHash))
                return string.Empty;

            return TokenGeneration.GenerateToken(userInBD.Id);
        }
        catch(Exception ex)
        {
            throw new Exception($"Login error: {ex.Message}");
        }
    }

    public async Task<bool> UpdateUser(int userId, string? newName = null, string? newPassword = null)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        if (newName is not null)
        {
            bool isExist = await CheckUserNameExistence(newName);
            if (!isExist)
            {
                user.Name = newName;
            }
        }
        if (newPassword is not null)
        {
            var hashedPassword = PasswordHasher.HashPassword(newPassword);
            user.PasswordHash = hashedPassword;
        }
        return await context.SaveChangesAsync() >= 1;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found.");
        context.Users.Remove(user);
        return await context.SaveChangesAsync() >= 1;
    }
}