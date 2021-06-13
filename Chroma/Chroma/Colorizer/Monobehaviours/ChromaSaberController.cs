namespace Chroma.Colorizer
{
    using UnityEngine;

    internal class ChromaSaberController : MonoBehaviour
    {
        private SaberType _saberType;
        private SaberColorizer _colorizer;

        internal void Init(Saber saber)
        {
            _saberType = saber.saberType;
            _colorizer = new SaberColorizer(saber);
        }

        private void OnDestroy()
        {
            _saberType.GetSaberColorizers().Remove(_colorizer);
        }
    }
}
