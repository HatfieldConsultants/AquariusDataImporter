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
        public static string SimpleSutronImportorName = "Simple Sutron Importer";

        public static IImportable CreateImportTask(ImportTask taskDomain)
        {
            if (taskDomain.HandlerName == SimpleSutronImportorName)
            {
                var deserializedTask = JsonConvert.DeserializeObject<SimpleSutronImportTask>(taskDomain.DefinitionJsonString);
                if(deserializedTask == null)
                {
                    throw new InvalidCastException("System is not able to cast task domain to Simple sutron import task");
                }
                return deserializedTask;
            }
            return null;
        }
    }
}
