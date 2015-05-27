using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Type;

namespace NetworkTablesDotNet.NetworkTables2
{
    public class TableKeyExistsWithDifferentTypeException : SystemException
    {
        public TableKeyExistsWithDifferentTypeException(String existingKey, NetworkTableEntryType existingType) :this(existingKey, existingType, "")
        {
            
        }

        public TableKeyExistsWithDifferentTypeException(String existingKey, NetworkTableEntryType existingType, String message) : base("Illegal put - key '" + existingKey + "' exists with type '" + existingType + "'. " + message)
        {
            
        }
    }
}
