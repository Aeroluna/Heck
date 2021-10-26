namespace Heck.Animation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class TrackBuilder
    {
        public static event Action<Track>? TrackCreated;

        public IDictionary<string, Track> Tracks { get; } = new Dictionary<string, Track>();

        public Track AddTrack(string trackName)
        {
            if (!Tracks.TryGetValue(trackName, out Track track))
            {
                track = new Track();
                TrackCreated?.Invoke(track);
                Tracks.Add(trackName, track);
            }

            return track;
        }
    }

    public class Track
    {
        public event Action<GameObject>? OnGameObjectAdded;

        public event Action<GameObject>? OnGameObjectRemoved;

        public HashSet<GameObject> GameObjects { get; } = new HashSet<GameObject>();

        internal IDictionary<string, Property> Properties { get; } = new Dictionary<string, Property>();

        internal IDictionary<string, Property> PathProperties { get; } = new Dictionary<string, Property>();

        public void AddGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Add(gameObject);
            OnGameObjectAdded?.Invoke(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            GameObjects.Remove(gameObject);
            OnGameObjectRemoved?.Invoke(gameObject);
        }

        public void AddProperty(string name, PropertyType propertyType)
        {
            if (Properties.ContainsKey(name))
            {
                Plugin.Logger.Log($"Duplicate property {name}, skipping...", IPA.Logging.Logger.Level.Trace);
            }
            else
            {
                Properties.Add(name, new Property(propertyType));
            }
        }

        public void AddPathProperty(string name, PropertyType propertyType)
        {
            if (PathProperties.ContainsKey(name))
            {
                Plugin.Logger.Log($"Duplicate path property {name}, skipping...", IPA.Logging.Logger.Level.Trace);
            }
            else
            {
                PathProperties.Add(name, new PathProperty(propertyType));
            }
        }
    }
}
