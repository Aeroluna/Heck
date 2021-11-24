using System;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;

namespace Heck
{
    public static class CodeMatcherExtensions
    {
        [PublicAPI]
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher, HeckLogger logger, string seperator = "\n\t")
        {
            logger.Log(string.Join(seperator, codeMatcher.Instructions()));
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
