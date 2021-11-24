using UnityEngine;

namespace Chroma.Colorizer.Monobehaviours
{
    internal class ChromaSaberController : MonoBehaviour
    {
        private SaberType _saberType;
        private SaberColorizer _colorizer = null!;

        internal void Init(Saber saber)
        {
            _saberType = saber.saberType;
            _colorizer = SaberColorizer.Create(saber);
        }

        private void OnDestroy()
        {
            _saberType.GetSaberColorizers().Remove(_colorizer);
        }
    }
}
