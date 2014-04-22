using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Models.Optimum;
using Hatfield.AquariusDataImporter.Core.Models;
using log4net;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;

namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class OptimumImportTaskHandler : IImportTaskHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Application");

        private IAquariusAdapter _aquariusAdapter;

        public OptimumImportTaskHandler(IAquariusAdapter aquariusAdapter)
        {
            _aquariusAdapter = aquariusAdapter;
        }


        public ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            var castedTask = task as OptimumImportTask;
            if (castedTask == null)
            {
                throw new InvalidCastException("Casted to Optimum Import Task fail");
            }

            //still within interval
            if (lastImportTime.HasValue && (DateTime.Now < lastImportTime.Value.AddMinutes(interval)))
            {
                return new ImportResult
                {
                    Success = false,
                    LogMessage = "Task still within the execute interval, skip"
                };
            }

            throw new NotImplementedException();
        }
    }
}
