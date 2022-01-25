using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Heck
{
    internal static class ActualParametersExtension
    {
        // AccessTools.ActualParameters except this one throws exceptions.
        internal static object[] ActualParameters(this MethodBase method, object[] inputs)
        {
            List<Type?> inputTypes = inputs.Select(obj => obj?.GetType()).ToList();
            return method.GetParameters().Select(p =>
            {
                int index = inputTypes.FindIndex(p.ParameterType.IsAssignableFrom);
                if (index >= 0)
                {
                    return inputs[index];
                }

                throw new InvalidOperationException($"[{method.FullDescription()}] requested [{p.ParameterType.FullName}] but was not available.");
            }).ToArray();
        }
    }
}
