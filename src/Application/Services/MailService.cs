using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MimeKit;
using RestSharp;
using RestSharp.Authenticators;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MailService : IMailService
    {
        private readonly IEncryptService _encrypt;
        private readonly SmtpConfigSettings _smtpConfigSettings;
        private readonly EmailLink _emailLink;
        private readonly IFileProvider _fileProvider;


        public MailService(IEncryptService encrypt, IOptions<SmtpConfigSettings> smtpConfigSettings, IOptions<EmailLink> emailLink,
            IFileProvider fileProvider)
        {
            _smtpConfigSettings = smtpConfigSettings.Value;
            _emailLink = emailLink.Value;
            _encrypt = encrypt;
            _fileProvider = fileProvider;
        }

        // To-do: Redesign email confimation link
        public string GenerateEmailConfirmationLinkAsync(string token, string email)
        {
            string baseUri = _emailLink.BaseUrl;

            var hrefValue = $"{baseUri}/{token}/{email}";
            var link = $"<a href='{hrefValue}' target='_blank' " +
                $"style='width: 223px;height: 50px;line-height: 50px;background: transparent linear-gradient(180deg, #faa84f 0%, #f05c38 100%) 0% 0% no-repeat padding-box;" +
                $"border-radius: 5px;outline: none;border: none;text-align: center;" +
                $"color: #ffffff;font-size: 15px;font-weight: 800;font-family: Arial, Helvetica, sans-serif;text-decoration: none;" +
                $"margin: 30px auto;display: block;cursor: pointer; '> Verify your account </a>";
            return link;
        }

        // To-do: Redesign reset password link
        public string GeneratePasswordResetLinkAsync(string token, string email)
        {
            string baseUri = _emailLink.ResetPasswordUrl;

            var hrefValue = $"{baseUri}?token={token}&email={email}";
            var link = $"<a href='{hrefValue}'" +
                $"style='width: 223px;height: 50px;line-height: 50px;" +
                $"background: transparent linear-gradient(180deg, #faa84f 0%, #f05c38 100%) 0% 0% no-repeat padding-box;" +
                $"border-radius: 5px;outline: none;font-family: Arial, Helvetica, sans-serif;" +
                $"border: none;color: #ffffff;font-size: 15px;text-align: center;text-decoration: none;" +
                $"font-weight: 800;margin: 30px auto;display: block;cursor: pointer;'> Reset my password</a>";
            return link;
        }

        public async Task SendAccountBlockedEmail(string email, string name)
        {
            string[] replacements = { name };
            await GenerateMail(email, replacements, "ACCOUNT BLOCKED", EmailTemplateUrl.AccountBlockedTemplate);
        }

        public async Task<BaseResponse<bool>> SendAccountConfirmationEmail(string email, string name)
        {
            string[] replacements = { name };
            return await GenerateMail(email, replacements, "ACCOUNT CONFIRMATION", EmailTemplateUrl.AccountConfirmationTemplate);
        }

        public async Task SendAccountUnBlockedEmail(string email, string name)
        {
            string[] replacements = { name };
            await GenerateMail(email, replacements, "ACCOUNT UNBLOCKED", EmailTemplateUrl.AccountUnBlockedTemplate);
        }

        public async Task<BaseResponse<bool>> SendAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink)
        {
            string[] replacements = { firstName, confirmationLink };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.AccountVerificationTemplate);
        }

        public async Task<BaseResponse<bool>> SendAdminAccountVerificationEmail(string emailAddress, string firstName, string subject, string confirmationLink, string password)
        {
            string[] replacements = { firstName, confirmationLink, password };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.AdminAccountVerificationTemplate);
        }

        public async Task<BaseResponse<bool>> SendMail(List<string> recipient, string[] replacements, string subject, string emailTemplatePath)
            => await BuildMailBody(recipient, replacements, subject, emailTemplatePath);

        public async Task<BaseResponse<bool>> SendPasswordResetEmail(string emailAddress, string subject, string passwordResetLink, string userName)
        {
            string[] replacements = { userName, passwordResetLink };
            return await GenerateMail(emailAddress, replacements, subject, EmailTemplateUrl.PasswordResetTemplate);
        }

        private async Task<BaseResponse<bool>> GenerateMail(string emailAddress, string[] replacements, string subject, string templateUrl)
        {
            var htmlbody = GenerateEmailHtmlBody(replacements, templateUrl);

            if (htmlbody == null)
            {
                return new BaseResponse<bool>(false);
            }

            var mail = new MailRequest();
            mail.Recipient.Add(emailAddress);
            mail.Subject = subject;
            mail.IsHtmlBody = true;
            mail.Body = htmlbody;

            return await SendEmail(mail);
        }

        private async Task<BaseResponse<bool>> BuildMailBody(List<string> destination, string[] replacements, string subject, string templateUrl)
        {
            var msg = new MailRequest
            {
                Recipient = destination,
                Subject = subject,
                IsHtmlBody = true,
                Body = GenerateEmailHtmlBody(replacements, templateUrl)
            };

            return await SendEmail(msg);
        }

        private string? GenerateEmailHtmlBody(string[] replacements, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var file = _fileProvider.GetFileInfo(path);
            if (file == null || !file.Exists)
            {
                var fullPath = Path.Combine(path);
                Console.WriteLine($"it is null Resolved file path: {fullPath} {Path.GetFullPath(path)}");

                return null;
            }

            var builder = new BodyBuilder();

            string messageBody;
            using var SourceReader = File.OpenText(file.PhysicalPath!);

            builder.HtmlBody = SourceReader.ReadToEnd();

            if (replacements.Length != 0)
            {
                messageBody = string.Format(builder.HtmlBody, replacements);
            }
            else
            {
                messageBody = string.Format(builder.HtmlBody);
            }


            return messageBody;
        }

        private async Task<BaseResponse<bool>> SendEmail(MailRequest mailRequest)
        {
            try
            {
                if (!string.IsNullOrEmpty(mailRequest.Body) && !string.IsNullOrEmpty(mailRequest.Subject) && mailRequest.Recipient.Count > 0)
                {

                    //await SendEmailViaSendGrid(_smtpConfigSettings.Password, _smtpConfigSettings.DisplayName, _smtpConfigSettings.Mail, mailRequest.Subject, mailRequest.Body, mailRequest.Body, mailRequest.Recipient);
                    await SendEmailViaMailgun(_smtpConfigSettings.Password, _smtpConfigSettings.DisplayName, _smtpConfigSettings.Mail, mailRequest.Subject, mailRequest.Body, mailRequest.Body, mailRequest.Recipient);

                    return new BaseResponse<bool>(true, "MESSAGE SENT SUCCESSFULLY!...");
                }

                return new BaseResponse<bool>(false, "Message not Sent!");
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>(false, $"Message not Sent...{ex.Message}");
            }

        }

        private async Task SendEmailViaMailgun(
        string apiKey,
        string displayName,
        string sender,
        string subject,
        string plainTextContent,
        string htmlContent,
        List<string> recipients)
        {
            var domain = "yourdomain";

            var options = new RestClientOptions("https://api.mailgun.net")
            {
                Authenticator = new HttpBasicAuthenticator("api", "yourpassword")
            };

            var client = new RestClient(options);
            var request = new RestRequest($"/v3/{domain}/messages", RestSharp.Method.Post)
            {
                AlwaysMultipartFormData = true
            };

            request.AddParameter("o:tag", "transactional");
            request.AddParameter("o:tracking", "yes");
            request.AddParameter("o:tracking-clicks", "yes");

            // From
            request.AddParameter("from", $"{displayName} <postmaster@yourdomain.com>");

            // To — Mailgun supports comma-separated recipients
            var recipientList = string.Join(",", recipients);
            request.AddParameter("to", recipientList);

            // Subject
            request.AddParameter("subject", subject);

            // Text & HTML body
            if (!string.IsNullOrEmpty(plainTextContent))
                request.AddParameter("text", plainTextContent);

            if (!string.IsNullOrEmpty(htmlContent))
                request.AddParameter("html", htmlContent);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Mailgun send failed: {response.StatusCode} - {response.Content}");
            }
        }

        private async Task SendEmailViaSendGrid(string apiKey, string displayName, string sender, string subject, string plainTextContent, string htmlContent, List<string> recipients)
        {
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(sender, displayName);
            var emailRecipients = new List<EmailAddress>();

            foreach (var recipient in recipients)
            {
                emailRecipients.Add(new EmailAddress
                {
                    Email = recipient
                });
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, emailRecipients, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }

        private async Task SendEmailViaSMTP(string password, string displayName, string sender, string subject, string plainTextContent, string htmlContent, List<string> recipients)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            MailMessage mailMessage = new()
            {
                IsBodyHtml = true,
                From = new MailAddress("postmaster@yourdomain.com") //IMPORTANT: This must be same as your smtp authentication address.
            };

            foreach (var recipient in recipients)
            {
                mailMessage.To.Add(recipient);
            }

            //mailMessage.To.Add("postmaster@yourdomain.com");

            //set the content 
            mailMessage.Subject = subject;

            //mail.Body = "This is from system.net.mail using C sharp with smtp authentication.";

            string plainTextBody = plainTextContent;
            AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
            mailMessage.AlternateViews.Add(plainTextView);

            string htmlBody = htmlContent;
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
            mailMessage.AlternateViews.Add(htmlView);


            //send the message 
            //SmtpClient smtp = new("mail.yourdomain.com");
            SmtpClient smtp = new(sender);

            //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
            NetworkCredential Credentials = new("postmaster@yourdomain.com", password);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 25;    //alternative port number is 8889
            smtp.EnableSsl = false;


            await smtp.SendMailAsync(mailMessage);
        }
    }

    public class SmtpMailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;

        public SmtpMailService()
        {
            _fromEmail = "noreply@e-homez.com";
            _smtpClient = new SmtpClient
            {
                Host = "mail.e-homez.com",
                Port = 25, // Using SSL port for better security
                Credentials = new NetworkCredential("noreply@e-homez.com", "Ehomez_1"),
                EnableSsl = true
            };
        }

        public async Task SendEmailAsync(
            List<string> toEmails,
            string subject,
            string body,
            bool isHtml = false,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            List<EmailAttachment>? attachments = null)
        {
            try
            {
                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();
                m.From = new MailAddress("noreply@e-homez.com");
                m.To.Add("anyanwuraphaelc@gmail.com");
                m.Subject = "This is a test";
                m.Body = "This is a sample message using SMTP authentication";
                sc.Host = "mail.e-homez.com";
                string str1 = "gmail.com";
                string str2 = "noreply@e-homez.com";

                if (str2.Contains(str1))
                {
                    try
                    {
                        sc.Port = 587;
                        sc.Credentials = new System.Net.NetworkCredential("noreply@e-homez.com", "Ehomez_1");
                        sc.EnableSsl = true;
                        sc.Send(m);
                    }
                    catch (Exception ex)
                    {
                        throw new EmailSendException("Failed to send email", ex);

                    }
                }
                else
                {
                    try
                    {
                        sc.Port = 25;
                        sc.Credentials = new System.Net.NetworkCredential("noreply@e-homez.com", "Ehomez_1");
                        sc.EnableSsl = false;
                        sc.Send(m);
                    }
                    catch (Exception ex)
                    {
                        throw new EmailSendException("Failed to send email", ex);

                    }
                }


                //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                //            //create the mail message 
                //MailMessage mail = new MailMessage();

                ////set the addresses 
                //mail.From = new MailAddress("noreply@e-homez.com"); //IMPORTANT: This must be same as your smtp authentication address.
                //mail.To.Add("grantlincoln007@outlook.com");

                ////set the content 
                //mail.Subject = "This is an email";
                //mail.Body = "This is from system.net.mail using C sharp with smtp authentication.";
                ////send the message 
                //SmtpClient smtp = new SmtpClient("mail.e-homez.com");
                ////SmtpClient smtp = new SmtpClient("mail5016.site4now.net");

                ////IMPORANT:  Your smtp login email MUST be same as your FROM address. 
                //NetworkCredential Credentials = new NetworkCredential("noreply@e-homez.com", "Ehomez_1");
                //smtp.UseDefaultCredentials = false;
                //smtp.Credentials = Credentials;
                //smtp.Port = 587;    //alternative port number is 8889
                //smtp.EnableSsl = true;
                //smtp.Send(mail);

                //using var message = new MailMessage
                //{
                //    From = new MailAddress(_fromEmail),
                //    Subject = subject,
                //    Body = body,
                //    IsBodyHtml = isHtml
                //};

                //// Add primary recipients
                //foreach (var email in toEmails)
                //{
                //    message.To.Add(email);
                //}

                //// Add CC recipients if any
                //if (ccEmails?.Any() == true)
                //{
                //    foreach (var email in ccEmails)
                //    {
                //        message.CC.Add(email);
                //    }
                //}

                //// Add BCC recipients if any
                //if (bccEmails?.Any() == true)
                //{
                //    foreach (var email in bccEmails)
                //    {
                //        message.Bcc.Add(email);
                //    }
                //}

                //// Add attachments if any
                //if (attachments?.Any() == true)
                //{
                //    foreach (var attachment in attachments)
                //    {
                //        if (attachment.ContentStream != null)
                //        {
                //            message.Attachments.Add(new System.Net.Mail.Attachment(attachment.ContentStream, attachment.FileName, attachment.ContentType));
                //        }
                //        else if (!string.IsNullOrEmpty(attachment.FilePath))
                //        {
                //            message.Attachments.Add(new System.Net.Mail.Attachment(attachment.FilePath));
                //        }
                //    }
                //}

                //await _smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new EmailSendException("Failed to send email", ex);
            }
        }
    }

    public class EmailAttachment
    {
        public string? FilePath { get; set; }
        public Stream? ContentStream { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "application/octet-stream";
    }

    public class EmailSendException : Exception
    {
        public EmailSendException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
