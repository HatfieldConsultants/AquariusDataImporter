using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Hatfield.AquariusDataImporter.Domain;

namespace Hatfield.AquariusDataImporter.Data
{
    public class ImportTaskOverride : IAutoMappingOverride<ImportTask>
    {
        public void Override(AutoMapping<ImportTask> mapping)
        {
            mapping.Table("importtask");
            mapping.Id(x => x.Id, "Id");
            mapping.Map(x => x.DefinitionJsonString).Length(99999);
        }
    }
}
