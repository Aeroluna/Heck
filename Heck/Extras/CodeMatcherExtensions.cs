using System;
using BepInEx.Logging;
using BSIPA_Utilities;
using HarmonyLib;
using JetBrains.Annotations;

namespace Heck
{
#if DEBUG
    public static class CodeMatcherExtensions
    {
        [PublicAPI]
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher, ManualLogSource logger, string seperator = "\t")
        {
            logger.LogInfo("Printing instructions:");
            codeMatcher.Instructions().ForEach(n => logger.LogInfo(seperator + n));
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
#endif
}
