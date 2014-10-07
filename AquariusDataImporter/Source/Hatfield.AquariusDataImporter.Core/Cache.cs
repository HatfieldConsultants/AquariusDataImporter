using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Helpers;
using Hatfield.AquariusDataImporter.Core.Models.Goes;

namespace Hatfield.AquariusDataImporter.Core
{
    public static class Cache
    {
        public static IEnumerable<SutronDataFile> SutronDataFileCache;
        public static Dictionary<string, IEnumerable<GoesValueData>> GoesDataCache;
    }
}
