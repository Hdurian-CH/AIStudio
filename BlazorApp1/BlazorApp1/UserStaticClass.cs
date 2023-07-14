using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1;

public static class UserStaticClass
{
    public static string UserName { get; set; } = "test";
    public static string UserPassword { get; set; } = HashPassword("sk-123456");

    public static string Role { get; set; } = "User";

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "HzMLt-cHitWP4qMd"));
        return Convert.ToBase64String(hashedBytes);
    }
}