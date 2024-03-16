using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Service
{
    public interface IMailService
    {
        public void SendMail(List<string> mailAddresses, string subject, string body);
    }
    public class MailService : IMailService
    {
        public void SendMail(List<string> mailAddresses, string subject, string body)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("minhduy1511@gmail.com");
                foreach (var mailAddress in mailAddresses)
                {
                    message.To.Add(mailAddress);
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential("minhduy1511@gmail.com", "dxhuwemtdtkobzoj");
                smtpClient.EnableSsl = true;

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send email" + ex.Message);
            }
        }
    }
}
