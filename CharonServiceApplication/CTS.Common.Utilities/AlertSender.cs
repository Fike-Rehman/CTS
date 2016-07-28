using System.Collections.Specialized;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CTS.Common.Utilities
{
    /// <summary>
    /// Sends an alert (either via email or text message) using the elastic email API
    /// Follow the link for more details: 
    // https://elasticemail.com/api-documentation
    /// </summary>
    public class AlertSender
    {
        // Elastic email API settings:
        private const string USERNAME = "fike.rehman@hotmail.com";
        private const string API_KEY = "bde71d06-2bd7-4869-ae6c-4c88a0f1a7a2";

        private const string @from = "charon.service@CTS.com";
        private const string fromName = "Charon Service";

        /// <summary>
        /// Email address at which this alert will be sent
        /// </summary>
        public string AlertSubscriberEmailAddress { get; set; } = "fike.rehman@gmail.com";

        /// <summary>
        /// SMS email address that is used for SMS Alert
        /// </summary>
        // TODO: Convert this into a phone number & carrier ID
        public string AlertSubscriberSMSAddress { get; set; } = "6123068969@tmomail.net";


        /// <summary>
        /// Sends an Alert via Email.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="bodyHtml"></param>
        /// <param name="bodyText"></param>
        /// <returns></returns>
        public bool SendEmailAlert(string subject, string bodyHtml = null, string bodyText = null)
        {
            var success = false;

            var mailSettings = new NameValueCollection
            {
                {"username", USERNAME},
                {"api_key", API_KEY},
                {"from", @from},
                {"from_name", fromName},
                {"subject", subject},
                {"to", AlertSubscriberEmailAddress}
            };


            if (bodyHtml != null)
            {
                mailSettings.Add("body_Html", bodyHtml);
            }
            else
            {
                if (bodyText != null) mailSettings.Add("body_text", bodyText);
            }

            using (var client = new WebClient())
            {
                try
                {
                    var response = client.UploadValues("https://api.elasticemail.com/v2/email/send", mailSettings);

                    var result = JObject.Parse(Encoding.UTF8.GetString(response));

                    if (result["success"].ToString() == "True")
                        success = true;
                }
                catch(System.Exception)
                {
                    // eat it up
                    success = false;
                }

                return success;
            }
        }


        /// <summary>
        /// Send an Alert via SMS
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="bodyText"></param>
        /// <returns></returns>
        public bool SendSMSAlert(string subject, string bodyText)
        {
            var bSuccess = false;

            var mailSettings = new NameValueCollection
            {
                {"username", USERNAME},
                {"api_key", API_KEY},
                {"from", @from},
                {"from_name", fromName},
                {"subject", subject},
                {"to", AlertSubscriberSMSAddress },
                {"body_text", bodyText }
            };

            using (var client = new WebClient())
            {
                try
                {
                    var response = client.UploadValues("https://api.elasticemail.com/v2/email/send", mailSettings);

                    var result = JObject.Parse(Encoding.UTF8.GetString(response));

                    if (result["success"].ToString() == "True")
                        bSuccess = true;
                }
                catch(System.Exception)
                {
                    // eat it up
                    bSuccess = false;
                }

                return bSuccess;   
            }
        }
    }
}
