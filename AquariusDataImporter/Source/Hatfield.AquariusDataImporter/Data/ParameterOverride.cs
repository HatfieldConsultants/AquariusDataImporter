using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Hatfield.AquariusDataImporter.Domain;
using NHibernate.Type;

namespace Hatfield.AquariusDataImporter.Data
{
    public class ParameterOverride : IAutoMappingOverride<Parameter>
    {
        public void Override(AutoMapping<Parameter> mapping)
        {
            mapping.Table("parameter");
            mapping.Id(x => x.Id, "Id");
        }
        
    }
}
