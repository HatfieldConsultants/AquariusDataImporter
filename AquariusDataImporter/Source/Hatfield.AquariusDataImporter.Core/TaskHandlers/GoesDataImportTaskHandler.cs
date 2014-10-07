using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.FtpClient;
using System.IO;
using Hatfield.AquariusDataImporter.Core.Models;
using Hatfield.AquariusDataImporter.Core.Models.Goes;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;
using log4net;

namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class GoesDataImportTaskHandler : IImportTaskHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Application");

        private IAquariusAdapter _aquariusAdapter;

        public GoesDataImportTaskHandler(IAquariusAdapter aquariusAdapter)
        {
            _aquariusAdapter = aquariusAdapter;
        }

        public ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            var castedTask = task as GoesDataImportTask;
            if(castedTask == null)
            {
                log.Error("Casted to GOES import task fail");
                throw new InvalidCastException("Casted to GOES import task fail");
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

            
            Dictionary<string, IEnumerable<GoesValueData>> goesDataFromFTP = new Dictionary<string, IEnumerable<GoesValueData>>();
            if(Cache.GoesDataCache != null && Cache.GoesDataCache.Any())
            {
                goesDataFromFTP = Cache.GoesDataCache;
            }
            else
            {
                goesDataFromFTP = DownloadDataFromFTP();
                Cache.GoesDataCache = goesDataFromFTP;
            }

            
            return PersistData(goesDataFromFTP, castedTask);
        }

        private ImportResult PersistData(Dictionary<string, IEnumerable<GoesValueData>> dataFromFiles, GoesDataImportTask task)
        {
            var message = new StringBuilder();
            foreach (var parameter in task.Parameters)
            {
                var dataSetName = parameter.ParameterNameInFile;
                var dataSetIdentifier = parameter.ParameterIdentifier;

                try
                {                    
                    if (dataFromFiles.ContainsKey(dataSetName))
                    {
                        var values = dataFromFiles[dataSetName];
                        var valueStringBuilder = new StringBuilder();
                        foreach(var value in values)
                        {
                            valueStringBuilder.Append(AquariusHelper.ConstructAquariusInsertString(value.DateTime, 0, value.Value));
                        }
                        var aquariusId = _aquariusAdapter.GetDataSetIdByIdentifier(dataSetIdentifier);

                        var success = _aquariusAdapter.PersistTimeSeriesData(aquariusId, valueStringBuilder.ToString());
                        if (success)
                        {
                            message.AppendLine("Save data successfully for parameter:" + dataSetIdentifier);
                        }
                        else
                        {
                            message.AppendLine("Save data fail for parameter:" + dataSetIdentifier);
                        }
                    }
                    else
                    {
                        log.Warn("No data is found for " + dataSetName + ". Importer would skip this parameter");
                        message.AppendLine("No data is found for " + dataSetName + ". Importer would skip this parameter");
                    }
                    
                }
                catch(Exception ex)
                {
                    log.Error("Import Goes data file fail due to " + ex.StackTrace);
                    return new ImportResult { 
                        Success = false,
                        LogMessage = "Import Goes data file fail:" + ex.Message
                    };
                }
                
            }

            return new ImportResult { 
                Success = true,
                LogMessage = "Import GOES data for station " + task.StationName + " successfully." + message.ToString()
            };
        
        }

        private Dictionary<string, IEnumerable<GoesValueData>> DownloadDataFromFTP()
        {
            var data = new Dictionary<string, IEnumerable<GoesValueData>>();

            var allFilesInFTP = GoesDataHelper.GetAllFilesFromFTP();
            log.Info("Found " + allFilesInFTP.Count() + " files on the FTP site to process");

            foreach (var listItem in allFilesInFTP)
            {
                var dataInSingleFile = GoesDataHelper.DownloadAndTransformDataForSingleFile(listItem, log);
                MergeDictionaries(data, dataInSingleFile);
            }

            return data;
        
        }

        private void MergeDictionaries(Dictionary<string, IEnumerable<GoesValueData>> original, Dictionary<string, IEnumerable<GoesValueData>> newDic)
        { 
            foreach(var key in newDic.Keys)
            {
                if(original.ContainsKey(key))
                {
                    original[key] = original[key].Concat(newDic[key]);
                }
                else
                {
                    original.Add(key, newDic[key]);
                }
            }
        
        }
    }
}
