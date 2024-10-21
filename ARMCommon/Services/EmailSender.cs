using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using MailKit.Security;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ARMCommon.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly Utils _common;
        public EmailSender(EmailConfiguration emailConfig, Utils common)
        {
            _emailConfig = emailConfig;
            _common = common;
        }

        public async Task SendEmailAsync(Message message)
        {
            IEmailSender emailSender;

            if (_emailConfig.Server == "Gmail")
            {
             emailSender = new GmailEmailSender(_emailConfig,_common);
            }
            else if (_emailConfig.Server == "Outlook")
            {
                emailSender = new OutlookEmailSender(_emailConfig,_common);
            }
            else
            {
                throw new Exception("Unsupported email provider.");
            }

            await emailSender.SendEmailAsync(message);
        }

        public class OutlookEmailSender : IEmailSender
        {
            private readonly EmailConfiguration _emailConfig;
            private readonly Utils _common;
            public OutlookEmailSender(EmailConfiguration emailConfig, Utils common)
            {
                _emailConfig = emailConfig;
                _common = common;
            }

            public async Task SendEmailAsync(Message message)
            {
                var mailMessage = CreateEmailMessage(message);

                if (_common.AreEmailAddressesValid(mailMessage))
                {
                    await SendAsync(mailMessage);
                }
                else
                {
                    throw new Exception("Invalid email address.");
                }
            }

            private MimeMessage CreateEmailMessage(Message message)
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.UserName));

                if (message.To != null)
                {
                    emailMessage.To.AddRange(message.To);
                }

                if (message.Bcc != null)
                {
                    emailMessage.Bcc.AddRange(message.Bcc);
                }

                if (message.Cc != null)
                {
                    emailMessage.Cc.AddRange(message.Cc);
                }

                emailMessage.Subject = message.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

                return emailMessage;
            }

            private async Task SendAsync(MimeMessage mailMessage)
            {
                using (var client = new SmtpClient())
                {
                    try
                    {
                        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                        await client.SendAsync(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        // Log an error message or handle the exception appropriately
                        throw;
                    }
                }
            }
        }

        public class GmailEmailSender : IEmailSender
        {
            private readonly EmailConfiguration _emailConfig;
            private readonly Utils _common;
            public GmailEmailSender(EmailConfiguration emailConfig, Utils common)
            {
                _emailConfig = emailConfig;
                _common = common;
            }


            public async Task SendEmailAsync(Message message)
            {
                var mailMessage = CreateEmailMessage(message);

                if (_common.AreEmailAddressesValid(mailMessage))
                {
                    await SendAsync(mailMessage);
                }
                else
                {
                    throw new Exception("Invalid email address.");
                }
            }

            private MimeMessage CreateEmailMessage(Message message)
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.UserName));
                emailMessage.To.AddRange(message.To);
                if (emailMessage.Bcc != null)
                {
                    emailMessage.Bcc.AddRange(message.Bcc);
                }
                if (emailMessage.Cc != null)
                {
                    emailMessage.Cc.AddRange(message.Cc);
                }

                emailMessage.Subject = message.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

                return emailMessage;
            }
            private async Task SendAsync(MimeMessage mailMessage)
            {
                using (var client = new SmtpClient())
                {
                    try
                    {
                        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                        await client.SendAsync(mailMessage);
                    }
                    catch
                    {
                        //log an error message or throw an exception, or both.
                        throw;
                    }
                    finally
                    {
                        await client.DisconnectAsync(true);
                        client.Dispose();
                    }
                }
            }
        }


 

    }


}
