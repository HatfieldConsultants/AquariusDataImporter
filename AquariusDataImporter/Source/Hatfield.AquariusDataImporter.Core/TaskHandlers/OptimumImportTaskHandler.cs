using System;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;
using Hatfield.AquariusDataImporter.Core.Models;
using Hatfield.AquariusDataImporter.Core.Models.Optimum;
using log4net;

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
                    try
                    {
                        var matchedInputInfo = inputInfoOfDataLogger.Where(x => x.Label == parameter.OptimumInputName).First();
                        var csvString = OptimumDataDownloadHelper.FetchCSVForInput(dataLogger, matchedInputInfo, startTime, endTime);

                        ImportCSVDataToAquarius(parameter, csvString);
                    }
                    catch (Exception ex)
                    {
                        importMessageBuilder.Append("Error: " + ex.Message);
                        errorThredHold--;
                        if (errorThredHold < 0)
                        {
                            throw ex;
                        }
                        continue;
                    }
                }

                importMessageBuilder.Append("Import data success");
            }
            catch (Exception ex)
            {
                importMessageBuilder.Append("Error: " + ex.Message);
                errorThredHold--;
                if (errorThredHold < 0)
                {
                    return new ImportResult
                    {
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
            var lines = csvString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var headers = lines[0].Split(',').ToList();

            var dateTimeColumnIndex = headers.IndexOf(Constants.DateTimeHeaderText);

            if (dateTimeColumnIndex == -1)
            {
                throw new IndexOutOfRangeException("Can not find date time header in the download csv for " + optimumParameter.AquariusDatasetIdentifier);
            }
            var valueColumnIndex = headers.IndexOf(Constants.MeasureMentHeaderText);

            if (valueColumnIndex == -1)
            {
                throw new IndexOutOfRangeException("Can not find measurement header in the download csv for " + optimumParameter.AquariusDatasetIdentifier);
            }

            try
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(new string[] { "," }, StringSplitOptions.None);
                    var dateTime = DateTime.Parse(values[dateTimeColumnIndex]);
                    var actualValue = string.IsNullOrEmpty(values[valueColumnIndex]) ? null : (double?)double.Parse(values[valueColumnIndex]);

                    _aquariusAdapter.PersistTimeSeriesData(aquariusId, AquariusHelper.ConstructAquariusInsertString(dateTime, 0, actualValue));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}