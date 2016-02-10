using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static NetworkTables.NtCore;

namespace NetworkTables
{
    internal class Logger
    {
        private static Logger s_instance;

        public static Logger Instance => s_instance ?? (s_instance = new Logger());

        //public delegate void LogFunc(uint level, string file, uint line, string msg);

        private Logger()
        {
            m_func = DefLogFunc;
        }

        private LogFunc m_func;
        public void SetLogger(LogFunc func)
        {
            m_func = func;
        }

        public void SetMinLevel(uint level)
        {
            m_minLevel = level;
        }

        public uint MinLevel()
        {
            return m_minLevel;
        }

        public void Log(uint level, string file, uint line, string msg)
        {
            if (m_func == null || level < m_minLevel) return;
            m_func(level, file, line, msg);
        }

        private uint m_minLevel = 20;

        public bool HasLogger()
        {
            return m_func != null;
        }

        private static void DefLogFunc(uint level, string file, uint line, string msg)
        {
            if (level == 20)
            {
                Console.Error.WriteLine($"NT: {msg}");
            }

            string levelmsg = "";
            if (level >= 50)
                levelmsg = "CRITICAL";
            else if (level >= 40)
                levelmsg = "ERROR";
            else if (level >= 30)
                levelmsg = "WARNING";
            else
                return;
            string fname = Path.GetFileName(file);
            Console.Error.WriteLine($"NT: {levelmsg}: {msg} ({fname}:{line}");
        }


        public static void Log(uint level, string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            do
            {
                Logger logger = Logger.Instance;
                if (logger.MinLevel() <= level && logger.HasLogger())
                {
                    logger.Log(level, filePath, (uint)lineNumber, msg);
                }
            }
            while (false);
        }

        public static void Error(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogError, msg, memberName, filePath, lineNumber);
        }

        public static void Waring(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogWarning, msg, memberName, filePath, lineNumber);
        }

        public static void Info(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogInfo, msg, memberName, filePath, lineNumber);
        }


        public static void Debug(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogDebug, msg, memberName, filePath, lineNumber);
        }

        public static void Debug1(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogDebug1, msg, memberName, filePath, lineNumber);
        }

        public static void Debug2(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogDebug2, msg, memberName, filePath, lineNumber);
        }

        public static void Debug3(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogDebug3, msg, memberName, filePath, lineNumber);
        }

        public static void Debug4(string msg, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log((uint)LogLevel.LogDebug4, msg, memberName, filePath, lineNumber);
        }
    }
}
