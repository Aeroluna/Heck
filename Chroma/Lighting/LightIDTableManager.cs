using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chroma.Settings;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Zenject;

namespace Chroma.Lighting
{
    [UsedImplicitly]
    internal class LightIDTableManager : IInitializable
    {
        private static readonly Dictionary<string, Dictionary<int, Dictionary<int, int>>> _lightIDTable = new();

        private static readonly Dictionary<int, Dictionary<int, int>> _defaultTable = new()
        {
            { 0, new Dictionary<int, int>() },
            { 1, new Dictionary<int, int>() },
            { 2, new Dictionary<int, int>() },
            { 3, new Dictionary<int, int>() },
            { 4, new Dictionary<int, int>() },
            { 5, new Dictionary<int, int>() },
            { 6, new Dictionary<int, int>() },
            { 7, new Dictionary<int, int>() },
            { 8, new Dictionary<int, int>() },
            { 9, new Dictionary<int, int>() }
        };

        private static Dictionary<int, Dictionary<int, int>>? _loadedTable;

        private readonly HashSet<Tuple<int, int>> _failureLog = new();

        private Dictionary<int, Dictionary<int, int>>? _activeTable;

        public void Initialize()
        {
            _activeTable = _loadedTable?.ToDictionary(n => n.Key, n => n.Value.ToDictionary(m => m.Key, m => m.Value));
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

        internal static void SetEnvironment(string environmentName)
        {
            if (_lightIDTable.TryGetValue(environmentName, out Dictionary<int, Dictionary<int, int>> selectedTable))
            {
                ////_activeTable = activeTable.ToDictionary(n => n.Key, n => n.Value.ToDictionary(m => m.Key, m => m.Value));
                _loadedTable = selectedTable;
            }
            else
            {
                ////_activeTable = new Dictionary<int, Dictionary<int, int>>();
                ////Enumerable.Range(0, 10).Do(n => _activeTable[n] = new Dictionary<int, int>());
                _loadedTable = _defaultTable;
                Log.Logger.Log($"Table not found for: {environmentName}", Logger.Level.Warning);
            }
        }

        internal int? GetActiveTableValue(int lightID, int id)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(lightID, out Dictionary<int, int> dictioanry) && dictioanry.TryGetValue(id, out int newId))
                {
                    return newId;
                }

                Tuple<int, int> failure = new(lightID, id);
                if (_failureLog.Contains(failure))
                {
                    return null;
                }

                Log.Logger.Log($"Unable to find value for light ID [{lightID}] and id [{id}]. Omitting future messages...", Logger.Level.Error);
                _failureLog.Add(failure);

                return null;
            }

            Log.Logger.Log("No active table loaded.", Logger.Level.Error);

            return null;
        }

        internal int? GetActiveTableValueReverse(int lightID, int id)
        {
            if (_activeTable != null)
            {
                if (!_activeTable.TryGetValue(lightID, out Dictionary<int, int> dictioanry))
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

        internal void RegisterIndex(int lightID, int index, int? requestedKey)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(lightID, out Dictionary<int, int> dictioanry))
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
                        if (dictioanry.Any())
                        {
                            key = dictioanry.Keys.Max() + 1;
                        }
                        else
                        {
                            key = 0;
                        }
                    }

                    dictioanry.Add(key, index);
                    if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Log.Logger.Log($"Registered key [{key}] to light ID [{lightID}].");
                    }
                }
                else
                {
                    Log.Logger.Log($"Table does not contain light ID [{lightID}].", Logger.Level.Warning);
                }
            }
            else
            {
                Log.Logger.Log("No active table, could not register index.", Logger.Level.Warning);
            }
        }

        internal void UnregisterIndex(int lightID, int index)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(lightID, out Dictionary<int, int> dictioanry))
                {
                    foreach ((int key, int value) in dictioanry)
                    {
                        if (value != index)
                        {
                            continue;
                        }

                        dictioanry.Remove(key);
                        if (ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            Log.Logger.Log($"Unregistered key [{key}] from light ID [{lightID}].");
                        }

                        return;
                    }

                    Log.Logger.Log("Could not find key to unregister.", Logger.Level.Warning);
                }
                else
                {
                    Log.Logger.Log($"Table does not contain light ID [{lightID}].", Logger.Level.Warning);
                }
            }
            else
            {
                Log.Logger.Log("No active table, could not unregister index.", Logger.Level.Warning);
            }
        }
    }
}
