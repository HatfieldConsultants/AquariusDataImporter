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
            log.Debug("# of tasks " + allTasks.Count);

            foreach (var taskDomain in allTasks)
            {
                try
                {
                    log.Debug("Create task " + taskDomain.Name);
                    var task = ImportTaskFactory.CreateImportTask(taskDomain);
                    log.Debug("Create task handler " + taskDomain.HandlerName);
                    var handler = TaskHandlerFactory.CreateTaskHandler(taskDomain.HandlerName);

                    var result = handler.Import(task, taskDomain.LastImportTime, taskDomain.ExecuteInterval);

                    SaveImportLog(taskDomain, result, dbSession);

                    log.Info("Data importer runs successfully");
                }
                catch(Exception ex)
                {
                    log.Error("handle task" + taskDomain.Name + "fail due to " + ex.StackTrace);
                }
            }
        }

        private static void SaveImportLog(ImportTask task, ImportResult result, ISession dbSession)
        {             
            task.LastImportTime = DateTime.Now;
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
