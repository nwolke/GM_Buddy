namespace GM_Buddy.Business;
public static class PasswordHasher
{
    public static string HashPassword(string password, string salt)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return hashedPassword;
    }

    public static string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt();
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
