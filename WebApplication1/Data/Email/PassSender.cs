using System.Net;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text;

namespace Project1.Email
{
    public class PassSender
    {
        public static void SendMessage(string adressTo, string messageText, int mode)
        {
            MimeMessage mes = new MimeMessage();
            if (mode == 1)
            {
                mes.From.Add(new MailboxAddress("Регистрация в системе", "Settlerreg@yandex.ru"));
            }
            if (mode == 2)
            {
                mes.From.Add(new MailboxAddress("Восстановление пароля", "Settlerreg@yandex.ru"));
            }
            mes.To.Add(MailboxAddress.Parse(adressTo));
            
            if (mode == 1)
            {
                mes.Subject = "Ваш пароль для аккаунта";
            }
            if (mode == 2)
            {
                mes.Subject = "Код для сброса пароля в системе";
            }
            if (mode == 1)
            {
                mes.Body = new TextPart("plain")
                {
                    Text = "Ваш пароль: " + messageText
                };
            }
            if(mode == 2) 
            {
                mes.Body = new TextPart("plain")
                {
                    Text = "Код для сброса пароля: " + messageText + ". Действителен в течении 5 минут"
                };
            }
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
