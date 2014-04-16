using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Sutron
{
    public class FortHillWaterIntakeImportTask : SimpleSutronImportTask, IImportable
    {
        public string StationATurbidityTSSIdentifier { get; set; }
        public string StationBTurbidityTSSIdentifier { get; set; }
        public string StationCTurbidityTSSIdentifier { get; set; }

        public string TSSLess250BAIdentifier { get; set; }
        public string TSSLess250CAIdentifier { get; set; }
        public string TSSLarger250BAIdentifier { get; set; }
        public string TSSLarger250CAIdentifier { get; set; }

    }
}
