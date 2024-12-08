
using System.Net;
using System.Net.Mail;

namespace BDAS2_DvorakovaKahounova.Email
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string fromEmail, string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {   
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("utuleklucky@gmail.com", "inrgqgawrdknqjok")

            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("utuleklucky@gmail.com", "Psí útulek Lucky"), // Autentizovaná adresa
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email); // Příjemce
            mailMessage.ReplyToList.Add(new MailAddress(fromEmail)); // Uživatelova adresa jako odpovědní

            return client.SendMailAsync(mailMessage);

            //return client.SendMailAsync(
            //    new MailMessage(from: fromEmail,
            //                    to: "utuleklucky@gmail.com",
            //                    subject,
            //                    message
            //                    ));

        }
    }
}
