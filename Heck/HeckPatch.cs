namespace Heck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;

    internal struct HeckPatchData
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

    public class HeckData
    {
        private static readonly Dictionary<Harmony, HeckData> _heckPatches = new Dictionary<Harmony, HeckData>();

        private readonly List<HeckPatchData> _heckPatchDatas;

        private HeckData(List<HeckPatchData> heckPatchDatas)
        {
            _heckPatchDatas = heckPatchDatas;
        }

        public bool Enabled { get; internal set; }

        public static void TogglePatches(Harmony id, bool value)
        {
            if (_heckPatches.TryGetValue(id, out HeckData heck))
            {
                if (value != heck.Enabled)
                {
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
                                Plugin.Logger.Log($"[{id.Id}] Exception while patching [{n.OriginalMethod}] of [{n.OriginalMethod.DeclaringType}].", IPA.Logging.Logger.Level.Critical);
                                throw;
                            }
                        });
                    }
                    else
                    {
                        id.UnpatchAll(id.Id);
                    }
                }
            }
            else
            {
                Plugin.Logger.Log($"Could not find Heck patch data for {id}.", IPA.Logging.Logger.Level.Critical);
            }
        }

        public static void InitPatches(Harmony id, Assembly assembly)
        {
            InitPatches(id, assembly, 0);
        }

        public static void InitPatches(Harmony harmony, Assembly assembly, int id)
        {
            if (!_heckPatches.ContainsKey(harmony))
            {
                Plugin.Logger.Log($"Initializing patches for Harmony instance [{harmony.Id}] in [{assembly.GetName()}].", IPA.Logging.Logger.Level.Trace);

                List<HeckPatchData> heckPatchDatas = new List<HeckPatchData>();
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
                        List<string> methodNames = new List<string>();
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

                        MethodInfo? prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo? postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo? transpiler = AccessTools.Method(type, "Transpiler");

                        // Logging
                        string methodsContained = string.Join(", ", new MethodInfo[] { prefix, postfix, transpiler }.Where(n => n != null).Select(n => n.Name));

                        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                        switch (methodType)
                        {
                            case MethodType.Normal:
                                foreach (string methodName in methodNames)
                                {
                                    MethodBase normalMethodBase;

                                    if (parameters != null)
                                    {
                                        normalMethodBase = declaringType.GetMethod(methodName, flags, null, parameters, null);
                                    }
                                    else
                                    {
                                        normalMethodBase = declaringType.GetMethod(methodName, flags);
                                    }

                                    if (normalMethodBase == null)
                                    {
                                        throw new ArgumentException($"Could not find method '{methodName}' of '{declaringType}'.");
                                    }

                                    Plugin.Logger.Log($"[{harmony.Id}] Found patch for method [{declaringType.FullName}.{normalMethodBase.Name}] containing [{methodsContained}]", IPA.Logging.Logger.Level.Trace);
                                    heckPatchDatas.Add(new HeckPatchData(normalMethodBase, prefix, postfix, transpiler));
                                }

                                break;

                            case MethodType.Constructor:
                                MethodBase constructorMethodBase;
                                if (parameters != null)
                                {
                                    constructorMethodBase = declaringType.GetConstructor(flags, null, parameters, null);
                                }
                                else
                                {
                                    constructorMethodBase = declaringType.GetConstructor(flags, null, Type.EmptyTypes, null);
                                }

                                if (constructorMethodBase == null)
                                {
                                    throw new ArgumentException($"Could not find constructor for '{declaringType}'.");
                                }

                                Plugin.Logger.Log($"[{harmony.Id}] Found patch for constructor [{declaringType.FullName}.{constructorMethodBase.Name}] containing [{methodsContained}]", IPA.Logging.Logger.Level.Trace);
                                heckPatchDatas.Add(new HeckPatchData(constructorMethodBase, prefix, postfix, transpiler));

                                break;

                            default:
                                continue;
                        }
                    }
                }

                _heckPatches.Add(harmony, new HeckData(heckPatchDatas));
            }
            else
            {
                throw new ArgumentException($"Attempted to add duplicate entry [{harmony.Id}].", nameof(harmony));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HeckPatch : Attribute
    {
        public HeckPatch(Type declaringType)
        {
            DeclaringType = declaringType;
        }

        public HeckPatch(string methodName)
        {
            MethodName = methodName;
        }

        public HeckPatch(Type[] parameters)
        {
            Parameters = parameters;
        }

        public HeckPatch(MethodType methodType)
        {
            MethodType = methodType;
        }

        public HeckPatch(int id)
        {
            Id = id;
        }

        public HeckPatch(Type declaringType, string methodName)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
        }

        internal Type? DeclaringType { get; }

        internal string? MethodName { get; }

        internal Type[]? Parameters { get; }

        internal MethodType? MethodType { get; }

        internal int? Id { get; }
    }
}
