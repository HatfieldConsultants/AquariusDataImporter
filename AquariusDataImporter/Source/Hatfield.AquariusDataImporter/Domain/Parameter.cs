using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hatfield.AquariusDataImporter.Domain
{
    public class Parameter : IDomainModel
    {
        public virtual int Id { get; protected set; }
        public virtual string DataSetName { get; set; }
    }
}
