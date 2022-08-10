using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Logger = IPA.Logging.Logger;

namespace Chroma.EnvironmentEnhancement
{
    internal static class LookupID
    {
        private const string LOOKUPDLL = @"LookupID.dll";

        private static bool _useFallback;

        // this is where i pretend to know what any of this is doing.
        internal static List<GameObjectInfo> Get(List<GameObjectInfo> source, string[] gameObjectIds, string id, LookupMethod lookupMethod)
        {
            if (_useFallback)
            {
                return LookupID_Legacy(source, id, lookupMethod);
            }

            try
            {
                int length = gameObjectIds.Length;
                LookupID_internal(gameObjectIds, length, out IntPtr buffer, ref length, id, lookupMethod);

                int[] arrayRes = new int[length];
                Marshal.Copy(buffer, arrayRes, 0, length);
                Marshal.FreeCoTaskMem(buffer);

                List<GameObjectInfo> returnList = new(length);
                returnList.AddRange(arrayRes.Select(index => source[index]));
                return returnList;
            }
            catch (Exception e)
            {
                Log.Logger.Log("Error running LookupID, falling back to managed code.", Logger.Level.Error);
                Log.Logger.Log("Expect long load times...", Logger.Level.Error);
                Log.Logger.Log(e.ToString(), Logger.Level.Error);

                _useFallback = true;
                return LookupID_Legacy(source, id, lookupMethod);
            }
        }

        // whatever the fuck rider is recommending causes shit to crash so we disable it
#pragma warning disable CA2101
        [DllImport(LOOKUPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void LookupID_internal(
            [In, Out] string[] array,
            int size,
            out IntPtr returnArray,
            ref int returnSize,
            [MarshalAs(UnmanagedType.LPStr)] string id,
            LookupMethod method);
#pragma warning restore CA2101

        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        // fuck mono regex fuck mono regex fuck mono regex fuck mono regex fuck mono regex
        private static List<GameObjectInfo> LookupID_Legacy(IEnumerable<GameObjectInfo> source, string id, LookupMethod lookupMethod)
        {
            Func<GameObjectInfo, bool> predicate;
            switch (lookupMethod)
            {
                case LookupMethod.Regex:
                    Regex regex = new(id, RegexOptions.CultureInvariant | RegexOptions.ECMAScript | RegexOptions.Compiled);
                    predicate = n => regex.IsMatch(n.FullID);
                    break;

                case LookupMethod.Exact:
                    predicate = n => n.FullID == id;
                    break;

                case LookupMethod.Contains:
                    predicate = n => n.FullID.Contains(id);
                    break;

                case LookupMethod.StartsWith:
                    predicate = n => n.FullID.StartsWith(id);
                    break;

                case LookupMethod.EndsWith:
                    predicate = n => n.FullID.EndsWith(id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lookupMethod), "Invalid lookup method.");
            }

            return source.Where(predicate).ToList();
        }
    }
}
