using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.TaskHandlers;
using Hatfield.AquariusDataImporter.Core.Aquarius;

namespace Hatfield.AquariusDataImporter.Core
{
    public static class TaskHandlerFactory
    {
        public static IImportTaskHandler CreateTaskHandler(string handlerName)
        {
            if (handlerName == Constants.SimpleSutronImporterName)
            {
                return new SimpleSutronImportTaskHandler(new AquariusAdapter());
            }

            else if (handlerName == Constants.FortHillWaterIntakeImporterName)
            {
                return new FortHillWaterIntakeSutronImportTaskHandler(new AquariusAdapter());
            }
            else if (handlerName == Constants.OptimumImporterName)
            {
                return new OptimumImportTaskHandler(new AquariusAdapter());
            }
            return null;
        }
    }
}
