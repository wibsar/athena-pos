using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public class Notification
    {
        /// <summary>
        /// Smpt email notification 
        /// </summary>
        /// <param name="toName"></param>
        /// <param name="toEmailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="fromPassword"></param>
        /// <param name="fromName"></param>
        /// <param name="attachmentFilePath"></param>
        /// <param name="fromEmailAddress"></param>
        /// <returns></returns>
        public static bool SendNotification(string toName, string toEmailAddress, string subject, string body, 
            string attachmentFilePath, string fromEmailAddress, string fromPassword, string fromName = "Wibsar POS")
        {
            if (string.IsNullOrEmpty(toEmailAddress) || string.IsNullOrEmpty(toName) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                throw new ArgumentException();
            }

            try
            {
                var fromAddress = new MailAddress(fromEmailAddress, fromName);
                var toAddress = new MailAddress(toEmailAddress, toName);

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        if (File.Exists(attachmentFilePath))
                        {
                            Attachment data = new Attachment(attachmentFilePath);
                            message.Attachments.Add(data);
                        }
                        smtp.Send(message);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                var exception = e;
                var errorMessage = e.Message;
                return false;
            }
        }
    }
}
