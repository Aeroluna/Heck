namespace Heck
{
    public static class CodeMatcherExtensions
    {
#if DEBUG
        [PublicAPI]
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher, HeckLogger logger, string seperator = "\t")
        {
            logger.Log("Printing instructions:");
            Logger ipaLogger = logger.IPALogger;
            codeMatcher.Instructions().ForEach(n => ipaLogger.Log(Logger.Level.Info, seperator + n));
            return codeMatcher;
        }

        [PublicAPI]
        public static CodeMatcher ThrowLastError(this CodeMatcher codeMatcher)
        {
            if (codeMatcher.IsInvalid)
            {
                throw new InvalidOperationException(codeMatcher.GetField<string, CodeMatcher>("lastError"));
            }

            return codeMatcher;
        }
#endif
    }
}
