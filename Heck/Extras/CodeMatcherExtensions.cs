using System;
using HarmonyLib;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;

namespace Heck
{
    public static class CodeMatcherExtensions
    {
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
    }
}
