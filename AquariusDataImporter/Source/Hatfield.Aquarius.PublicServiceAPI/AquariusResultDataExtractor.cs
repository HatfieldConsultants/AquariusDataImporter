using System;
using System.Collections.Generic;
using System.Linq;

namespace Hatfield.Aquarius.PublicServiceAPI
{
    internal class AquariusResultDataExtractor
    {
        public static IEnumerable<QueryResult> ExtractTimeSeriesFromAquarius(string responseString)
        {
            var lines = responseString.Split(Constants.Sperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lines.Count > 0)
            {
                var startIndex = lines.IndexOf("RangeNumber,Time,Value,Quality,Interpolation,Approval");
                if (startIndex < 0)
                    return null;

                var headers = new string[]
                {
                    Constants.TimeSeriesTime,
                    Constants.TimeSeriesValue,
                    Constants.TimeSeriesFlag,
                    Constants.TimeSeriesApprovalLevel
                };

                var headerDictionary = getHeaders(lines[startIndex], headers);
                var result = new List<QueryResult>();
                for (var i = startIndex + 1; i < lines.Count; i++)
                {
                    var values = lines[i].Split(',');
                    string valueString = values[headerDictionary[Constants.TimeSeriesValue]];

                    result.Add(new QueryResult
                    {
                        Time = DateTime.Parse(values[headerDictionary[Constants.TimeSeriesTime]]),
                        Value = ParseStringToNullableDouble(values[headerDictionary[Constants.TimeSeriesValue]]),
                        Quality = ParseStringToNullableInt(values[headerDictionary[Constants.TimeSeriesFlag]]),
                        ApprovalLevel = ParseStringToNullableInt(values[headerDictionary[Constants.TimeSeriesApprovalLevel]])
                    });
                }

                return result;
            }

            return null;
        }

        private static Dictionary<string, int> getHeaders(string headerString, string[] headers)
        {
            var headerDictionary = new Dictionary<string, int>();
            var headersList = headerString.Split(',').ToList();

            foreach (var header in headers)
            {
                headerDictionary.Add(header, headersList.IndexOf(header));
            }

            return headerDictionary;
        }

        private static double? ParseStringToNullableDouble(string stringValue)
        {
            try
            {
                return double.Parse(stringValue);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static int? ParseStringToNullableInt(string stringValue)
        {
            try
            {
                return int.Parse(stringValue);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}