using System;
using System.Reflection;
using HarmonyLib;

namespace Heck;

public class Capability
{
    private static Action<string>? _deregister;
    private static bool _initialized;
    private static Action<string>? _register;

    private readonly string _capability;

    public Capability(string capability)
    {
        _capability = capability;

        if (_initialized)
        {
            return;
        }

        _initialized = true;
        Type? collections = Type.GetType("SongCore.Collections, SongCore");
        if (collections == null)
        {
            return;
        }

        MethodInfo register = AccessTools.Method(collections, "RegisterCapability");
        _register = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), register);
#if !PRE_V1_37_1
        // random rename in 1.37.0 songcore
        MethodInfo deregister = AccessTools.Method(collections, "DeregisterCapability");
#else
        MethodInfo deregister = AccessTools.Method(collections, "DeregisterizeCapability");
#endif
        _deregister = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), deregister);
    }

    public void Deregister()
    {
        _deregister?.Invoke(_capability);
    }

    public void Register()
    {
        _register?.Invoke(_capability);
    }
}
