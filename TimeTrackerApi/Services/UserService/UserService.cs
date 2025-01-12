using Microsoft.EntityFrameworkCore;
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
    /// Получить количество пользователей
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetUsersNumber()
    {
        var users = await context.Users.ToListAsync();
        return users.Count;
    }

    /// <summary>
    /// Получить пользователя по его UserID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<User> GetUserById(int userId)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Получить пользователя по chatID
    /// </summary>
    /// <param name="chatId"></param>
    /// <returns></returns>
    public async Task<User> GetUserByChatId(long chatId)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
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
                Console.WriteLine("Ошибка: " + ex.Message);
                return null;
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
        var userInBD = await context.Users.FirstOrDefaultAsync(u => u.Name == name);
        if (userInBD == null)
            return string.Empty;

        if (!PasswordHasher.VerifyPassword(password, userInBD.PasswordHash))
            return string.Empty;

        return TokenGeneration.GenerateToken(userInBD.Id);
    }
}