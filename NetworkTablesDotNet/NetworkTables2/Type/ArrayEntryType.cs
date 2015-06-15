using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using NetworkTablesDotNet.NetworkTables2.Stream;

namespace NetworkTablesDotNet.NetworkTables2.Type
{
    public class ArrayEntryType : ComplexEntryType
    {
        private readonly NetworkTableEntryType m_elementType;

        public ArrayEntryType(byte id, NetworkTableEntryType elementType)
            : base(id, "Array of [" + elementType.name + "]")
        {
            this.m_elementType = elementType;
        }

        public override void SendValue(object value, BinaryWriterBE os)
        {
            if (value is object[])
            {
                object[] dataArray = (object[])value;
                if (dataArray.Length > 255)
                {
                    throw new IOException("Cannot write " + value + " as " + name + ". Arrays have a max length of 255 values");
                }
                os.Write((byte) dataArray.Length);
                foreach (var s in dataArray)
                {
                    m_elementType.SendValue(s, os);
                }
            }
            else
            {
                throw new IOException("Cannot write " + value + " as " + name);
            }
        }

        public override object ReadValue(BinaryReaderBE inStream)
        {
            int length = inStream.ReadByte();
            object[] dataArray = new object[length];
            for (int i = 0; i < length; ++i)
            {
                dataArray[i] = m_elementType.ReadValue(inStream);
            }
            return dataArray;
        }

        public override object InternalizeValue(string key, object externalRepresentation, object currentInternalValue)
        {
            ArrayData externalArrayData = externalRepresentation as ArrayData;
            if (externalArrayData == null)
                throw new TableKeyExistsWithDifferentTypeException(key, this,
                    externalRepresentation + " is not an ArrayData");
            object[] internalArray;

            if (currentInternalValue is object[] &&
                (internalArray = ((object[]) currentInternalValue)).Length == externalArrayData.Size())
            {
                Array.Copy(externalArrayData.GetDataArray(), 0, internalArray, 0, internalArray.Length);
                return internalArray;
            }
            else
            {
                internalArray = new object[externalArrayData.Size()];
                Array.Copy(externalArrayData.GetDataArray(), 0, internalArray, 0, internalArray.Length);

                return internalArray;
            }
        }

        public override void ExportValue(string key, object internalData, object externalRepresentation)
        {
            ArrayData externalArrayData = externalRepresentation as ArrayData;
            if (externalArrayData == null)
                throw new TableKeyExistsWithDifferentTypeException(key, this,
                    externalRepresentation + " is not an ArrayData");
            object[] internalArray = internalData as object[];
            if (internalArray == null)
            {
                throw new TableKeyExistsWithDifferentTypeException(key, this, "Internal data: " + internalData + " is not an array");
            }

            externalArrayData.SetSize(internalArray.Length);
            Array.Copy(internalArray, 0, externalArrayData.GetDataArray(), 0, internalArray.Length);
        }
    }
}
