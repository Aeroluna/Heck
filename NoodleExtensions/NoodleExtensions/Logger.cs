using System;
using System.Runtime.CompilerServices;
using IPALogger = IPA.Logging.Logger;

namespace NoodleExtensions
{
    internal static class Logger
    {
        public static IPALogger logger { get; set; }

        public static void Log(Exception e, IPALogger.Level level = IPALogger.Level.Error,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log(e.ToString(), level);
        }

        public static void Log(object obj, IPALogger.Level level = IPALogger.Level.Debug,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log(obj.ToString(), level, member, line);
        }

        public static void Log(string message, IPALogger.Level level = IPALogger.Level.Debug,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            if (Plugin.DebugMode)
            {
                logger.Log(level, $"{member}({line}): {message}");
            }
            else
            {
                logger.Log(level, $"{message}");
            }
        }
    }
}