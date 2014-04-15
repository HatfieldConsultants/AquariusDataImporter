using System;
using log4net;

namespace Hatfield.Aquarius.PublicServiceAPI
{
    internal class WebRequestHelper
    {
        private static readonly ILog _log = LogManager.GetLogger("Application");

        public static string GenerateRequest(string url, string authToken, System.Text.Encoding encoding)
        {
            System.Net.HttpWebRequest request = null;
            System.Net.WebResponse response = null;
            System.IO.Stream responses = null;
            System.IO.StreamReader sr = null;
            try
            {
                //try to use the existing token to execute the request
                return executeRequest(url, authToken, encoding, request, response, responses, sr);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string executeRequest(string url, string authToken, System.Text.Encoding encoding, System.Net.HttpWebRequest request, System.Net.WebResponse response, System.IO.Stream responses, System.IO.StreamReader sr)
        {
            try
            {
                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "GET";
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Add("AQAuthToken", authToken);
                }
                response = request.GetResponse();
                responses = response.GetResponseStream();
                sr = new System.IO.StreamReader(responses, encoding);
                string ret = sr.ReadToEnd();
                return ret;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}