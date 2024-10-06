using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Chroma.EnvironmentEnhancement;
using Chroma.Settings;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;

namespace Chroma.Lighting;

internal class LightIDTableManager
{
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

    private static readonly Dictionary<string, Dictionary<int, Dictionary<int, int>>> _lightIDTable = new();

    private readonly HashSet<(int, int)> _failureLog = [];

    private readonly SiraLog _log;
    private readonly Config _config;
    private readonly EnvironmentOverrideChecker? _environmentOverrideChecker;
    private readonly Dictionary<int, Dictionary<int, int>> _activeTable;

    private LightIDTableManager(
        SiraLog log,
        Config config,
        EnvironmentSceneSetupData environmentSceneSetupData,
        [InjectOptional] EnvironmentOverrideChecker? environmentOverrideChecker)
    {
        _log = log;
        _config = config;
        _environmentOverrideChecker = environmentOverrideChecker;
        string environmentName = environmentSceneSetupData.environmentInfo.serializedName;
        Dictionary<int, Dictionary<int, int>> loadedTable;
        if (_lightIDTable.TryGetValue(environmentName, out Dictionary<int, Dictionary<int, int>> selectedTable))
        {
            loadedTable = selectedTable;
        }
        else
        {
            loadedTable = _defaultTable;
            Plugin.Log.Warn($"Table not found for [{environmentName}]");
        }

        _activeTable = loadedTable.ToDictionary(n => n.Key, n => n.Value.ToDictionary(m => m.Key, m => m.Value));
    }

    internal static void InitTable()
    {
        const string tableNamespace = "Chroma.LightIDTables.";
        Assembly assembly = typeof(LightIDTableManager).Assembly;
        IEnumerable<string> tableNames = assembly.GetManifestResourceNames().Where(n => n.StartsWith(tableNamespace));
        foreach (string tableName in tableNames)
        {
            using JsonReader reader = new JsonTextReader(
                new StreamReader(
                    assembly.GetManifestResourceStream(tableName) ??
                    throw new InvalidOperationException($"Failed to retrieve {tableName}")));
            Dictionary<int, Dictionary<int, int>> typeTable = new();

            JsonSerializer serializer = new();
            Dictionary<string, Dictionary<string, int>> rawDict =
                serializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(reader) ??
                throw new InvalidOperationException($"Failed to deserialize ID table [{tableName}]");

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

    internal int? GetActiveTableValue(int lightID, int id)
    {
        if (_activeTable.TryGetValue(lightID, out Dictionary<int, int> dictioanry) &&
            dictioanry.TryGetValue(id, out int newId))
        {
            return newId;
        }

        // suppress error logs when no environment override is loaded
        if (_environmentOverrideChecker == null ||
            _environmentOverrideChecker.LoadedEnvironment == LoadedEnvironmentType.None)
        {
            return null;
        }

        (int, int) failure = new(lightID, id);
        if (_failureLog.Contains(failure))
        {
            return null;
        }

        _log.Error($"Unable to find value for light ID [{lightID}] and id [{id}], omitting future errors of same id...");
        _failureLog.Add(failure);

        return null;
    }

    internal int? GetActiveTableValueReverse(int lightID, int id)
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

    internal void RegisterIndex(int lightID, int index, int? requestedKey)
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
                if (dictioanry.Count != 0)
                {
                    key = dictioanry.Keys.Max() + 1;
                }
                else
                {
                    key = 0;
                }
            }

            dictioanry.Add(key, index);
            if (_config.PrintEnvironmentEnhancementDebug)
            {
                _log.Debug($"Registered key [{key}] to light ID [{lightID}]");
            }
        }
        else
        {
            _log.Warn($"Table does not contain light ID [{lightID}]");
        }
    }

    internal void UnregisterIndex(int lightID, int index)
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
                if (_config.PrintEnvironmentEnhancementDebug)
                {
                    _log.Debug($"Unregistered key [{key}] from light ID [{lightID}]");
                }

                return;
            }

            _log.Warn("Could not find key to unregister");
        }
        else
        {
            _log.Warn($"Table does not contain light ID [{lightID}]");
        }
    }
}
