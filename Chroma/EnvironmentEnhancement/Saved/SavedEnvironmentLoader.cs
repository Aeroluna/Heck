using System;
using System.Collections.Generic;
using System.IO;
using Chroma.Settings;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SiraUtil.Logging;

namespace Chroma.EnvironmentEnhancement.Saved;

internal class SavedEnvironmentLoader
{
    private static readonly Version _currVer = new(1, 0, 0);

    private static readonly string _directory = Path.Combine(
        UnityGame.UserDataPath,
        ChromaController.ID,
        "Environments");

    private readonly Config _config;

    private readonly SiraLog _log;

    [UsedImplicitly]
    private SavedEnvironmentLoader(SiraLog log, Config config)
    {
        _log = log;
        _config = config;
        Init();
    }

    public SavedEnvironment? SavedEnvironment
    {
        get
        {
            string? name = _config.CustomEnvironment;
            if (name == null)
            {
                return null;
            }

            Environments.TryGetValue(name, out SavedEnvironment? result);
            return result;
        }
    }

    public Dictionary<string?, SavedEnvironment?> Environments { get; private set; } = new();

    internal void Init()
    {
        Environments = new Dictionary<string?, SavedEnvironment?>();

        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        JsonSerializerSettings serializerSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error
        };

        foreach (string file in Directory.EnumerateFiles(_directory, "*.dat"))
        {
            try
            {
                using StreamReader streamReader = new(file);
                using JsonReader reader = new JsonTextReader(streamReader);
                JsonSerializer serializer = JsonSerializer.Create(serializerSettings);
                SavedEnvironment savedEnvironment = serializer.Deserialize<SavedEnvironment>(reader) ??
                                                    throw new InvalidOperationException("Deserializing returned null.");
                if (savedEnvironment.Version != _currVer)
                {
                    throw new InvalidOperationException(
                        $"Unhandled version: [{savedEnvironment.Version}], must be [{_currVer}].");
                }

                string fileName = Path.GetFileName(file);
                _log.Trace($"Loaded [{file}]");

                Environments.Add(fileName, savedEnvironment);
            }
            catch (Exception e)
            {
                _log.Error($"Encountered error deserializing [{file}]");
                _log.Error(e);
            }
        }
    }
}
