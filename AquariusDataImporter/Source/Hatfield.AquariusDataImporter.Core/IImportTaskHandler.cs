using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core
{
    public interface IImportTaskHandler
    {
        bool Import(IImportable task);
    }
}
