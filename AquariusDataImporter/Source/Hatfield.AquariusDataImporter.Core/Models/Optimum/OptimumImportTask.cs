using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Optimum
{
    public class OptimumImportTask : IImportable
    {
        private readonly static string listAllDataLoggerURLFormat = @"http://vdn5.optinst.com/api/v1/dataloggers.xml?api_key={0}";
        private readonly static string listAllInputsURLFormat = @"http://vdn5.optinst.com/api/v1/inputs.xml?api_key={0}";
        private readonly static string singleDataLoggerDetailURLFormat = @"http://vdn5.optinst.com/api/v1/dataloggers/{1}.xml?api_key={0}";
        private readonly static string singleInputCSVDataDownloadURLFormat = @"http://vdn5.optinst.com/api/v1/dolphin_data/{1}.csv?api_key={0}&amp;input={2}&amp;timezone=true&amp;start_date={3}&amp;end_date={4}";
        
        private readonly static int defaultNumberOfDayPriorToToday = 7;
        private readonly static bool forceFixedStartDate = false;
        private readonly static bool forceFixedEndDate = false;
        private readonly static string defaultAPIKey = @"1uvUcZsBvaS2lQz0c3wmQA";
        private readonly static DateTime defaultStartDateTime = new DateTime(2013, 6, 1);
        private readonly static DateTime defaultEndDateTime = new DateTime(2013, 6, 1);

        public int NumberOfDayPriorToToday { get; set; }
        public bool ForceFixedStartDate { get; set; }
        public bool ForceFixedEndDate { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string APIKey { get; set; }

        public IEnumerable<OptimumParameter> Parameters { get; set; }

        public OptimumImportTask()
        {
            Parameters = new List<OptimumParameter>();

            NumberOfDayPriorToToday = defaultNumberOfDayPriorToToday;
            ForceFixedStartDate = forceFixedStartDate;
            ForceFixedEndDate = forceFixedEndDate;
            StartDateTime = defaultStartDateTime;
            EndDateTime = defaultEndDateTime;
            APIKey = defaultAPIKey;
        }
    }
}
