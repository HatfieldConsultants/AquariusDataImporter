using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Sutron
{
    public class SutronParameter
    {
        public string Identifier { get; set; }
        public int? ColumnIndex { get; set; }//parameter's column index in the data file, zero based
    }
}
