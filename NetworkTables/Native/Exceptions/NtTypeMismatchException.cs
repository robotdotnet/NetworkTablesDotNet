using System;

namespace NetworkTables.Native.Exceptions
{
    public class NtTypeMismatchException : InvalidOperationException
    {
        public NtTypeMismatchException(NtType requested, NtType actual)
            : base($"Requested Type {requested} does not match actual Type {actual}.")
        {

        }
    }
}