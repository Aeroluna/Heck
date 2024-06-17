using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Heck
{
    // TODO: use categories instead
    // nvm, bsipa needs to update its harmony ver
    internal class HeckPatcher
    {
        private readonly Harmony _harmony;
        private readonly HashSet<Type> _types = new();
        private bool _enabled;

        internal HeckPatcher(Assembly assembly, string harmonyId, object? id)
        {
            Id = id;
            Plugin.Log.Trace($"Initializing patches for Harmony instance [{harmonyId}] in [{assembly.GetName()}]");

            _harmony = new Harmony(harmonyId);

            foreach (Type type in AccessTools.GetTypesFromAssembly(assembly))
            {
                HeckPatchAttribute? heckPatch = type.GetCustomAttribute<HeckPatchAttribute>(false);

                if (heckPatch == null)
                {
                    continue;
                }

                if (heckPatch.Id != null)
                {
                    if (!heckPatch.Id.Equals(id))
                    {
                        continue;
                    }

                    _types.Add(type);
                }
                else if (id == null)
                {
                    _types.Add(type);
                }
            }
        }

        internal object? Id { get; }

        internal bool Enabled
        {
            set
            {
                if (value == _enabled)
                {
                    return;
                }

                Plugin.Log.Trace($"Toggling [{_harmony.Id}] to [{value}]");
                _enabled = value;
                if (value)
                {
                    _types.Do(_harmony.PatchAll);
                }
                else
                {
                    _harmony.UnpatchSelf();
                }
            }
        }
    }
}
