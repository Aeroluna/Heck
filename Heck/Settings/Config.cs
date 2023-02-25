using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using JetBrains.Annotations;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

// ReSharper disable MemberCanBeMadeStatic.Global
namespace Heck.Settings
{
    internal class Config
    {
        [UsedImplicitly]
        public ReLoaderSettings ReLoader { get; set; } = new();

        public class ReLoaderSettings
        {
            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode Reload { get; set; } = KeyCode.Space;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode SaveTime { get; set; } = KeyCode.LeftControl;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode JumpToSavedTime { get; set; } = KeyCode.Space;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode ScrubBackwards { get; set; } = KeyCode.LeftArrow;

            [UsedImplicitly]
            [UseConverter(typeof(EnumConverter<KeyCode>))]
            public KeyCode ScrubForwards { get; set; } = KeyCode.RightArrow;

            [UsedImplicitly]
            public float ScrubIncrement { get; set; } = 5;

            [UsedImplicitly]
            public bool ReloadOnRestart { get; set; }
        }
    }
}
