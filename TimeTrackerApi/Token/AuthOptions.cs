using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TimeTrackerApi.Token;

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer";
    public const string AUDIENCE = "MyAuthClient"; 
    const string KEY = "mysupersecret_secretsecretsecretkey!123";   
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
