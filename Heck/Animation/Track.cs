using System;
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Heck.HeckController;

namespace Heck.Animation
{
    public class TrackBuilder
    {
        public Dictionary<string, Track> Tracks { get; } = new();

        public void AddTrack(string trackName)
        {
            if (Tracks.ContainsKey(trackName))
            {
                return;
            }

            Track track = new();
            Tracks.Add(trackName, track);
        }

        public void AddManyFromCustomData(CustomData customData, bool v2, bool required = true)
        {
            AddManyFromCustomData(customData, v2 ? V2_TRACK : TRACK, required);
        }

        [AssertionMethod]
        public void AddManyFromCustomData(CustomData customData, string name, bool required = true)
        {
            object? trackName = customData.Get<object>(name);
            if (trackName != null)
            {
                switch (trackName)
                {
                    case string trackNameStr:
                        AddTrack(trackNameStr);
                        break;

                    case List<string> names:
                        names.ForEach(AddTrack);
                        break;
                }
            }
            else if (required)
            {
                throw new JsonNotDefinedException(name);
            }
        }

        public void AddFromCustomData(CustomData customData, bool v2, bool required = true)
        {
            AddFromCustomData(customData, v2 ? V2_TRACK : TRACK, required);
        }

        [AssertionMethod]
        public void AddFromCustomData(CustomData customData, string name, bool required = true)
        {
            string? trackName = customData.Get<string>(name);
            if (trackName != null)
            {
                AddTrack(trackName);
            }
            else if (required)
            {
                throw new JsonNotDefinedException(name);
            }
        }
    }

    public class Track
    {
        private static readonly Dictionary<string, IPropertyBuilder> _registeredProperties = new();

        private static readonly Dictionary<string, IPropertyBuilder> _registeredPathProperties = new();

        private static readonly Dictionary<string, List<string>> _aliases = new();

        private static readonly Dictionary<string, List<string>> _pathAliases = new();

        private readonly Dictionary<string, BaseProperty> _properties = new();

        private readonly Dictionary<string, BaseProperty> _pathProperties = new();

        public event Action<GameObject>? GameObjectAdded;

        public event Action<GameObject>? GameObjectRemoved;

        // TODO: use this more and possible replace with frameCount comparisons
        public bool UpdatedThisFrame { get; internal set; }

        public HashSet<GameObject> GameObjects { get; } = new();

        public static void RegisterProperty<T>(string name, string? v2Alias = null)
            where T : struct
        {
            RegisterPropertyInternal<T>(_registeredProperties, _aliases, name, v2Alias);
        }

        public static void RegisterPathProperty<T>(string name, string? v2Alias = null)
            where T : struct
        {
            RegisterPropertyInternal<T>(_registeredPathProperties, _pathAliases, name, v2Alias);
        }

        public void AddGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Add(gameObject);
            GameObjectAdded?.Invoke(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Remove(gameObject);
            GameObjectRemoved?.Invoke(gameObject);
        }

        internal static IEnumerable<string>? GetAliases(string name)
        {
            return _aliases.TryGetValue(name, out List<string> aliases) ? aliases
                : throw new InvalidOperationException($"Could not find alias for [{name}].");
        }

        internal static IEnumerable<string>? GetPathAliases(string name)
        {
            return _pathAliases.TryGetValue(name, out List<string> aliases) ? aliases
                : throw new InvalidOperationException($"Could not find path alias for [{name}].");
        }

        internal static IPropertyBuilder? GetBuilder(string name)
        {
            return _registeredProperties.TryGetValue(name, out IPropertyBuilder propertyBuilder) ? propertyBuilder : null;
        }

        internal static IPropertyBuilder? GetPathBuilder(string name)
        {
            return _registeredPathProperties.TryGetValue(name, out IPropertyBuilder propertyBuilder) ? propertyBuilder : null;
        }

        internal Property<T>? FindProperty<T>(string name)
            where T : struct
        {
            if (!_properties.TryGetValue(name, out BaseProperty property))
            {
                return null;
            }

            if (property is Property<T> result)
            {
                return result;
            }

            throw new InvalidOperationException(
                $"Path property [{name}] was wrong type. Expected: [{typeof(Property<T>).Name}], was: [{property.GetType().Name}]");
        }

        internal PathProperty<T>? FindPathProperty<T>(string name)
            where T : struct
        {
            if (!_pathProperties.TryGetValue(name, out BaseProperty property))
            {
                return null;
            }

            if (property is PathProperty<T> result)
            {
                return result;
            }

            throw new InvalidOperationException(
                $"Path property [{name}] was wrong type. Expected: [{typeof(PathProperty<T>).Name}], was: [{property.GetType().Name}]");
        }

        internal BaseProperty GetOrCreateProperty(string name, IPropertyBuilder propertyBuilder)
        {
            if (_properties.TryGetValue(name, out BaseProperty property))
            {
                return property;
            }

            property = propertyBuilder.Property;
            _properties[name] = property;
            return property;
        }

        internal BaseProperty GetOrCreatePathProperty(string name, IPropertyBuilder propertyBuilder)
        {
            if (_pathProperties.TryGetValue(name, out BaseProperty property))
            {
                return property;
            }

            property = propertyBuilder.PathProperty;
            _pathProperties[name] = property;
            return property;
        }

        private static void RegisterPropertyInternal<T>(
            Dictionary<string, IPropertyBuilder> registered,
            Dictionary<string, List<string>> registeredAliases,
            string name,
            string? v2Alias)
            where T : struct
        {
            registered.Add(name, new PropertyBuilder<T>());

            if (v2Alias == null)
            {
                return;
            }

            if (!registeredAliases.TryGetValue(v2Alias, out List<string> aliases))
            {
                aliases = new List<string>();
                registeredAliases[v2Alias] = aliases;
            }

            aliases.Add(name);
        }
    }
}
