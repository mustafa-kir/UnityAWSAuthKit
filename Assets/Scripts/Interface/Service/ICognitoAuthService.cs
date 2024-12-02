using System.Threading.Tasks;

public interface ICognitoAuthService
{
    Task<bool> SignInAsync(string email, string password);
    Task<string> GetUserEmailAsync();
    Task<bool> SignUpAsync(string nickname, string email, string password);
    Task<bool> ForgotPasswordAsync(string email);
}