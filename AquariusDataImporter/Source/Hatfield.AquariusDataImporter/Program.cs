using log4net;
using log4net.Config;
using System.IO;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;
using Hatfield.AquariusDataImporter.Data;
using Hatfield.AquariusDataImporter.Domain;
using Hatfield.AquariusDataImporter.Core;
using Hatfield.AquariusDataImporter.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Hatfield.AquariusDataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = System.Configuration.ConfigurationManager.AppSettings["ProgramFolder"];
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(System.Environment.CurrentDirectory, ("log4net.config"))));
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(path, ("log4net.config"))));
            var log = log4net.LogManager.GetLogger("Application");

            
            log.Debug("Get all tasks from database");

            var dbSession = CreateSessionFactory(path).OpenSession();
            var allTasks = dbSession.QueryOver<ImportTask>().List();
            var allParameters = dbSession.QueryOver<Parameter>().List();

            bool allGOESTasksImportSuccess = true;//this variable is used to check if all GOES tasks import success

            log.Debug("# of tasks " + allTasks.Count);

            foreach (var taskDomain in allTasks)
            {
                try
                {
                    log.Debug("Create task " + taskDomain.Name);
                    var task = ImportTaskFactory.CreateImportTask(taskDomain, allParameters);
                    log.Debug("Create task handler " + taskDomain.HandlerName);
                    var handler = TaskHandlerFactory.CreateTaskHandler(taskDomain.HandlerName);

                    var result = handler.Import(task, taskDomain.LastImportTime, taskDomain.ExecuteInterval);

                    if(taskDomain.HandlerName == Constants.GoesDataImporterName && !result.Success)
                    {
                        allGOESTasksImportSuccess = false;
                    }

                    SaveImportLog(taskDomain, result, dbSession);

                    
                }
                catch(Exception ex)
                {
                    log.Error("handle task" + taskDomain.Name + "fail due to " + ex.StackTrace);
                }
            }

            //if all GOES tasks import successfully
            //clean the folder
            //if(allGOESTasksImportSuccess)
            //{
            //    Hatfield.AquariusDataImporter.Core.Helpers.GoesDataHelper.MoveProcessedFile(true, log);
            //}

            log.Info("Data importer runs successfully");
        }

        private static void SaveImportLog(ImportTask task, ImportResult result, ISession dbSession)
        {             
            if(result.Success)
            {
                task.LastImportTime = DateTime.Now;
            }
            
            task.LastImportLog = result.LogMessage;

            try
            {
                dbSession.BeginTransaction();
                dbSession.SaveOrUpdate(task);
                dbSession.Transaction.Commit();
            }
            catch(Exception ex)
            {
                dbSession.Transaction.Rollback();
                throw ex;
            }
            
        }

        // Returns our session factory
        public static ISessionFactory CreateSessionFactory(string path)
        {
            // read hibernate.cfg.xml
            Configuration config = new Configuration().Configure(Path.Combine(path, "Hibernate.cfg.xml"));
            // load mappings from this assembly
            return Fluently.Configure(config)
                .Mappings(m => m.AutoMappings.Add(new AutoPersistenceModelGenerator().GenerateDataBaseMapping()))
                .CurrentSessionContext<WebSessionContext>()
                .BuildSessionFactory();
        }
    }
}
