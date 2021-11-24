using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.Colorizer
{
    public class BombColorizer : ObjectColorizer
    {
        private static readonly int _simpleColor = Shader.PropertyToID("_SimpleColor");
        private readonly Renderer _bombRenderer;

        private BombColorizer(NoteControllerBase noteController)
        {
            _bombRenderer = noteController.gameObject.GetComponentInChildren<Renderer>();
            OriginalColor = _bombRenderer.material.GetColor(_simpleColor);
        }

        public static Dictionary<NoteControllerBase, BombColorizer> Colorizers { get; } = new();

        public static Color? GlobalColor { get; private set; }

        protected override Color? GlobalColorGetter => GlobalColor;

        [UsedImplicitly]
        public static void GlobalColorize(Color? color)
        {
            GlobalColor = color;
            foreach (KeyValuePair<NoteControllerBase, BombColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        internal static void Create(NoteControllerBase noteController)
        {
            Colorizers.Add(noteController, new BombColorizer(noteController));
        }

        internal static void Reset()
        {
            GlobalColor = null;
        }

        protected override void Refresh()
        {
            Material bombMaterial = _bombRenderer.material;
            Color color = Color;
            if (color == bombMaterial.GetColor(_simpleColor))
            {
                return;
            }

            bombMaterial.SetColor(_simpleColor, color);
        }
    }
}
