namespace Chroma.Colorizer
{
    using UnityEngine;

    internal class ChromaClashEffectController : MonoBehaviour
    {
        private readonly Color[] _colors = new Color[2];
        private ParticleSystem? _sparkleParticleSystem;
        private ParticleSystem? _glowParticleSystem;

        internal void Init(ParticleSystem sparkleParticleSystem, ParticleSystem glowParticleSystem, ColorManager colorManager)
        {
            _sparkleParticleSystem = sparkleParticleSystem;
            _glowParticleSystem = glowParticleSystem;
            _colors[0] = colorManager.ColorForSaberType(SaberType.SaberA);
            _colors[1] = colorManager.ColorForSaberType(SaberType.SaberB);
            SaberColorizer.SaberColorChanged += OnSaberColorChanged;
        }

        private void OnDestroy()
        {
            SaberColorizer.SaberColorChanged -= OnSaberColorChanged;
        }

        private void OnSaberColorChanged(SaberType saberType, Color color)
        {
            _colors[(int)saberType] = color;

            Color average = Color.Lerp(_colors[0], _colors[1], 0.5f);
            ParticleSystem.MainModule sparkleMain = _sparkleParticleSystem!.main;
            sparkleMain.startColor = average;
            ParticleSystem.MainModule glowMain = _glowParticleSystem!.main;
            glowMain.startColor = average;
        }
    }
}
