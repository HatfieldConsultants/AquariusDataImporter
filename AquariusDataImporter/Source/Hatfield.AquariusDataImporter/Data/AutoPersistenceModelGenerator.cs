using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using Hatfield.AquariusDataImporter.Domain;
namespace Hatfield.AquariusDataImporter.Data
{

    /// <summary>
    /// Generates the automapping for the domain assembly
    /// </summary>
    public class AutoPersistenceModelGenerator
    {
        public AutoPersistenceModel GenerateDataBaseMapping()
        {
            var mappings = AutoMap.AssemblyOf<ImportTask>(new AutomappingConfiguration());                       
            
            mappings.UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();
            

            return mappings;
        }

    }
}
