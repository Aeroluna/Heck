﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck.Deserialize;
using Heck.Patcher;
using JetBrains.Annotations;
using SiraUtil.Logging;
using Zenject;

namespace Heck.Module;

public class ModuleManager : IDisposable
{
    private readonly SiraLog _log;
    private List<ModuleData> _modules = [];

    private bool _sorted;

    [UsedImplicitly]
    private ModuleManager(
        SiraLog log,
        [Inject(Optional = true, Source = InjectSources.Local)]
        IEnumerable<IModule> modules,
        DeserializerManager deserializerManager)
    {
        _log = log;
        foreach (IModule module in modules)
        {
            Type type = module.GetType();
            ModuleAttribute? attribute = type.GetCustomAttribute<ModuleAttribute>();
            if (attribute == null)
            {
                log.Warn($"[{type.FullName}] is missing Module attribute and will be ignored.");
                continue;
            }

            List<IModuleFeature> features = [];

            MethodInfo? callback = GetMethodWithAttribute<ModuleCallbackAttribute>(type);
            if (callback != null)
            {
                features.Add(new CallbackModuleFeature(callback));
            }

            MethodInfo? condition = GetMethodWithAttribute<ModuleConditionAttribute>(type);
            if (condition != null)
            {
                if (condition.ReturnType != typeof(bool))
                {
                    log.Warn(
                        $"[{type.FullName}] does not contain a method marked with [{nameof(ModuleConditionAttribute)}] that returns [{nameof(Boolean)}].");
                }
                else
                {
                    features.Add(new ConditionModuleFeature(condition));
                }
            }

            ModuleDataDeserializerAttribute? dataDeserializer =
                type.GetCustomAttribute<ModuleDataDeserializerAttribute>();
            if (dataDeserializer != null)
            {
                features.Add(
                    new DeserializerModuleFeature(
                        deserializerManager.Register(dataDeserializer.Id, dataDeserializer.Type)));
            }

            ModulePatcherAttribute? modulePatcher = type.GetCustomAttribute<ModulePatcherAttribute>();
            if (modulePatcher != null)
            {
                features.Add(
                    new PatcherModuleFeature(
                        new HeckPatcher(type.Assembly, modulePatcher.HarmonyId, modulePatcher.Id)));
            }

            ModuleData moduleData = new(
                module,
                attribute.Id,
                attribute.Priority,
                attribute.LoadType,
                attribute.Depends ?? [],
                attribute.Conflict ?? [],
                features.ToArray());

            _modules.Add(moduleData);
            _sorted = false;
        }

        return;

        static MethodInfo? GetMethodWithAttribute<TAttribute>(Type type)
            where TAttribute : Attribute
        {
            return type
                .GetMethods(AccessTools.allDeclared)
                .FirstOrDefault(n => n.GetCustomAttribute<TAttribute>() != null);
        }
    }

    public void Dispose()
    {
        foreach (IModuleFeature moduleFeature in _modules.SelectMany(n => n.Features))
        {
            if (moduleFeature is PatcherModuleFeature patcherFeature)
            {
                patcherFeature.Patcher.Dispose();
            }
        }
    }

    internal void Activate(
#if !PRE_V1_37_1
        in BeatmapKey beatmapKey,
        BeatmapLevel? beatmapLevel,
#else
        IDifficultyBeatmap? difficultyBeatmap,
#endif
        LevelType levelType,
        ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
    {
        bool disableAll = false;
        string[]? requirements = null;
        string[]? suggestions = null;
#if !PRE_V1_37_1
        CustomData? beatmapCustomData = beatmapLevel?.GetBeatmapCustomData(beatmapKey);
        if (beatmapCustomData != null)
        {
#else
        if (difficultyBeatmap != null)
        {
            CustomData beatmapCustomData = difficultyBeatmap.GetBeatmapCustomData();
#endif
            requirements = beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().ToArray() ??
                           [];
            suggestions = beatmapCustomData.Get<List<object>>("_suggestions")?.Cast<string>().ToArray() ??
                          [];
        }
        else
        {
            disableAll = true;
        }

        requirements ??= [];
        suggestions ??= [];

        ModuleArgs moduleArgs = new(overrideEnvironmentSettings);

        object[] inputs =
        {
            new Capabilities(requirements, suggestions),
#if !PRE_V1_37_1
            beatmapKey,
            beatmapLevel ??
            new BeatmapLevel(
                0,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                0,
                0,
                0,
                0,
                0,
                0,
                PlayerSensitivityFlag.Unknown,
                null,
                null),
#else
            difficultyBeatmap ?? new EmptyDifficultyBeatmap(),
#endif
            moduleArgs,
            levelType
        };

        if (!_sorted)
        {
            _modules = _modules.OrderByDescending(n => n.Priority).ToList();
            _log.Trace($"Modules registered: {string.Join(", ", _modules.Select(n => $"[{n}]"))}");
            _sorted = true;
        }

        List<ModuleData> queue = _modules.ToList();
        HashSet<ModuleData> active = [];

        while (queue.Count != 0)
        {
            InitializeModule(queue.First());
        }

        overrideEnvironmentSettings = moduleArgs.OverrideEnvironmentSettings;
        return;

        void InitializeModule(ModuleData module, bool depended = false)
        {
            // Remove module from processing queue
            queue.Remove(module);

            // force fail on all modules
            if (disableAll)
            {
                Finish(false);
                return;
            }

            // check requirement
            switch (module.LoadType)
            {
                case LoadType.Passive:
                    if (!depended)
                    {
                        _log.Trace($"[{module.Id}] not requested by any other module, skipping");
                        Finish(false);
                        return;
                    }

                    break;
                case LoadType.Active:
                    ConditionModuleFeature? conditionFeature = module.GetFeature<ConditionModuleFeature>();
                    if (conditionFeature != null)
                    {
                        MethodInfo condition = conditionFeature.Method;
                        if (!(bool)condition.Invoke(
                                module.Module,
                                condition.ActualParameters(inputs.AddToArray(depended))))
                        {
                            _log.Trace($"[{module.Id}] did not pass condition, skipping");
                            Finish(false);
                            return;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(module), $"Was not valid {nameof(LevelType)}.");
            }

            // Handle conflicts
            foreach (string conflictId in module.Conflict)
            {
                ModuleData? conflict = active.FirstOrDefault(n => n.Id == conflictId);
                if (conflict == null)
                {
                    continue;
                }

                _log.Trace($"[{module.Id}] conflicts with [{conflictId}], skipping");
                Finish(false);
                return;
            }

            // handle dependencies
            foreach (string dependId in module.Depends)
            {
                ModuleData? depend = queue.FirstOrDefault(n => n.Id == dependId);
                if (depend != null)
                {
                    InitializeModule(depend, true);
                }

                depend ??= active.FirstOrDefault(n => n.Id == dependId);
                if (active.Contains(depend))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"[{module.Id}] requires [{dependId}] but it is not available.");
            }

            // passed the checks, initilaize
            active.Add(module);
            try
            {
                Finish(true);
                _log.Trace($"[{module.Id}] loaded");
            }
            catch
            {
                _log.Critical($"Exception while loading [{module.Id}]");
                throw;
            }

            return;

            void Finish(bool success)
            {
                MethodInfo? callBack =
                    module.GetFeature<CallbackModuleFeature>()?.Method;
                callBack?.Invoke(module.Module, callBack.ActualParameters(inputs.AddToArray(success)));

                DataDeserializer? dataDeserializer =
                    module.GetFeature<DeserializerModuleFeature>()?.DataDeserializer;
                if (dataDeserializer != null)
                {
                    dataDeserializer.Enabled = success;
                }

                HeckPatcher? patcher =
                    module.GetFeature<PatcherModuleFeature>()?.Patcher;
                if (patcher != null)
                {
                    patcher.Enabled = success;
                }
            }
        }
    }

    public class ModuleArgs
    {
        public ModuleArgs(OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            OverrideEnvironmentSettings = overrideEnvironmentSettings;
        }

        public OverrideEnvironmentSettings? OverrideEnvironmentSettings { get; set; }
    }
}
