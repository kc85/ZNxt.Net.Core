using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    class EmailService : IEmailService
    {
        private readonly IDBService _dbService;
        private readonly ILogger _logger;
        private readonly IAppSettingService _appSettingService;
        public EmailService(IDBService dBService, IAppSettingService appSettingService, ILogger logger)
        {
            _logger = logger;
            _dbService = dBService;
            _appSettingService = appSettingService;
        }
        public bool Send(List<string> toEmail, string fromEmail, List<string> CC, string emailBody, string subject)
        {
            JObject emailData = new JObject();
            emailData[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
            emailData[CommonConst.CommonField.FROM] = fromEmail;
            emailData[CommonConst.CommonField.SUBJECT] = subject;
            emailData[CommonConst.CommonField.TO] = new JArray();
            foreach (var email in toEmail)
            {
                (emailData[CommonConst.CommonField.TO] as JArray).Add(email);
            }
            emailData[CommonConst.CommonField.CC] = new JArray();
            if (CC != null)
            {
                foreach (var email in CC)
                {
                    (emailData[CommonConst.CommonField.CC] as JArray).Add(email);
                }
            }
            emailData[CommonConst.CommonField.BODY] = emailBody;
            emailData[CommonConst.CommonField.STATUS] = EmailStatus.Queue.ToString();

            if (_dbService.Write(CommonConst.Collection.EMAIL_QUEUE, emailData))
            {
                Dictionary<string, string> filter = new Dictionary<string, string>();
                filter[CommonConst.CommonField.DISPLAY_ID] = emailData[CommonConst.CommonField.DISPLAY_ID].ToString();
                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(_appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER));

                    mail.From = new MailAddress(emailData[CommonConst.CommonField.FROM].ToString());
                    foreach (var item in emailData[CommonConst.CommonField.TO])
                    {
                        mail.To.Add(item.ToString());
                    }

                    mail.Subject = emailData[CommonConst.CommonField.SUBJECT].ToString();
                    mail.Body = emailData[CommonConst.CommonField.BODY].ToString();
                    mail.IsBodyHtml = true;
                    int port = 587;
                    int.TryParse(_appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_PORT), out port);
                    SmtpServer.Port = port;
                    var user = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_USER);
                    var password = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_PASSWORD);
                    SmtpServer.Credentials = new System.Net.NetworkCredential(user, password);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    emailData[CommonConst.CommonField.STATUS] = EmailStatus.Sent.ToString();
                    _dbService.Write(CommonConst.Collection.EMAIL_QUEUE, emailData, filter);

                    return true;
                }
                catch (Exception ex)
                {
                    emailData[CommonConst.CommonField.STATUS] = EmailStatus.SendError.ToString();
                    _dbService.Write(CommonConst.Collection.EMAIL_QUEUE, emailData, filter);
                    _logger.Error("Error email send ", ex);
                    return false;
                }
            }
            else
            {
                _logger.Error("Error in add email data in queue");
                return false;
            }
        }
    }
}
