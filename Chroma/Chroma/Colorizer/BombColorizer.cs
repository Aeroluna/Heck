namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BombColorizer : ObjectColorizer
    {
        private readonly Renderer _bombRenderer;

        internal BombColorizer(NoteControllerBase noteController)
        {
            _bombRenderer = noteController.gameObject.GetComponentInChildren<Renderer>();
            OriginalColor = _bombRenderer.material.GetColor("_SimpleColor");

            Colorizers.Add(noteController, this);
        }

        public static Dictionary<NoteControllerBase, BombColorizer> Colorizers { get; } = new Dictionary<NoteControllerBase, BombColorizer>();

        public static Color? GlobalColor { get; private set; }

        protected override Color? GlobalColorGetter => GlobalColor;

        public static void GlobalColorize(Color? color)
        {
            GlobalColor = color;
            foreach (KeyValuePair<NoteControllerBase, BombColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        protected override void Refresh()
        {
            Material bombMaterial = _bombRenderer.material;
            Color color = Color;
            if (color == bombMaterial.GetColor("_SimpleColor"))
            {
                return;
            }

            bombMaterial.SetColor("_SimpleColor", color);
        }
    }
}
