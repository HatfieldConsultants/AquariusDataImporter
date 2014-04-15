using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using Hatfield.AquariusDataImporter.Domain;
using Hatfield.AquariusDataImporter.Data;

namespace Hatfield.AquariusDataImporter.Data
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {

            var shouldMap = type.FullName == "Hatfield.AquariusDataImporter.Domain.ImportTask";
            return shouldMap;

        }

        public override bool IsId(Member member)
        {
            return member.Name == "Id";
        }
    }
}
