using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Optimum
{
    public class OptimumImportTask : IImportable
    {        
        
        private readonly static int defaultNumberOfDayPriorToToday = 7;

        public int NumberOfDayPriorToToday { get; set; }
        
        public string StationName { get; set; }

        public IEnumerable<OptimumParameter> Parameters { get; set; }

        public OptimumImportTask()
        {
            Parameters = new List<OptimumParameter>();
            NumberOfDayPriorToToday = defaultNumberOfDayPriorToToday;
        }
    }
}
