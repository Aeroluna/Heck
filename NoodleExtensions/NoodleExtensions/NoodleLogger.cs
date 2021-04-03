namespace NoodleExtensions
{
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using HarmonyLib;
    using IPALogger = IPA.Logging.Logger;

    internal static class NoodleLogger
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

        internal static void PrintHarmonyInfo(MethodBase method)
        {
            Patches patches = Harmony.GetPatchInfo(method);
            if (patches is null)
            {
                Log($"{method.Name} is not patched");
            }

            Log("all owners: " + string.Join(", ", patches.Owners));

            Log("=============================");
            foreach (Patch patch in patches.Postfixes.Concat(patches.Prefixes).Concat(patches.Transpilers))
            {
                Log("index: " + patch.index);
                Log("owner: " + patch.owner);
                Log("patch method: " + patch.PatchMethod.Name);
                Log("priority: " + patch.priority);
                Log("before: " + string.Join(", ", patch.before));
                Log("after: " + string.Join(", ", patch.after));
                Log("=============================");
            }
        }
    }
}
