#if DEBUG
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Logging;
using JetBrains.Annotations;

namespace Heck;

public static class LoggerExtensions
{
    [PublicAPI]
    public static void PrintHarmonyInfo(Logger logger, MethodBase method)
    {
        Patches patches = Harmony.GetPatchInfo(method);
        if (patches == null)
        {
            logger.Debug($"{method.Name} is not patched");
            return;
        }

        logger.Debug("all owners: " + string.Join(", ", patches.Owners));

        logger.Debug("=============================");
        foreach (Patch patch in patches.Postfixes.Concat(patches.Prefixes).Concat(patches.Transpilers))
        {
            logger.Debug("index: " + patch.index);
            logger.Debug("owner: " + patch.owner);
            logger.Debug("patch method: " + patch.PatchMethod.FullDescription());
            logger.Debug("priority: " + patch.priority);
            logger.Debug("before: " + string.Join(", ", patch.before));
            logger.Debug("after: " + string.Join(", ", patch.after));
            logger.Debug("=============================");
        }
    }
}
#endif
