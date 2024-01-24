using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Mail
{
    public interface IEmailService
    {
        void SendMail(MailContent mailContent);

        void SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
