using System.Collections.Generic;
using ModernSlavery.Core.Classes.Logger;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions.AspNetCore;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;

namespace ModernSlavery.Core.Classes
{
    public class GovNotifyAPI : IGovNotifyAPI
    {

        private readonly NotificationClient _client = new NotificationClient(_apiKey);
        private static string _apiKey => Config.GetAppSetting("Email:Providers:GovNotify:ApiKey");

        public EmailNotificationResponse SendEmail(NotifyEmail notifyEmail)
        {
            try
            {
                EmailNotificationResponse response = _client.SendEmail(
                    notifyEmail.EmailAddress,
                    notifyEmail.TemplateId,
                    notifyEmail.Personalisation);

                return response;
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "EMAIL FAILURE: Error whilst sending email using Gov.UK Notify",
                    new {
                        NotifyEmail = notifyEmail,
                        Personalisation = JsonConvert.SerializeObject(notifyEmail.Personalisation),
                        ErrorMessageFromNotify = e.Message
                    });
                throw;
            }
        }

        public LetterNotificationResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null)
        {
            try
            {
                LetterNotificationResponse response = _client.SendLetter(templateId, personalisation, clientReference);

                return response;
            }
            catch (NotifyClientException e)
            {
                CustomLogger.Error(
                    "Error whilst sending letter using Gov.UK Notify",
                    new {
                        NotifyTemplateId = templateId,
                        Personalisation = JsonConvert.SerializeObject(personalisation),
                        ErrorMessageFromNotify = e.Message
                    });

                return null;
            }
        }

    }
}
