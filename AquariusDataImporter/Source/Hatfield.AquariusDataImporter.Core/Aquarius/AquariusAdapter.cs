using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.Aquarius.PublicServiceAPI;

namespace Hatfield.AquariusDataImporter.Core.Aquarius
{
    public class AquariusAdapter : IAquariusAdapter
    {
        private const string SaveByUserName = "Admin";

        private static readonly ILog log = LogManager.GetLogger("Application");

        private AQAcquisitionServiceClient _acquictionClient;
        private AquariusPublicServiceAPIWrapper _publicServiceAPIClient;
        private static string _acquisitionServiceToken;

        private static readonly string _userName = System.Configuration.ConfigurationManager.AppSettings["acquisitionServiceUserName"];
        private static readonly string _password = System.Configuration.ConfigurationManager.AppSettings["acquisitionServicePassWord"];

        public AquariusAdapter()
        {
            _acquictionClient = new AQAcquisitionServiceClient("BasicHttpBinding_IAQAcquisitionService");
            _acquisitionServiceToken = _acquictionClient.GetAuthToken(_userName, _password);
            _publicServiceAPIClient = new AquariusPublicServiceAPIWrapper();
        }


        public bool PersistTimeSeriesData(long Id, string timeSeriesData)
        {
            try
            {
                var result = _acquictionClient.AppendTimeSeriesFromBytes2(_acquisitionServiceToken, Id, System.Text.Encoding.UTF8.GetBytes(timeSeriesData), SaveByUserName);
                if (result.NumPointsAppended < 0)
                {
                    _acquictionClient.UndoAppend(_acquisitionServiceToken, "", result.AppendToken);
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }

        public long GetDataSetIdByIdentifier(string identifier)
        {
            return _acquictionClient.GetLocationId(_acquisitionServiceToken, identifier);
        }

        public int DeleteTimeSeries(long dataId, DateTime queryFrom, DateTime queryTo)
        {
            var numberOfDelete = _acquictionClient.DeleteTimeSeriesPointsByTimeRange(_acquisitionServiceToken, dataId, queryFrom, queryTo);
            return numberOfDelete;
        }

        public IEnumerable<QueryResult> GetTimeSeriesData(string dataId, DateTime queryFrom, DateTime queryTo)
        {
            return _publicServiceAPIClient.GetTimeSeriesData(dataId, queryFrom, queryTo);
        }
    }
}
