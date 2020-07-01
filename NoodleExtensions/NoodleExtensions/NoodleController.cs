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
        public static bool LeftHandedMode { get; set; }

        public static void ToggleNoodlePatches(bool value)
        {
            if (value)
            {
                if (!Harmony.HasAnyPatches(HARMONYID))
                {
                    _noodlePatches.ForEach(n => _harmonyInstance.Patch(
                        n.OriginalMethod,
                        n.Prefix != null ? new HarmonyMethod(n.Prefix) : null,
                        n.Postfix != null ? new HarmonyMethod(n.Postfix) : null,
                        n.Transpiler != null ? new HarmonyMethod(n.Transpiler) : null));
                }
            }
            else
            {
                _harmonyInstance.UnpatchAll(HARMONYID);
            }
        }

        internal static void InitNoodlePatches()
        {
            if (_noodlePatches == null)
            {
                _noodlePatches = new List<NoodlePatchData>();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    object[] noodleattributes = type.GetCustomAttributes(typeof(NoodlePatch), true);
                    if (noodleattributes.Length > 0)
                    {
                        Type declaringType = null;
                        List<string> methodNames = new List<string>();
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
                        }

                        if (declaringType == null || !methodNames.Any())
                        {
                            throw new ArgumentException("Type or Method Name not described");
                        }

                        MethodInfo prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo transpiler = AccessTools.Method(type, "Transpiler");

                        methodNames.ForEach(n => _noodlePatches.Add(new NoodlePatchData(AccessTools.Method(declaringType, n), prefix, postfix, transpiler)));
                    }
                }
            }
        }
    }
}
