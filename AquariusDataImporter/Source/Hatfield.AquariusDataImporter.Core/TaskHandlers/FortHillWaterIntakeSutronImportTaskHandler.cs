using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Models.Sutron;
using log4net;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;

namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class FortHillWaterIntakeSutronImportTaskHandler : SimpleSutronImportTaskHandler, IImportTaskHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Application");

        private IAquariusAdapter _aquariusAdapter;

        private Dictionary<string, long> parameterIdDictionary = new Dictionary<string, long>();

        public FortHillWaterIntakeSutronImportTaskHandler(IAquariusAdapter aquariusAdapter) : base(aquariusAdapter)
        {
            _aquariusAdapter = aquariusAdapter;
        }

        public override Models.ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            InitialDataSetIdMapping(task as FortHillWaterIntakeImportTask);
            return base.Import(task, lastImportTime, interval);
        }

        protected override void ImportDataToAquarius(string dataFileString, SimpleSutronImportTask task, Dictionary<int, long> paramentIdDictionary)
        {
            var linesOfData = ExtractDataFromDownloadString(dataFileString);            

            var itemInLine = linesOfData[0].Length;

            try
            {
                foreach (var line in linesOfData)
                {
                    //start from the 3rd item in the array
                    //because the first item is project
                    //the second item is date
                    //the third item is time
                    for (int i = 3; i < itemInLine; i++)
                    {
                        //if the index is not in the dictionary, skip it
                        if (!paramentIdDictionary.Keys.Contains(i))
                        {
                            continue;
                        }
                        var appendDataString = AquariusHelper.ConstructAquariusInsertString(line[1],
                                                                             line[2],
                                                                             -7,
                                                                             ToNullable<double>(line[i]));
                        if (!string.IsNullOrEmpty(appendDataString))
                        {
                            _aquariusAdapter.PersistTimeSeriesData(paramentIdDictionary[i], appendDataString);
                        }

                    }
                    //This need to be turned on to insert data to calculated station
                    InsertDataToCalculationStation(line, task as FortHillWaterIntakeImportTask);

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void InsertDataToCalculationStation(string[] lineOfData, FortHillWaterIntakeImportTask task)
        {
            //we need to save the collect time to Aquarius, and the data is collect in Alberta
            //convert UTC time to Alberta time is -7 hours
            var dateTimeValue = DateTime.Parse(lineOfData[1] + " " + lineOfData[2]).AddHours(-7);


            var stationATSS = _aquariusAdapter.GetTimeSeriesData(parameterIdDictionary[task.StationATurbidityTSSIdentifier].ToString(), dateTimeValue, dateTimeValue).FirstOrDefault();
            var stationBTSS = _aquariusAdapter.GetTimeSeriesData(parameterIdDictionary[task.StationBTurbidityTSSIdentifier].ToString(), dateTimeValue, dateTimeValue).FirstOrDefault();
            var stationCTSS = _aquariusAdapter.GetTimeSeriesData(parameterIdDictionary[task.StationCTurbidityTSSIdentifier].ToString(), dateTimeValue, dateTimeValue).FirstOrDefault();

            if (stationATSS.Value.HasValue)
            {
                if (stationBTSS.Value.HasValue)
                {
                    var BALessThan250 = (stationATSS.Value < 250) ? (stationBTSS.Value - stationATSS.Value) : 0;
                    var BALargerThan250 = (stationATSS.Value > 250) ? ((stationBTSS.Value - stationATSS.Value) * 1.1) + 10 : 0;
                    _aquariusAdapter.PersistTimeSeriesData(parameterIdDictionary[task.TSSLess250BAIdentifier], AquariusHelper.ConstructAquariusInsertString(lineOfData[1], lineOfData[2], -7, BALessThan250));
                    _aquariusAdapter.PersistTimeSeriesData(parameterIdDictionary[task.TSSLarger250BAIdentifier], AquariusHelper.ConstructAquariusInsertString(lineOfData[1], lineOfData[2], -7, BALargerThan250));
                }
                if (stationCTSS.Value.HasValue)
                {
                    var CALessThan250 = (stationATSS.Value < 250) ? (stationCTSS.Value - stationATSS.Value) : 0;
                    var CALargerThan250 = (stationATSS.Value > 250) ? ((stationCTSS.Value - stationATSS.Value) * 1.1) + 10 : 0;


                    _aquariusAdapter.PersistTimeSeriesData(parameterIdDictionary[task.TSSLess250CAIdentifier], AquariusHelper.ConstructAquariusInsertString(lineOfData[1], lineOfData[2], -7, CALessThan250));
                    _aquariusAdapter.PersistTimeSeriesData(parameterIdDictionary[task.TSSLarger250CAIdentifier], AquariusHelper.ConstructAquariusInsertString(lineOfData[1], lineOfData[2], -7, CALargerThan250));
                }


            }

        }

        private void InitialDataSetIdMapping(FortHillWaterIntakeImportTask task)
        {
            parameterIdDictionary.Add(task.StationATurbidityTSSIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.StationATurbidityTSSIdentifier));
            parameterIdDictionary.Add(task.StationBTurbidityTSSIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.StationBTurbidityTSSIdentifier));
            parameterIdDictionary.Add(task.StationCTurbidityTSSIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.StationCTurbidityTSSIdentifier));

            parameterIdDictionary.Add(task.TSSLess250BAIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.TSSLess250BAIdentifier));
            parameterIdDictionary.Add(task.TSSLess250CAIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.TSSLess250CAIdentifier));
            parameterIdDictionary.Add(task.TSSLarger250BAIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.TSSLarger250BAIdentifier));
            parameterIdDictionary.Add(task.TSSLarger250CAIdentifier, _aquariusAdapter.GetDataSetIdByIdentifier(task.TSSLarger250CAIdentifier));
        
        }
    }
}
