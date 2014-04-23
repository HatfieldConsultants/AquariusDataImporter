using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hatfield.AquariusDataImporter.Core.Models.Sutron;
using Hatfield.AquariusDataImporter.Core.Models;
using log4net;
using Hatfield.AquariusDataImporter.Core.Aquarius;
using Hatfield.AquariusDataImporter.Core.Helpers;


namespace Hatfield.AquariusDataImporter.Core.TaskHandlers
{
    public class SimpleSutronImportTaskHandler : IImportTaskHandler
    {
        private static readonly ILog log = LogManager.GetLogger("Application");

        private IAquariusAdapter _aquariusAdapter;

        public SimpleSutronImportTaskHandler(IAquariusAdapter aquariusAdapter)
        {
            _aquariusAdapter = aquariusAdapter;
        }

        public virtual ImportResult Import(IImportable task, DateTime? lastImportTime, int interval)
        {
            var castedTask = task as SimpleSutronImportTask;
            if (castedTask == null)
            {
                throw new InvalidCastException("Casted to Simple Sutron Import Task fail");
            }

            //still within interval
            if(lastImportTime.HasValue && (DateTime.Now < lastImportTime.Value.AddMinutes(interval)))
            {
                return new ImportResult { 
                    Success = false,
                    LogMessage = "Task still within the execute interval, skip"
                };
            }

            

            var dataFileList = SutronDataDownloadHelper.FetchDownloadableDataFileList(castedTask.DownloadURL);

            var firstDateAtMidnight = DateTime.Now.AddHours(7).AddMinutes(-castedTask.NumberOfMinuteInThePast.Value);//since the program is run in BC, so we need to add 8 hours to be UTC time
            
            
            var filesNeedToImport = dataFileList.Where(x => x.LastModified >= firstDateAtMidnight && castedTask.IsMatchedDefinition(x.FileName)).ToList();

            var errorThreshold = 10;

            var columnIndexAndAquariusIdDictionary = GetParametersIndexAquariusIdDictionary(castedTask);
            foreach(var file in filesNeedToImport)
            {
                try
                {
                    var dataFileString = SutronDataDownloadHelper.DownloadSingleDataFile(castedTask.DownloadURL, file);
                    ImportDataToAquarius(dataFileString, castedTask, columnIndexAndAquariusIdDictionary);
                }
                catch(Exception ex)
                {
                    errorThreshold--;
                    if (errorThreshold < 0)
                    {
                        return new ImportResult
                        {
                            Success = false,
                            LogMessage = "Task execute fail"
                        };
                    }
                    else
                    {
                        log.Warn("Importer fail try to continue" + ex.StackTrace);
                        continue;
                    }
                }
                
            }

            return new ImportResult
            {
                Success = true,
                LogMessage = "Task execute successfully"
            };
            
        }

        protected virtual void ImportDataToAquarius(string dataFileString, SimpleSutronImportTask task, Dictionary<int, long> paramentIdDictionary)
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
                        if(!paramentIdDictionary.Keys.Contains(i))
                        {
                            continue;
                        }
                        var appendDataString = AquariusHelper.ConstructAquariusInsertString(line[1],
                                                                             line[2],
                                                                             -7,
                                                                             ToNullable<double>(line[i]));
                        if(!string.IsNullOrEmpty(appendDataString))
                        {
                            _aquariusAdapter.PersistTimeSeriesData(paramentIdDictionary[i], appendDataString);
                        }
                        
                    }
                }
                
            }
            catch (Exception)
            {

                throw;
            }
        
        }

        //splict the download string into multiple lines
        protected IList<string[]> ExtractDataFromDownloadString(string downLoadDataString)
        {
            var lines = downLoadDataString.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            return lines.Select(line => line.Split(',').ToArray()).ToList();
        }

        /// <summary>
        /// Generate the parameter index and aquarius parameter Id
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        protected Dictionary<int, long> GetParametersIndexAquariusIdDictionary(SimpleSutronImportTask task)
        {
            var parameter = task.Parameters.ToList();
            var dictionary = new Dictionary<int, long>();
            parameter.ForEach(x => dictionary.Add(x.ColumnIndex, _aquariusAdapter.GetDataSetIdByIdentifier(x.Identifier)));

            return dictionary;
        }

        

        protected static Nullable<T> ToNullable<T>(string s) where T : struct
        {
            Nullable<T> result = new Nullable<T>();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFrom(s);
                }
            }
            catch { }
            return result;
        }

        

        

        

        
    }
}