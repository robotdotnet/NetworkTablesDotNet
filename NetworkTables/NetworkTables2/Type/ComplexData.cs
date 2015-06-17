using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Type
{
    public class ComplexData
    {
        private readonly ComplexEntryType type;
        public ComplexData(ComplexEntryType type)
        {
            this.type = type;
        }

        public new ComplexEntryType GetType()
        {
            return type;
        }
    }
}
