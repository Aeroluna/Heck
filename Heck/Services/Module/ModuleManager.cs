using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Logging;

namespace Heck
{
    public static class ModuleManager
    {
        private static List<Module> _modules = new();

        private static bool _sorted;

        public static Module Register<T>(
            string id,
            int priority,
            RequirementType requirementType,
            object? attributeId = null,
            string[]? depends = null,
            string[]? conflict = null)
        {
            bool SearchForAttribute<TAttribute>(MethodInfo method)
                where TAttribute : AttributeWithId
            {
                AttributeWithId? attribute = method.GetCustomAttribute<TAttribute>();

                if (attribute == null)
                {
                    return false;
                }

                if (attribute.Id != null)
                {
                    return attribute.Id.Equals(attributeId);
                }

                return attributeId == null;
            }

            MethodInfo? method = typeof(T).GetMethods(AccessTools.allDeclared).FirstOrDefault(SearchForAttribute<ModuleCallback>);
            if (method == null)
            {
                throw new ArgumentException($"[{typeof(T).FullName}] does not contain a method marked with [{nameof(ModuleCallback)}] and id [{attributeId}].", nameof(T));
            }

            MethodInfo? condition = null;
            if (requirementType == RequirementType.Condition)
            {
                condition = typeof(T).GetMethods(AccessTools.allDeclared).FirstOrDefault(SearchForAttribute<ModuleCondition>);
                if (condition == null || condition.ReturnType != typeof(bool))
                {
                    throw new ArgumentException($"[{typeof(T).FullName}] does not contain a method marked with [{nameof(ModuleCondition)}] and id [{attributeId}] that returns [{nameof(Boolean)}].", nameof(T));
                }
            }

            Module module = new(
                id,
                priority,
                method,
                requirementType,
                condition,
                depends ?? Array.Empty<string>(),
                conflict ?? Array.Empty<string>());

            _modules.Add(module);
            _sorted = false;

            return module;
        }

        internal static void Activate(
            IDifficultyBeatmap? difficultyBeatmap,
            IPreviewBeatmapLevel? previewBeatmapLevel,
            LevelType levelType,
            ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            bool disableAll = false;
            string[]? requirements = null;
            string[]? suggestions = null;
            if (difficultyBeatmap != null)
            {
                CustomData beatmapCustomData = difficultyBeatmap.GetBeatmapCustomData();
                requirements = beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().ToArray() ?? Array.Empty<string>();
                suggestions = beatmapCustomData.Get<List<object>>("_suggestions")?.Cast<string>().ToArray() ?? Array.Empty<string>();
            }
            else
            {
                disableAll = true;
            }

            requirements ??= Array.Empty<string>();
            suggestions ??= Array.Empty<string>();

            ModuleArgs moduleArgs = new(overrideEnvironmentSettings);

            object[] inputs =
            {
                new Capabilities(requirements, suggestions),
                difficultyBeatmap ?? new EmptyDifficultyBeatmap(),
                previewBeatmapLevel ?? new EmptyBeatmapLevel(),
                moduleArgs,
                levelType
            };

            if (!_sorted)
            {
                _modules = _modules.OrderByDescending(n => n.Priority).ToList();
                Log.Logger.Log($"Modules registered: {string.Join(", ", _modules.Select(n => $"[{n}]"))}", Logger.Level.Trace);
                _sorted = true;
            }

            List<Module> queue = _modules.ToList();
            HashSet<Module> active = new();

            void InitializeModule(Module module, bool depended = false)
            {
                // Remove module from processing queue
                queue.Remove(module);

                // skip disabled modules
                if (!module.Enabled)
                {
                    return;
                }

                MethodInfo callBack = module.Callback;

                // force fail on all modules
                if (disableAll)
                {
                    goto fail;
                }

                // check requirement
                switch (module.RequirementType)
                {
                    case RequirementType.None:
                        if (!depended)
                        {
                            Log.Logger.Log($"[{module.Id}] not requested by any other module, skipping.", Logger.Level.Trace);
                            goto fail;
                        }

                        break;
                    case RequirementType.Condition:
                        MethodInfo condition = module.ConditionCallback!;
                        if (!(bool)condition.Invoke(null, condition.ActualParameters(inputs.AddToArray(depended))))
                        {
                            Log.Logger.Log($"[{module.Id}] did not pass condition, skipping.", Logger.Level.Trace);
                            goto fail;
                        }

                        break;
                    case RequirementType.Always:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(module.RequirementType), "Was not valid RequirementType.");
                }

                // Handle conflicts
                foreach (string conflictId in module.Conflict)
                {
                    Module? conflict = active.FirstOrDefault(n => n.Id == conflictId);
                    if (conflict == null)
                    {
                        continue;
                    }

                    Log.Logger.Log($"[{module.Id}] conflicts with [{conflictId}], skipping.", Logger.Level.Trace);
                    goto fail;
                }

                // handle dependencies
                foreach (string dependId in module.Depends)
                {
                    Module? depend = queue.FirstOrDefault(n => n.Id == dependId);
                    if (depend != null)
                    {
                        InitializeModule(depend, true);
                    }

                    depend ??= active.FirstOrDefault(n => n.Id == dependId);
                    if (active.Contains(depend))
                    {
                        continue;
                    }

                    throw new InvalidOperationException($"[{module.Id}] requires [{dependId}] but it is not available.");
                }

                // passed the checks, initilaize
                active.Add(module);
                try
                {
                    callBack.Invoke(null, callBack.ActualParameters(inputs.AddToArray(true)));
                    Log.Logger.Log($"[{module.Id}] loaded.", Logger.Level.Trace);
                }
                catch
                {
                    Log.Logger.Log($"Exception while loading [{module.Id}].", Logger.Level.Critical);
                    throw;
                }

                return;

                // goto just seemed to work for this
                fail:
                callBack.Invoke(null, callBack.ActualParameters(inputs.AddToArray(false)));
            }

            while (queue.Any())
            {
                InitializeModule(queue.First());
            }

            overrideEnvironmentSettings = moduleArgs.OverrideEnvironmentSettings;
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
}
