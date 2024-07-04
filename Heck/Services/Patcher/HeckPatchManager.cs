using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IPA.Loader;

namespace Heck.Patcher
{
    public static class HeckPatchManager
    {
        private static readonly Dictionary<Assembly, List<HeckPatcher>> _patchers = new();

        public static void Register(string harmonyId, object? id = null)
        {
            Assembly assembly = new StackTrace().GetFrame(1).GetMethod().ReflectedType!.Assembly;
            HeckPatcher patcher = new(assembly, harmonyId, id);

            if (!_patchers.TryGetValue(assembly, out List<HeckPatcher> patchers))
            {
                patchers = new List<HeckPatcher>();
                _patchers[assembly] = patchers;
            }

            patchers.Add(patcher);

            if (id == null)
            {
                patcher.Enabled = true;
            }
        }

        internal static void Enable()
        {
            PluginManager.PluginEnabled += OnPluginEnabled;
            PluginManager.PluginDisabled += OnPluginDisabled;

            foreach (List<HeckPatcher> patchersValue in _patchers.Values)
            {
                foreach (HeckPatcher heckPatcher in patchersValue.Where(n => n.Id == null))
                {
                    heckPatcher.Enabled = true;
                }
            }
        }

        internal static void Disable()
        {
            PluginManager.PluginEnabled -= OnPluginEnabled;
            PluginManager.PluginDisabled -= OnPluginDisabled;

            foreach (List<HeckPatcher> patchersValue in _patchers.Values)
            {
                foreach (HeckPatcher heckPatcher in patchersValue)
                {
                    heckPatcher.Enabled = false;
                }
            }
        }

        private static void OnPluginEnabled(PluginMetadata metadata, bool _)
        {
            if (!_patchers.TryGetValue(metadata.Assembly, out List<HeckPatcher> patchers))
            {
                return;
            }

            HeckPatcher? patcher = patchers.FirstOrDefault(n => n.Id == null);
            if (patcher != null)
            {
                patcher.Enabled = true;
            }
        }

        private static void OnPluginDisabled(PluginMetadata metadata, bool _)
        {
            if (!_patchers.TryGetValue(metadata.Assembly, out List<HeckPatcher> patchers))
            {
                return;
            }

            foreach (HeckPatcher patcher in patchers)
            {
                patcher.Enabled = false;
            }
        }
    }
}
