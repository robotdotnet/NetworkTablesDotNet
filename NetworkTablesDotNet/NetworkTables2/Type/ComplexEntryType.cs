using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public abstract class ComplexEntryType : NetworkTableEntryType
    {
        protected ComplexEntryType(byte id, string name) : base(id, name)
        {
            
        }

        public abstract object InternalizeValue(string key, object externalRepresentation, object currentInteralValue);
        public abstract void ExportValue(string key, object internalData, object externalRepresentation);
    }
}
