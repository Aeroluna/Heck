using System;
using System.Runtime.CompilerServices;
using IPA.Logging;

// ReSharper disable ExplicitCallerInfoArgument
namespace Heck
{
    // TODO: remove entirely
    public class HeckLogger
    {
        [Obsolete("Use IPA logger", true)]
        public HeckLogger(Logger _)
        {
        }

        public Logger IPALogger => throw new NotImplementedException();

        public void Log(object? obj, Logger.Level level = Logger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public void Log(string? message, Logger.Level level = Logger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }
    }
}
