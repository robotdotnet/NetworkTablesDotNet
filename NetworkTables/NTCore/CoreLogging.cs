using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NTCore
{
    public static class CoreLogging
    {
        public delegate void NTLogger(int level, string file, int line, string msg);

        private static bool logSet = false;

        private static Interop.NT_LogFunc nativeLog;

        public static void SetLogFunction(NTLogger logFunc, NT_LogLevel level)
        {
            if (!logSet)
            {
                logSet = true;
                nativeLog = (u, file, line, msg) =>
                {
                    string message = CoreMethods.ReadUTF8String(msg);
                    string fileName = CoreMethods.ReadUTF8String(file);

                    logFunc((int)u, fileName, (int)line, message);
                };
                Interop.NT_SetLogger(nativeLog, (uint)level);
            }
        }
    }
}
