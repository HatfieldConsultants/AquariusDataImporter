using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;


namespace Hatfield.AquariusDataImporter.Core
{
    public class LongTimeoutWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            int timeoutMin = 5;
            try
            {
                var timeoutString = System.Configuration.ConfigurationManager.AppSettings["TimeOutMinute"];
                timeoutMin = int.Parse(timeoutString);
            }
            catch (Exception ex)
            {
                timeoutMin = 5;
            }

            var webRequest = base.GetWebRequest(address);
            webRequest.Timeout = timeoutMin * 60 * 1000;
            return webRequest;
        }
    }
}
