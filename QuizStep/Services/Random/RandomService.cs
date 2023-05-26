namespace QuizStep.Services.RandomService;

public class RandomService : IRandomService
{
    private readonly System.Random _random = new();
    private readonly String _codeChars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private readonly String _safeChars = new string(
        Enumerable.Range(20, 107)
            .Select(i => (char)i)
            .ToArray()
    );
    private readonly String _fileNameChars = "abcdefghijklmnopqrstuvwxyz0123456789-_=!?@#$%^&*()";
    public string RandomString(int length)
    {
        return _MakeString(_safeChars, length);
    }

    public string ConfirmCode(int length)
    {
        return _MakeString(_codeChars, length);
    }

    public string FileName(int length)
    {
        return _MakeString(_fileNameChars, length);
    }

    private string _MakeString(String sourceString, int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = sourceString[_random.Next(sourceString.Length)];
        }

        return new string(chars);
    }
}