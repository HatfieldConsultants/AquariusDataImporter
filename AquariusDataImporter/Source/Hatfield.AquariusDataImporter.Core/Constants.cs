using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core
{
    public static class Constants
    {
        public static string SimpleSutronImporterName = "Simple Sutron Importer";
        public static string FortHillWaterIntakeImporterName = "Fort Hill Water Intake Importer";
        public static string OptimumImporterName = "Optimum Data Importer";
        public static string GoesDataImporterName = "Goes Data Importer";

        public readonly static string ListAllDataLoggerURLFormat = @"http://vdn5.optinst.com/api/v1/dataloggers.xml?api_key={0}";
        public readonly static string ListAllInputsURLFormat = @"http://vdn5.optinst.com/api/v1/inputs.xml?api_key={0}";
        public readonly static string SingleDataLoggerDetailURLFormat = @"http://vdn5.optinst.com/api/v1/dataloggers/{1}.xml?api_key={0}";
        public readonly static string SingleInputCSVDataDownloadURLFormat = @"http://vdn5.optinst.com/api/v1/dolphin_data/{1}.csv?api_key={0}&amp;input={2}&amp;timezone=true&amp;start_date={3}&amp;end_date={4}";
        public readonly static string DefaultOptimumAPIKey = @"1uvUcZsBvaS2lQz0c3wmQA";
        public readonly static string DateTimeHeaderText = "DateTimeStamp";
        public readonly static string MeasureMentHeaderText = "Measurement";

    }
}
