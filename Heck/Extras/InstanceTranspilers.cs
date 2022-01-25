using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ModestTree;
using MonoMod.Utils;

namespace Heck
{
    public static class InstanceTranspilers
    {
        private static readonly FieldInfo _cacheField = AccessTools.Field(typeof(InstanceTranspilers), nameof(_delegateCache));
        private static readonly MethodInfo _getMethod = AccessTools.Method(typeof(Dictionary<int, Delegate>), "get_Item");

        // ReSharper disable once CollectionNeverQueried.Local => used by generated DMDs
        private static readonly Dictionary<int, Delegate> _delegateCache = new();
        private static readonly Dictionary<CodeInstruction, int> _indices = new();

        public static void DisposeDelegate(CodeInstruction instruction)
        {
            _delegateCache.RemoveWithConfirm(_indices.GetValueAndRemove(instruction));
        }

        // The version of this that is in HarmonyX is stinky
        public static CodeInstruction EmitInstanceDelegate<T>(T action)
            where T : Delegate
        {
            if (action.Method.IsStatic || action.Target == null)
            {
                throw new ArgumentException("Can only handle instance methods.", nameof(action));
            }

            Type[] paramTypes = action.Method.GetParameters().Select(x => x.ParameterType).ToArray();

            DynamicMethodDefinition dynamicMethod = new(
                action.Method.Name,
                action.Method.ReturnType,
                paramTypes);

            ILGenerator il = dynamicMethod.GetILGenerator();

            int currentDelegateCounter = 0;
            while (_delegateCache.ContainsKey(currentDelegateCounter))
            {
                currentDelegateCounter++;
            }

            _delegateCache.Add(currentDelegateCounter, action);

            il.Emit(OpCodes.Ldsfld, _cacheField);
            il.Emit(OpCodes.Ldc_I4, currentDelegateCounter);
            il.Emit(OpCodes.Callvirt, _getMethod);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_S, (short)i);
            }

            il.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(T), "Invoke"));
            il.Emit(OpCodes.Ret);

            CodeInstruction codeInstruction = new(OpCodes.Call, dynamicMethod.Generate());

            _indices.Add(codeInstruction, currentDelegateCounter);

            return codeInstruction;
        }
    }
}
