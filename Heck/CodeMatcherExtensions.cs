namespace Heck
{
    using System;
    using HarmonyLib;
    using IPA.Utilities;

    public static class CodeMatcherExtensions
    {
        public static CodeMatcher PrintInstructions(this CodeMatcher codeMatcher, HeckLogger logger, string seperator = "\n\t")
        {
            logger.Log(string.Join(seperator, codeMatcher.Instructions()));
            return codeMatcher;
        }

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
