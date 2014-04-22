using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using log4net;
using Hatfield.AquariusDataImporter.Core.Models.Optimum;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class OptimumDataDownloadHelper
    {
        internal static IEnumerable<InputInfo> AllInputInfo { get; set; }
    }

    
}
