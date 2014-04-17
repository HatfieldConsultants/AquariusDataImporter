using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core;
using Hatfield.AquariusDataImporter.Core.Models;
using Hatfield.AquariusDataImporter.Core.Models.Sutron;
using Hatfield.AquariusDataImporter.Domain;
using Newtonsoft.Json;

namespace Hatfield.AquariusDataImporter
{
    public static class ImportTaskFactory
    {
        

        public static IImportable CreateImportTask(ImportTask taskDomain)
        {
            if (taskDomain.HandlerName == Constants.SimpleSutronImporterName)
            {
                var deserializedTask = JsonConvert.DeserializeObject<SimpleSutronImportTask>(System.Text.Encoding.UTF8.GetString(taskDomain.DefinitionJsonString));
                if(deserializedTask == null)
                {
                    throw new InvalidCastException("System is not able to cast task domain to Simple sutron import task");
                }
                return deserializedTask;
            }

            else if (taskDomain.HandlerName == Constants.FortHillWaterIntakeImporterName)
            {
                var deserializedTask = JsonConvert.DeserializeObject<FortHillWaterIntakeImportTask>(System.Text.Encoding.UTF8.GetString(taskDomain.DefinitionJsonString));
                if (deserializedTask == null)
                {
                    throw new InvalidCastException("System is not able to cast task domain to Simple sutron import task");
                }
                return deserializedTask;
            }
            return null;
        }
    }
}
