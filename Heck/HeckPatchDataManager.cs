using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Logging;

namespace Heck
{
    public class HeckPatchDataManager
    {
        private static readonly Dictionary<Harmony, HeckPatchDataManager> _heckPatches = new();

        private readonly List<HeckPatchData> _heckPatchDatas;

        private HeckPatchDataManager(List<HeckPatchData> heckPatchDatas)
        {
            _heckPatchDatas = heckPatchDatas;
        }

        public bool Enabled { get; internal set; }

        public static void TogglePatches(Harmony id, bool value)
        {
            if (_heckPatches.TryGetValue(id, out HeckPatchDataManager heck))
            {
                if (value == heck.Enabled)
                {
                    return;
                }

                heck.Enabled = value;
                if (value)
                {
                    heck._heckPatchDatas.ForEach(n =>
                    {
                        try
                        {
                            id.Patch(
                                n.OriginalMethod,
                                n.Prefix != null ? new HarmonyMethod(n.Prefix) : null,
                                n.Postfix != null ? new HarmonyMethod(n.Postfix) : null,
                                n.Transpiler != null ? new HarmonyMethod(n.Transpiler) : null);
                        }
                        catch
                        {
                            Log.Logger.Log($"[{id.Id}] Exception while patching [{n.OriginalMethod}] of [{n.OriginalMethod.DeclaringType}].", Logger.Level.Critical);
                            throw;
                        }
                    });
                }
                else
                {
                    id.UnpatchSelf();
                }
            }
            else
            {
                Log.Logger.Log($"Could not find Heck patch data for {id}.", Logger.Level.Critical);
            }
        }

        public static void InitPatches(Harmony harmony, Assembly assembly, int id = 0)
        {
            if (!_heckPatches.ContainsKey(harmony))
            {
                Log.Logger.Log($"Initializing patches for Harmony instance [{harmony.Id}] in [{assembly.GetName()}].", Logger.Level.Trace);

                List<HeckPatchData> heckPatchDatas = new();
                foreach (Type type in assembly.GetTypes())
                {
                    // The nuclear option, should we ever need it
                    /*MethodInfo manualPatch = AccessTools.Method(type, "ManualPatch");
                    if (manualPatch != null)
                    {
                        _noodlePatches.Add((NoodlePatchData)manualPatch.Invoke(null, null));
                        continue;
                    }*/

                    object[] attributes = type.GetCustomAttributes(typeof(HeckPatch), true);
                    if (attributes.Length > 0)
                    {
                        Type? declaringType = null;
                        List<string> methodNames = new();
                        Type[]? parameters = null;
                        MethodType methodType = MethodType.Normal;
                        int patchId = 0;
                        foreach (HeckPatch n in attributes)
                        {
                            if (n.DeclaringType != null)
                            {
                                declaringType = n.DeclaringType;
                            }

                            if (n.MethodName != null)
                            {
                                methodNames.Add(n.MethodName);
                            }

                            if (n.Parameters != null)
                            {
                                parameters = n.Parameters;
                            }

                            if (n.MethodType != null)
                            {
                                methodType = n.MethodType.Value;
                            }

                            if (n.MethodType != null)
                            {
                                methodType = n.MethodType.Value;
                            }

                            if (n.Id != null)
                            {
                                patchId = n.Id.Value;
                            }
                        }

                        if (patchId != id)
                        {
                            continue;
                        }

                        if (declaringType == null)
                        {
                            throw new ArgumentException("Type not described");
                        }

                        // dont use accesstools because harmony spams logs if search fails (which is totally expected here)
                        MethodInfo? prefix = type.GetMethod("Prefix", AccessTools.all);
                        MethodInfo? postfix = type.GetMethod("Postfix", AccessTools.all);
                        MethodInfo? transpiler = type.GetMethod("Transpiler", AccessTools.all);

                        // Logging
                        string methodsContained = string.Join(", ", new[] { prefix, postfix, transpiler }.Where(n => n != null).Select(n => n?.Name));

                        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                        switch (methodType)
                        {
                            case MethodType.Normal:
                                foreach (string methodName in methodNames)
                                {
                                    MethodBase? normalMethodBase = parameters != null ? declaringType.GetMethod(methodName, flags, null, parameters, null)
                                        : declaringType.GetMethod(methodName, flags);

                                    if (normalMethodBase == null)
                                    {
                                        throw new ArgumentException($"Could not find method '{methodName}' of '{declaringType}'.");
                                    }

                                    Log.Logger.Log($"[{harmony.Id}] Found patch for method [{declaringType.FullName}.{normalMethodBase.Name}] containing [{methodsContained}]", Logger.Level.Trace);
                                    heckPatchDatas.Add(new HeckPatchData(normalMethodBase, prefix, postfix, transpiler));
                                }

                                break;

                            case MethodType.Constructor:
                                MethodBase? constructorMethodBase = declaringType.GetConstructor(flags, null, parameters ?? Type.EmptyTypes, null);

                                if (constructorMethodBase == null)
                                {
                                    throw new ArgumentException($"Could not find constructor for '{declaringType}'.");
                                }

                                Log.Logger.Log($"[{harmony.Id}] Found patch for constructor [{declaringType.FullName}.{constructorMethodBase.Name}] containing [{methodsContained}]", Logger.Level.Trace);
                                heckPatchDatas.Add(new HeckPatchData(constructorMethodBase, prefix, postfix, transpiler));

                                break;

                            default:
                                continue;
                        }
                    }
                }

                _heckPatches.Add(harmony, new HeckPatchDataManager(heckPatchDatas));
            }
            else
            {
                throw new ArgumentException($"Attempted to add duplicate entry [{harmony.Id}].", nameof(harmony));
            }
        }

        internal readonly struct HeckPatchData
        {
            internal HeckPatchData(MethodBase orig, MethodInfo? pre, MethodInfo? post, MethodInfo? tran)
            {
                OriginalMethod = orig;
                Prefix = pre;
                Postfix = post;
                Transpiler = tran;
            }

            internal MethodBase OriginalMethod { get; }

            internal MethodInfo? Prefix { get; }

            internal MethodInfo? Postfix { get; }

            internal MethodInfo? Transpiler { get; }
        }
    }
}
