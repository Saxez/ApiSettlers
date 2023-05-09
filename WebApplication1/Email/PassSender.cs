using System.Net;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace Project1.Email
{
    public class PassSender
    {
        public static void SendMessage(string adressTo, string messageText, string theme)
        {
            MimeMessage mes = new MimeMessage();
            mes.From.Add(new MailboxAddress(theme, "Settlerreg@yandex.ru"));

            mes.To.Add(MailboxAddress.Parse(adressTo));
            mes.Subject = "Ваш пароль для аккаунта";
            mes.Body = new TextPart("plain")
            {
                Text = messageText
            };

            SmtpClient client = new SmtpClient();

            try
            {
                client.Connect("smtp.yandex.ru", 568, false);

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
