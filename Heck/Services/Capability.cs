using System;
using System.Reflection;
using HarmonyLib;
using IPA.Loader;

namespace Heck
{
    public class Capability
    {
        private static Action<string>? _register;
        private static Action<string>? _deregister;
        private static bool _initialized;

        private readonly string _capability;

        public Capability(string capability)
        {
            _capability = capability;

            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Assembly? assembly = PluginManager.GetPlugin("SongCore")?.Assembly;
            if (assembly == null)
            {
                return;
            }

            Type collections = assembly.GetType("SongCore.Collections");
            MethodInfo register = AccessTools.Method(collections, "RegisterCapability");
            _register = (Action<string>)Delegate.CreateDelegate(collections, register);
            MethodInfo deregister = AccessTools.Method(collections, "DeregisterizeCapability");
            _deregister = (Action<string>)Delegate.CreateDelegate(collections, deregister);
        }

        public void Register()
        {
            _register?.Invoke(_capability);
        }

        public void Deregister()
        {
            _deregister?.Invoke(_capability);
        }
    }
}
