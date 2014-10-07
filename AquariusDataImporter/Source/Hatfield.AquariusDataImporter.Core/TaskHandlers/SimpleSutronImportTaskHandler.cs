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
using System.Threading.Tasks;
using System.Diagnostics;

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
            var numberOfFileBeenImport = 0;
            var castedTask = task as SimpleSutronImportTask;
            if (castedTask == null)
            {
                throw new InvalidCastException("Casted to Simple Sutron Import Task fail");
            }

            //still within interval
            if(!ShouldExecute(lastImportTime, interval))
            {
                return new ImportResult
                {
                    Success = false,
                    LogMessage = "Task still within the execute interval, skip"
                };
            }


            var firstDateAtMidnight = DateTime.Now.AddHours(7).AddMinutes(-castedTask.NumberOfMinuteInThePast.Value);//since the program is run in BC, so we need to add 8 hours to be UTC time

            IEnumerable<SutronDataFile> dataFileList = null;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            if (Cache.SutronDataFileCache == null)
            {
                var downloadDataFileListTask = Task.Factory.StartNew(() => SutronDataDownloadHelper.FetchDownloadableDataFileList(castedTask.DownloadURL));
                Task.WaitAll(downloadDataFileListTask);
                dataFileList = downloadDataFileListTask.Result;
                Cache.SutronDataFileCache = dataFileList;
            }
            else
            {
                dataFileList = Cache.SutronDataFileCache;
            }

            

            var filesNeedToImport = dataFileList.Where(x => x.LastModified >= firstDateAtMidnight && castedTask.IsMatchedDefinition(x.FileName)).ToList();
            var numberOfFileNeedToImport = filesNeedToImport.Count;
            var errorThreshold = 10;

            Dictionary<int, long> columnIndexAndAquariusIdDictionary = null;
            try
            {
                columnIndexAndAquariusIdDictionary = GetParametersIndexAquariusIdDictionary(castedTask);
            }
            catch(Exception ex)
            {
                return new ImportResult
                {
                    Success = false,
                    LogMessage = "Task execute fail. " + ex.Message
                };
            }

            

            Parallel.ForEach(filesNeedToImport, file => {
                    try
                    {
                        Console.WriteLine(numberOfFileBeenImport + "/" + numberOfFileNeedToImport + " files has been imported");
                        var dataFileString = SutronDataDownloadHelper.DownloadSingleDataFile(castedTask.DownloadURL, file);
                        ImportDataToAquarius(dataFileString, castedTask, columnIndexAndAquariusIdDictionary);

                        numberOfFileBeenImport++;
                    }
                    catch(Exception ex)
                    {
                        log.Warn("Importer fail try to continue" + ex.StackTrace);
                        errorThreshold--;
                    }
        
            });

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("It takes " + ts.Seconds + " seconds to import when asynchroinization");

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
                return new ImportResult
                {
                    Success = true,
                    LogMessage = "Task execute successfully. " + numberOfFileBeenImport + " files have been imported to Aquarius."
                };

            }
                        
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
            var parameters = task.Parameters.Where(x => x.ColumnIndex.HasValue).ToList();
            var dictionary = new Dictionary<int, long>();
            Parallel.ForEach(parameters, parameter => {
                var aquariusId = _aquariusAdapter.GetDataSetIdByIdentifier(parameter.Identifier);
                lock(dictionary)
                {
                    dictionary.Add(parameter.ColumnIndex.Value, aquariusId);    
                }
            });
            

            return dictionary;
        }


        protected virtual bool ShouldExecute(DateTime? lastImportTime, int interval)
        {
            if (lastImportTime.HasValue && (DateTime.Now < lastImportTime.Value.AddMinutes(interval)))
            {
                return false;
            }
            else
            {
                return true;
            }
        
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