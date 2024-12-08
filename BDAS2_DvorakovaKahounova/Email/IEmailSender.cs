namespace BDAS2_DvorakovaKahounova.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string fromEmail, string email, string subject, string message);
    }
}
