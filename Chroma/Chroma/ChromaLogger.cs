namespace Chroma
{
    using System.Runtime.CompilerServices;
    using IPALogger = IPA.Logging.Logger;

    internal static class ChromaLogger
    {
        internal static IPALogger IPAlogger { get; set; }

        internal static void Log(object obj, IPALogger.Level level = IPALogger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Log(obj.ToString(), level, member, line);
        }

        internal static void Log(string message, IPALogger.Level level = IPALogger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            IPAlogger.Log(level, $"{member}({line}): {message}");
        }
    }
}
