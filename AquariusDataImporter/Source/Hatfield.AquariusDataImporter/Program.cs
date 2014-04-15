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
using System;

namespace Hatfield.AquariusDataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(System.Environment.CurrentDirectory, ("log4net.config"))));
            var log = log4net.LogManager.GetLogger("Application");

            var sessionFactory = CreateSessionFactory();
            log.Debug("Get all tasks from database");
            var allTasks = sessionFactory.OpenSession().QueryOver<ImportTask>().List();
            log.Debug("# of tasks " + allTasks.Count);

            foreach(var taskDomain in allTasks)
            {
                try
                {
                    log.Debug("Create task " + taskDomain.Name);
                    var task = ImportTaskFactory.CreateImportTask(taskDomain);
                    log.Debug("Create task handler " + taskDomain.HandlerName);
                    var handler = TaskHandlerFactory.CreateTaskHandler(taskDomain.HandlerName);

                    handler.Import(task);
                }
                catch(Exception ex)
                {
                    log.Error("handle task" + taskDomain.Name + "fail due to " + ex.StackTrace);
                }
            }
        }

        // Returns our session factory
        public static ISessionFactory CreateSessionFactory()
        {
            // read hibernate.cfg.xml
            Configuration config = new Configuration().Configure("Hibernate.cfg.xml");
            // load mappings from this assembly
            return Fluently.Configure(config)
                .Mappings(m => m.AutoMappings.Add(new AutoPersistenceModelGenerator().GenerateDataBaseMapping()))
                .CurrentSessionContext<WebSessionContext>()
                .BuildSessionFactory();
        }
    }
}
