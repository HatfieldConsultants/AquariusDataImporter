using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core
{
    public interface IImportTaskHandler
    {
        void Import(IImportable task);
    }
}
