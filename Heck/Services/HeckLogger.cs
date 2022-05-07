using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using IPA.Logging;
using JetBrains.Annotations;

// ReSharper disable ExplicitCallerInfoArgument
namespace Heck
{
    public class HeckLogger
    {
        public HeckLogger(Logger logger)
        {
            IPALogger = logger;
        }

        public Logger IPALogger { get; }

        public void Log(object? obj, Logger.Level level = Logger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Log(obj?.ToString(), level, member, line);
        }

        public void Log(string? message, Logger.Level level = Logger.Level.Debug, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            if (level == Logger.Level.Trace && HeckController.DebugMode)
            {
                level = Logger.Level.Info;
            }

            message ??= "NULL";
            IPALogger.Log(level, HeckController.DebugMode ? $"{member}({line}): {message}" : message);
        }

        [PublicAPI]
        public void PrintHarmonyInfo(MethodBase method)
        {
            Patches patches = Harmony.GetPatchInfo(method);
            if (patches == null)
            {
                Log($"{method.Name} is not patched");
                return;
            }

            Log("all owners: " + string.Join(", ", patches.Owners));

            Log("=============================");
            foreach (Patch patch in patches.Postfixes.Concat(patches.Prefixes).Concat(patches.Transpilers))
            {
                Log("index: " + patch.index);
                Log("owner: " + patch.owner);
                Log("patch method: " + patch.PatchMethod.FullDescription());
                Log("priority: " + patch.priority);
                Log("before: " + string.Join(", ", patch.before));
                Log("after: " + string.Join(", ", patch.after));
                Log("=============================");
            }
        }
    }
}
