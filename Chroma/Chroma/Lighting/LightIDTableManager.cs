namespace Chroma
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    internal static class LightIDTableManager
    {
        private static readonly Dictionary<string, Dictionary<int, Dictionary<int, int>>> _lightIDTable = new Dictionary<string, Dictionary<int, Dictionary<int, int>>>();

        private static Dictionary<int, Dictionary<int, int>>? _activeTable;

        internal static int? GetActiveTableValue(int type, int id)
        {
            if (_activeTable != null)
            {
                if (_activeTable.TryGetValue(type, out Dictionary<int, int> dictioanry) && dictioanry.TryGetValue(id, out int newId))
                {
                    return newId;
                }
                else
                {
                    Plugin.Logger.Log($"Unable to find value for type [{type}] and id [{id}].", IPA.Logging.Logger.Level.Error);
                }
            }

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
                Plugin.Logger.Log($"Table not found for: {environmentName}", IPA.Logging.Logger.Level.Warning);
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
                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Plugin.Logger.Log($"Registered key [{key}] to type [{type}].");
                    }
                }
                else
                {
                    Plugin.Logger.Log($"Table does not contain type [{type}].", IPA.Logging.Logger.Level.Warning);
                }
            }
            else
            {
                Plugin.Logger.Log($"No active table, could not register index.", IPA.Logging.Logger.Level.Warning);
            }
        }

        internal static void InitTable()
        {
            string tableNamespace = "Chroma.LightIDTables.";
            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<string> tableNames = assembly.GetManifestResourceNames().Where(n => n.StartsWith(tableNamespace));
            foreach (string tableName in tableNames)
            {
                using JsonReader reader = new JsonTextReader(new StreamReader(assembly.GetManifestResourceStream(tableName)));
                Dictionary<int, Dictionary<int, int>> typeTable = new Dictionary<int, Dictionary<int, int>>();

                JsonSerializer serializer = new JsonSerializer();
                Dictionary<string, Dictionary<string, int>> rawDict = serializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader) ?? throw new System.InvalidOperationException($"Failed to deserialize ID table [{tableName}].");

                foreach (KeyValuePair<string, Dictionary<string, int>> typePair in rawDict)
                {
                    typeTable[int.Parse(typePair.Key)] = typePair.Value.ToDictionary(n => int.Parse(n.Key), n => n.Value);
                }

                string tableNameWithoutExtension = Path.GetFileNameWithoutExtension(tableName.Remove(tableName.IndexOf(tableNamespace), tableNamespace.Length));
                _lightIDTable.Add(tableNameWithoutExtension, typeTable);
            }
        }
    }
}
