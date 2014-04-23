using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class AquariusHelper
    {
        /// <summary>
        /// The aquarius append format should be "YYYY-MM-DD HH:MM:SS, nnn.mmm, fff, ggg, iii, aaa, note"
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConstructAquariusInsertString(string date, string time, int offSet, double? value)
        {
            //we need to save the collect time to Aquarius, and the data is collect in Alberta
            //convert UTC time to Alberta time is -7 hours
            var dateTimeValue = DateTime.Parse(date + " " + time).AddHours(offSet);
            var dateTimeString = dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss");
            string appendDataFormat = "{0}, {1},,,,,\n";
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time) || !value.HasValue)
            {
                return string.Empty;
            }
            //here need to construct the aquarius append string, reference aquarius acqusition API manual
            return string.Format(appendDataFormat, dateTimeString, value);
        }

        public static string ConstructAquariusInsertString(DateTime dateTime, int offSet, double? value)
        {

            var dateTimeString = dateTime.AddHours(offSet).ToString("yyyy-MM-dd HH:mm:ss");
            string appendDataFormat = "{0}, {1},,,,,\n";

            //here need to construct the aquarius append string, reference aquarius acqusition API manual
            return string.Format(appendDataFormat, dateTimeString, value);
        }
    }
}
