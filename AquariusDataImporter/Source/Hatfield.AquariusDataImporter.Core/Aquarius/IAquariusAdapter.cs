using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.Aquarius.PublicServiceAPI;

namespace Hatfield.AquariusDataImporter.Core.Aquarius
{
    public interface IAquariusAdapter
    {
        bool PersistTimeSeriesData(long Id, string timeSeriesData);
        long GetDataSetIdByIdentifier(string identifier);
        int DeleteTimeSeries(long dataId, DateTime queryFrom, DateTime queryTo);
        IEnumerable<QueryResult> GetTimeSeriesData(string dataId, DateTime queryFrom, DateTime queryTo);
    }
}
