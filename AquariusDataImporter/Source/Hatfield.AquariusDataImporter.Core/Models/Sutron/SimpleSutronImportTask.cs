using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Sutron
{
    public class SimpleSutronImportTask : IImportable
    {
        protected static readonly int defaultNumberOfMinute = 20;

        public string DownloadURL { get; set; }
        public string FileNameRegex { get; set; }

        private int? numberOfMinuteInThePast;
        public int? NumberOfMinuteInThePast {
            get{
                return numberOfMinuteInThePast.HasValue ? numberOfMinuteInThePast : defaultNumberOfMinute;            
            }
            set{
                numberOfMinuteInThePast = value;
            }
 
        }

        public IEnumerable<SutronParameter> Parameters { get; set; }

        public SimpleSutronImportTask()
        {
            Parameters = new List<SutronParameter>();
        }

        public virtual bool IsMatchedDefinition(string fileName)
        {
            var match = System.Text.RegularExpressions.Regex.Match(fileName, FileNameRegex);
            return (match.Length > 0);
        }
    }
}
