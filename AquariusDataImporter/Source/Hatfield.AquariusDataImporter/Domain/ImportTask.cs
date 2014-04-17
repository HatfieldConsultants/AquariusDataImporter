using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Domain
{
    public class ImportTask : IDomainModel
    {
        public virtual int Id { get; protected set; }
        public virtual string Name { get; set; }
        public virtual string HandlerName { get; set; }
        public virtual DateTime? LastImportTime { get; set; }
        public virtual string LastImportLog { get; set; }
        public virtual int ExecuteInterval { get; set; }
        public virtual byte[] DefinitionJsonString { get; set; }
    }
}
