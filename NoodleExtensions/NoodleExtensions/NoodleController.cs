namespace NoodleExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using static NoodleExtensions.Plugin;

    public static class NoodleController
    {
        private static List<NoodlePatchData> _noodlePatches;

        // used by tracks
        public static bool LeftHandedMode { get; internal set; }

        public static bool NoodleExtensionsActive { get; private set; }

        public static void ToggleNoodlePatches(bool value)
        {
            NoodleExtensionsActive = value;
            if (value)
            {
                if (!Harmony.HasAnyPatches(HARMONYID))
                {
                    _noodlePatches.ForEach(n => _harmonyInstance.Patch(
                        n.OriginalMethod,
                        n.Prefix != null ? new HarmonyMethod(n.Prefix) : null,
                        n.Postfix != null ? new HarmonyMethod(n.Postfix) : null,
                        n.Transpiler != null ? new HarmonyMethod(n.Transpiler) : null));

                    CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit += Animation.AnimationController.CustomEventCallbackInit;
                }
            }
            else
            {
                _harmonyInstance.UnpatchAll(HARMONYID);

                CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit -= Animation.AnimationController.CustomEventCallbackInit;
            }
        }

        internal static void InitNoodlePatches()
        {
            if (_noodlePatches == null)
            {
                _noodlePatches = new List<NoodlePatchData>();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    // The nuclear option, should we ever need it
                    /*MethodInfo manualPatch = AccessTools.Method(type, "ManualPatch");
                    if (manualPatch != null)
                    {
                        _noodlePatches.Add((NoodlePatchData)manualPatch.Invoke(null, null));
                        continue;
                    }*/

                    object[] noodleattributes = type.GetCustomAttributes(typeof(NoodlePatch), true);
                    if (noodleattributes.Length > 0)
                    {
                        Type declaringType = null;
                        List<string> methodNames = new List<string>();
                        Type[] parameters = null;
                        foreach (NoodlePatch n in noodleattributes)
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
                        }

                        if (declaringType == null || !methodNames.Any())
                        {
                            throw new ArgumentException("Type or Method Name not described");
                        }

                        MethodInfo prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo transpiler = AccessTools.Method(type, "Transpiler");

                        foreach (string methodName in methodNames)
                        {
                            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                            MethodInfo methodInfo;
                            if (parameters != null)
                            {
                                methodInfo = declaringType.GetMethod(methodName, flags, null, parameters, null);
                            }
                            else
                            {
                                methodInfo = declaringType.GetMethod(methodName, flags);
                            }

                            if (methodInfo == null)
                            {
                                throw new ArgumentException($"Could not find method '{methodName}' of '{declaringType}'");
                            }

                            _noodlePatches.Add(new NoodlePatchData(methodInfo, prefix, postfix, transpiler));
                        }
                    }
                }
            }
        }
    }
}
