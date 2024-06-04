using System;
using HarmonyLib;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;

namespace Heck
{
    public static class CodeMatcherExtensions
    {
#if DEBUG
        [PublicAPI]
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher, Logger logger, string seperator = "\t")
        {
            logger.Info("Printing instructions:");
            codeMatcher.Instructions().ForEach(n => logger.Info(seperator + n));
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
