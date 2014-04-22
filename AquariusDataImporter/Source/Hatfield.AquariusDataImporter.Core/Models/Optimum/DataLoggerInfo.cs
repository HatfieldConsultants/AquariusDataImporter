using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Models.Optimum
{
    internal class DataLoggerInfo
    {
        /*
         * <datalogger-id>18</datalogger-id>
            <serial-number>1234</serial-number>
            <site-name>Creek 1</site-name>
            <latitude>56.7429</latitude>
            <longitude>-111.335</longitude>
            <construction>false</construction>
            <datalogger-condition>0</datalogger-condition>
         */
        public int Id { get; set; }
        public string SiteName { get; set; }

        public DataLoggerInfo(int id, string sitename)
        {
            this.Id = id;
            this.SiteName = sitename;
        }

    }
}
