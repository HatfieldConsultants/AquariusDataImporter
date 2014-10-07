using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Hatfield.AquariusDataImporter.Core.Models.Goes
{
    public class GoesDataImportTask : IImportable
    {
        public string StationName { get; set; }

        public IEnumerable<GoesDataParameter> Parameters { get; set; }

        public GoesDataImportTask()
        {
            Parameters = new List<GoesDataParameter>();
        }
    }
}
