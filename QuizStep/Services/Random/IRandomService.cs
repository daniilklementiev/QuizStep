namespace QuizStep.Services.RandomService;

public interface IRandomService
{
    String RandomString(int length);
    String ConfirmCode(int length);
    String FileName(int length);
}