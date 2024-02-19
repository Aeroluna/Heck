using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace Heck
{
    public class HeckPatcher
    {
        private readonly Harmony _harmony;
        private readonly HashSet<Type> _types = new();
        private bool _enabled;

        public HeckPatcher(string harmonyId, object? id = null)
        {
            Assembly assembly = new StackTrace().GetFrame(1).GetMethod().ReflectedType!.Assembly;

            Plugin.Log.Trace($"Initializing patches for Harmony instance [{harmonyId}] in [{assembly.GetName()}]");

            _harmony = new Harmony(harmonyId);

            foreach (Type type in AccessTools.GetTypesFromAssembly(assembly))
            {
                HeckPatch? heckPatch = type.GetCustomAttribute<HeckPatch>(false);

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

        [PublicAPI]
        public bool Enabled
        {
            get => _enabled;
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
