using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Models;

namespace Hatfield.AquariusDataImporter.Core
{
    public interface IImportTaskHandler
    {
        ImportResult Import(IImportable task, DateTime? lastImportTime, int interval);
    }
}
