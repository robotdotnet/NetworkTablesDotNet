using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTables.NetworkTables2.Type
{
    public abstract class ComplexEntryType : NetworkTableEntryType
    {
        protected ComplexEntryType(byte id, string name) : base(id, name)
        {
            
        }

        public abstract object InternalizeValue(string key, object externalRepresentation, object currentInternalValue);
        public abstract void ExportValue(string key, object internalData, object externalRepresentation);
    }
}
