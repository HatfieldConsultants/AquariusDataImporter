using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Models.Optimum;
using Hatfield.AquariusDataImporter.Core.Models;
using log4net;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;

namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class OptimumImportTaskHandler : IImportTaskHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Application");        

        private IAquariusAdapter _aquariusAdapter;

        public OptimumImportTaskHandler(IAquariusAdapter aquariusAdapter)
        {
            _aquariusAdapter = aquariusAdapter;
        }


        public ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            var castedTask = task as OptimumImportTask;
            if (castedTask == null)
            {
                throw new InvalidCastException("Casted to Optimum Import Task fail");
            }

            //still within interval
            if (lastImportTime.HasValue && (DateTime.Now < lastImportTime.Value.AddMinutes(interval)))
            {
                return new ImportResult
                {
                    Success = false,
                    LogMessage = "Task still within the execute interval, skip"
                };
            }

            var errorThredHold = 10;
            var importMessageBuilder = new StringBuilder();

            try
            {
                var endTime = DateTime.Now;
                var startTime = DateTime.Now.AddDays(-castedTask.NumberOfDayPriorToToday);

                var dataLogger = OptimumDataDownloadHelper.GetDataLoggersBySiteName(castedTask.StationName);
                var inputInfoOfDataLogger = OptimumDataDownloadHelper.GetInputInfoOfDataLogger(dataLogger);
                foreach (var parameter in castedTask.Parameters)
                {
                    var matchedInputInfo = inputInfoOfDataLogger.Where(x => x.Label == parameter.OptimumInputName).First();
                    var csvString = OptimumDataDownloadHelper.FetchCSVForInput(dataLogger, matchedInputInfo, startTime, endTime);

                    ImportCSVDataToAquarius(parameter, csvString);
                }

                importMessageBuilder.Append("Import data success");
                
            }
            catch(Exception ex)
            {
                importMessageBuilder.Append("Error: " + ex.Message);
                errorThredHold--;
                if(errorThredHold < 0)
                {
                    return new ImportResult { 
                        Success = false,
                        LogMessage = importMessageBuilder.ToString()
                    };
                }
            }

            return new ImportResult
            {
                Success = true,
                LogMessage = importMessageBuilder.ToString()
            };
        }


        private void ImportCSVDataToAquarius(OptimumParameter optimumParameter, string csvString)
        {
            var aquariusId = _aquariusAdapter.GetDataSetIdByIdentifier(optimumParameter.AquariusDatasetIdentifier);
            var lines = csvString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            var headers = lines[0].Split(',').ToList();
            var dateTimeColumnIndex = headers.IndexOf(Constants.DateTimeHeaderText);
            var valueColumnIndex = headers.IndexOf(Constants.MeasureMentHeaderText);

            for(int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                var dateTime = DateTime.Parse(values[dateTimeColumnIndex]);
                var actualValue = string.IsNullOrEmpty(values[valueColumnIndex]) ? null : (double?)double.Parse(values[valueColumnIndex]);

                _aquariusAdapter.PersistTimeSeriesData(aquariusId, AquariusHelper.ConstructAquariusInsertString(dateTime, 0, actualValue));
            }
            


        }
    }
}
