using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chroma.Settings;
using IPA.Logging;
using IPA.Utilities;
using Newtonsoft.Json;

namespace Chroma.Lighting
{
    internal static class LightIDTableManager
    {
        private static readonly Dictionary<string, Dictionary<int, Dictionary<int, int>>> _lightIDTable = new();

        private static Dictionary<int, Dictionary<int, int>>? _activeTable;

        internal static int? GetActiveTableValue(int type, int id)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(type, out Dictionary<int, int> dictioanry) && dictioanry.TryGetValue(id, out int newId))
                {
                    return newId;
                }

                Log.Logger.Log($"Unable to find value for type [{type}] and id [{id}].", Logger.Level.Error);
                return null;
            }

            Log.Logger.Log("No active table loaded.", Logger.Level.Error);

            return null;
        }

        internal static int? GetActiveTableValueReverse(int type, int id)
        {
            if (_activeTable != null)
            {
                if (!_activeTable.TryGetValue(type, out Dictionary<int, int> dictioanry))
                {
                    return null;
                }

                foreach ((int key, int value) in dictioanry)
                {
                    if (value == id)
                    {
                        return key;
                    }
                }

                ////Plugin.Logger.Log($"Unable to find value for type [{type}] and id [{id}].", IPA.Logging.Logger.Level.Error);
                return null;
            }

            Log.Logger.Log("No active table loaded.", Logger.Level.Error);

            return null;
        }

        internal static void SetEnvironment(string environmentName)
        {
            if (_lightIDTable.TryGetValue(environmentName, out Dictionary<int, Dictionary<int, int>> activeTable))
            {
                _activeTable = activeTable.ToDictionary(n => n.Key, n => n.Value.ToDictionary(m => m.Key, m => m.Value));
            }
            else
            {
                _activeTable = null;
                Log.Logger.Log($"Table not found for: {environmentName}", Logger.Level.Warning);
            }
        }

        internal static void RegisterIndex(int type, int index, int? requestedKey)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(type, out Dictionary<int, int> dictioanry))
                {
                    int key;

                    if (requestedKey.HasValue)
                    {
                        key = requestedKey.Value;
                        while (dictioanry.ContainsKey(key))
                        {
                            key++;
                        }
                    }
                    else
                    {
                        key = dictioanry.Keys.Max() + 1;
                    }

                    dictioanry.Add(key, index);
                    if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Log.Logger.Log($"Registered key [{key}] to type [{type}].");
                    }
                }
                else
                {
                    Log.Logger.Log($"Table does not contain type [{type}].", Logger.Level.Warning);
                }
            }
            else
            {
                Log.Logger.Log("No active table, could not register index.", Logger.Level.Warning);
            }
        }

        internal static void InitTable()
        {
            const string tableNamespace = "Chroma.LightIDTables.";
            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<string> tableNames = assembly.GetManifestResourceNames().Where(n => n.StartsWith(tableNamespace));
            foreach (string tableName in tableNames)
            {
                using JsonReader reader = new JsonTextReader(new StreamReader(
                    assembly.GetManifestResourceStream(tableName)
                    ?? throw new InvalidOperationException($"Failed to retrieve {tableName}.")));
                Dictionary<int, Dictionary<int, int>> typeTable = new();

                JsonSerializer serializer = new();
                Dictionary<string, Dictionary<string, int>> rawDict =
                    serializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader)
                    ?? throw new InvalidOperationException($"Failed to deserialize ID table [{tableName}].");

                foreach ((string key, Dictionary<string, int> value) in rawDict)
                {
                    typeTable[int.Parse(key)] = value.ToDictionary(n => int.Parse(n.Key), n => n.Value);
                }

                string tableNameWithoutExtension = Path.GetFileNameWithoutExtension(
                    tableName.Remove(
                        tableName.IndexOf(tableNamespace, StringComparison.Ordinal),
                        tableNamespace.Length));
                _lightIDTable.Add(tableNameWithoutExtension, typeTable);
            }
        }
    }
}
