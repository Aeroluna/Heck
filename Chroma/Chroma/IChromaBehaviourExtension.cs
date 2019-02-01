using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma {

    public interface IChromaBehaviourExtension {

        void PostInitialization(float songBPM, BeatmapData beatmapData, PlayerSpecificSettings playerSettings, ScoreController scoreController);

        void OnEnable();

        void OnDisable();

    }

}
