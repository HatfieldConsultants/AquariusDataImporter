using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using log4net;
using Hatfield.AquariusDataImporter.Core.Models.Optimum;
using Hatfield.AquariusDataImporter.Core;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class OptimumDataDownloadHelper
    {
        private static readonly ILog log = log4net.LogManager.GetLogger("Application");

        private static IEnumerable<InputInfo> AllInputInfo;//cache all input info
        private static IEnumerable<DataLoggerInfo> AllDataLoggerInfo;//cache all data logger info

        public static DataLoggerInfo GetDataLoggersBySiteName(string siteName)
        { 
            if(AllDataLoggerInfo == null)
            {
                AllDataLoggerInfo = FetchAllDataLoggersFromOptimum();
            }

            var matchedDataLoggers = AllDataLoggerInfo.Where(x => x.SiteName == siteName);

            if(matchedDataLoggers == null || !matchedDataLoggers.Any())
            {
                var message = "System can not find data logger named: " + siteName;
                LogMessageAndThrowException(message);
            }

            if(matchedDataLoggers.Count() > 1)
            {
                var message = "There are multiple data logger named: " + siteName;
                LogMessageAndThrowException(message);
            }

            return matchedDataLoggers.First();
        
        }

        public static IEnumerable<InputInfo> GetInputInfoOfDataLogger(DataLoggerInfo dataLoggerInfo)
        { 
            if(AllInputInfo == null)
            {
                AllInputInfo = FetchAllInputsFromOptimum();
            }

            var allInputIdsOfDataLogger = FetchAllInputIdsForLogger(dataLoggerInfo);

            return AllInputInfo.Where(x => allInputIdsOfDataLogger.Contains(x.Id));
        
        }

        public static string FetchCSVForInput(DataLoggerInfo dataLogger, InputInfo input, DateTime startDate, DateTime endDate)
        {
            var dateParamFormat = "yyyy-MM-dd HH':'mm':'ss";// "2013-05-01 00:00:00";

            var endDateParam = endDate.ToString(dateParamFormat);
            var startDateParam = startDate.ToString(dateParamFormat);

            var url = String.Format(Constants.SingleInputCSVDataDownloadURLFormat, Constants.DefaultOptimumAPIKey, dataLogger.Id, input.Id, startDateParam, endDateParam);

            log.InfoFormat("Fetching CSV between {3} and {4} for logger {1} input {2}. Url:{5}", Constants.DefaultOptimumAPIKey, dataLogger.Id, input.Id, startDateParam, endDateParam, url);

            var webClient = new LongTimeoutWebClient();

            var csv = webClient.DownloadString(url);
            log.Info("CSV of length " + csv.Length + " fetched");
            return csv;
        }

        private static IEnumerable<DataLoggerInfo> FetchAllDataLoggersFromOptimum()
        {
            var webClient = new LongTimeoutWebClient();
            var url = String.Format(Constants.ListAllDataLoggerURLFormat, Constants.DefaultOptimumAPIKey);

            var xml = webClient.DownloadString(url);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var dataLoggerNodes = doc.SelectNodes("/dataloggers/datalogger");
            var ret = new List<DataLoggerInfo>();
            foreach (XmlNode dataLoggerNode in dataLoggerNodes)
            {
                var idNode = dataLoggerNode.SelectSingleNode("child::datalogger-id");

                var loggerId = Int32.MinValue;
                if (Int32.TryParse(idNode.InnerText, out loggerId))
                {
                    var nameNode = dataLoggerNode.SelectSingleNode("child::site-name");
                    var siteName = nameNode.InnerText;
                    ret.Add(new DataLoggerInfo(loggerId, siteName));
                }
                else
                {
                    log.Error("Could not parse dataLoggerId from \"" + idNode.InnerText + "\"");
                }
                    
            } // foreach
            return ret;
        }

        private static IEnumerable<InputInfo> FetchAllInputsFromOptimum()
        {
            var webClient = new LongTimeoutWebClient();
            var url = String.Format(Constants.ListAllInputsURLFormat, Constants.DefaultOptimumAPIKey);

            var xml = webClient.DownloadString(url);

            return InputInfo.ParseFromListAllInputsXml(xml);
        }


        private static int[] FetchAllInputIdsForLogger(DataLoggerInfo dataLogger)
        {
            var webClient = new LongTimeoutWebClient();
            var url = String.Format(Constants.SingleDataLoggerDetailURLFormat, Constants.DefaultOptimumAPIKey, dataLogger.Id);

            var xml = webClient.DownloadString(url);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var inputIdNodes = doc.SelectNodes("/datalogger/active-inputs/active-input/input-id");
            var ret = new List<int>();
            foreach (XmlNode node in inputIdNodes)
            {
                var inputId = Int32.MinValue;
                if (Int32.TryParse(node.InnerText, out inputId))
                {
                    ret.Add(inputId);
                }
                else
                {
                    log.Error("Could not parse inputId from \"" + node.InnerText + "\"");
                }
                    
            } // foreach
            return ret.ToArray();
        }

        

        private static void LogMessageAndThrowException(string errorMessage)
        {
            log.Error(errorMessage);
            throw new InvalidDataException(errorMessage);
        
        }
    }

    
}
