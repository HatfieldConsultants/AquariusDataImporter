using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core;
using Hatfield.AquariusDataImporter.Core.Models;
using Hatfield.AquariusDataImporter.Core.Models.Sutron;
using Hatfield.AquariusDataImporter.Domain;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Hatfield.AquariusDataImporter
{
    public static class ImportTaskFactory
    {
        

        public static IImportable CreateImportTask(ImportTask taskDomain)
        {
            if (taskDomain.HandlerName == Constants.SimpleSutronImporterName)
            {
                var resultString = EscapeRegexString(System.Text.Encoding.UTF8.GetString(taskDomain.DefinitionJsonString));
                var deserializedTask = JsonConvert.DeserializeObject<SimpleSutronImportTask>(resultString);
                if(deserializedTask == null)
                {
                    throw new InvalidCastException("System is not able to cast task domain to Simple sutron import task");
                }
                return deserializedTask;
            }

            else if (taskDomain.HandlerName == Constants.FortHillWaterIntakeImporterName)
            {
                var resultString = EscapeRegexString(System.Text.Encoding.UTF8.GetString(taskDomain.DefinitionJsonString));
                var deserializedTask = JsonConvert.DeserializeObject<FortHillWaterIntakeImportTask>(resultString);
                if (deserializedTask == null)
                {
                    throw new InvalidCastException("System is not able to cast task domain to Simple sutron import task");
                }
                return deserializedTask;
            }
            return null;
        }

        private static string EscapeRegexString(string rawJson)
        {

            //http://stackoverflow.com/questions/7247712/regex-to-replaces-slashes-inside-of-json
            var resultString = Regex.Replace(rawJson,
            @"(?<!\\)  # lookbehind: Check that previous character isn't a \
            \\         # match a \
            (?!\\)     # lookahead: Check that the following character isn't a \",
            @"\\", RegexOptions.IgnorePatternWhitespace);

            return resultString;
        
        }
    }
}
