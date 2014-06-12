using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class GoesDataImportTaskHandler : IImportTaskHandler
    {
        public Models.ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            throw new NotImplementedException();
        }
    }
}
