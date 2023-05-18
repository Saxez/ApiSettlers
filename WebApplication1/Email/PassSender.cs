using System.Net;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text;

namespace Project1.Email
{
    public class PassSender
    {
        public static void SendMessage(string adressTo, string messageText, string theme)
        {
            MimeMessage mes = new MimeMessage();
            mes.From.Add(new MailboxAddress("Регистрация в системе", "Settlerreg@yandex.ru"));
            mes.To.Add(MailboxAddress.Parse(adressTo));
            mes.Subject = "Your password to account";
            mes.Body = new TextPart("plain")
            {
                Text = messageText
            };

            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.yandex.ru", 465, true);

                client.Authenticate("Settlerreg@yandex.ru", "sjwzkzwrmwmdfniu");
                client.Send(mes);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

    }
}
