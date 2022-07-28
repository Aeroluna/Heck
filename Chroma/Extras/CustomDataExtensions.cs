using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;

namespace Heck
{
    // TODO: Move to CustomJsonData
    public static class CustomDataExtensions
    {
        public static IEnumerable<T>? GetFromArrayOfFloatArray<T>(this CustomData customData, string key, Func<float[], T> convert)
        {
            return customData.Get<IEnumerable<IEnumerable<object>>>(key)?.Select(o => convert(o.Select(Convert.ToSingle).ToArray()));
        }

        public static IEnumerable<T> GetRequiredFromArrayOfFloatArray<T>(this CustomData customData, string key, Func<float[], T> convert)
        {
            return customData.GetFromArrayOfFloatArray(key, convert) ?? throw new JsonNotDefinedException(key);
        }
    }
}
