using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    public static class PsbulkSMSHelper
    {
        public static bool SendSMS(string smsbody, string to, string apikey, string endpoint, string from, ILogger logger)
        {
            try
            {
                logger.Debug(string.Format("TextLocalSMSHelper.SendSMS phone: {0}, EndPoint: {1}", to, endpoint));
                String messageData = smsbody;



                HttpClient _Client = new HttpClient();
                var json = JObject.Parse(@"{
    'root': {
                    'type': 'A',
        'flash': 0,
        'sender': 'TXTSMS',
        'message': '',
        'service': 'T'
    },
    'nodes': [
        {
            'to': 'XXX'
        }
    ]
}");

                json["root"]["message"] = smsbody;
                json["nodes"][0]["to"] = to;
                json["root"]["sender"] = from;


                var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(endpoint);
                httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apikey);
               
                        HttpContent httpContent = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                        httpRequestMessage.Content = httpContent;

                var response = _Client.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    logger.Debug(string.Format("SMS sender resonse : {0}", responseText));
                    return true;
                }
                else
                {
                    logger.Debug(string.Format("SMS sender resonse : {0}", response.StatusCode));
                    return false;
                }

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in SendSMS : {0}", ex.Message), ex);
                return false;
            }
        }
    }
}
