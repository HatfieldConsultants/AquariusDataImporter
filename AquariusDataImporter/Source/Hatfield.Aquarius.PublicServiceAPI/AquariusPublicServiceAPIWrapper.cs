using System;
using System.Collections.Generic;
using System.Web;
using log4net;

namespace Hatfield.Aquarius.PublicServiceAPI
{
    public class AquariusPublicServiceAPIWrapper
    {
        private static readonly ILog log = LogManager.GetLogger("Application");
        private static readonly string _publicServiceURL = System.Configuration.ConfigurationManager.AppSettings["publicServiceURL"];
        private static readonly string _userName = System.Configuration.ConfigurationManager.AppSettings["publicServiceUserName"];
        private static readonly string _password = System.Configuration.ConfigurationManager.AppSettings["publicServicePassWord"];

        private static string _publicServiceToken;

        private string getToken()
        {
            string tokenURL = string.Format(Constants.TokenURL, _userName, _password);
            return WebRequestHelper.GenerateRequest(_publicServiceURL + tokenURL, null, System.Text.Encoding.UTF8);
        }

        public IEnumerable<QueryResult> GetTimeSeriesData(string dataSeriesIdentifier, DateTime startTime, DateTime endTime)
        {
            var startTimeString = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            var endTimeString = endTime.ToString("yyyy-MM-dd HH:mm:ss");

            string timeSeriesURL = string.Format(Constants.TimeSeriesURL, HttpUtility.UrlEncode(dataSeriesIdentifier), startTimeString, endTimeString);

            var timeSeriesString = FetchDataByWebRequest(timeSeriesURL);

            if (!string.IsNullOrEmpty(timeSeriesString) || timeSeriesString.Split(Constants.Sperators, StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                return AquariusResultDataExtractor.ExtractTimeSeriesFromAquarius(timeSeriesString);
            }
            else
            {
                throw new HttpParseException("Unable to parse the time series data");
            }
        }

        private string FetchDataByWebRequest(string timeSeriesURL)
        {
            try
            {
                var timeSeriesString = WebRequestHelper.GenerateRequest(_publicServiceURL + timeSeriesURL, _publicServiceToken, System.Text.Encoding.UTF8);
                return timeSeriesString;
            }
            catch (Exception ex)
            {
                log.Warn("Fail to execute Aquarius public service request. Try to get the token and execute again.");
                try
                {
                    //fail, could be caused by the expired token
                    //refresh the token and execute the request again
                    string refreshToken = getToken();
                    _publicServiceToken = refreshToken;
                    string responseString = WebRequestHelper.GenerateRequest(_publicServiceURL + timeSeriesURL, _publicServiceToken, System.Text.Encoding.UTF8);
                    //execute success with the new token
                    //update to the global token variable
                    
                    return responseString;
                }
                catch (Exception)
                {
                    //request still fail after token refresh
                    //throw exception
                    log.Error("Fail to execute Aquarius public service request with new token");
                    throw new Exception("Can not connect to the Aquarius server. Please contact the EIS team");
                }
            }
        }
    }
}