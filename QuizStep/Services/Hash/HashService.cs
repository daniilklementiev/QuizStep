namespace QuizStep.Services.Hash;

public class HashService : IHashService
{
    public string Hash(string text)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text)));
    }
}