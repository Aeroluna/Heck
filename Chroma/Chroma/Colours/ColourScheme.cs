using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public class ColourScheme {
        
        /*
         * LIGHTS
         */

        //public static Color DefaultLightAmbient { get; set; } = new Color(0, 0.3765f, 0.5f, 1); //0, 192, 255
        public ColourController LightAmbient { get; } = new ColourController(new Color(0, 0.706f, 1f, 1));

        public ColourController LightA { get; } = new ColourController(new Color(1, 0.016f, 0.016f, 1)); //255, 4, 4

        public ColourController LightB { get; } = new ColourController(new Color(0, 0.753f, 1, 1)); //0, 192, 255

        public ColourController LightAltA { get; } = new ColourController(new Color(1, 0.032f, 1, 1)); //255, 8, 255

        public ColourController LightAltB { get; } = new ColourController(new Color(0.016f, 1, 0.016f, 1)); //4, 255, 4

        public ColourController LightWhite { get; } = new ColourController(new Color(1, 1, 1, 1)); //Color.white

        public ColourController LightGrey { get; } = new ColourController(new Color(0.6f, 0.6f, 0.6f, 1)); //Color.white

        /*
         * BLOCKS / SABERS
         */

        public ColourController A { get; } = new ColourController(new Color(1, 0, 0, 1));

        public ColourController B { get; } = new ColourController(new Color(0, 0.502f, 1, 1));

        public ColourController AltA { get; } = new ColourController(new Color(1, 0, 1, 1)); //Color.magenta

        public ColourController AltB { get; } = new ColourController(new Color(0, 1, 0, 1)); //Color.green

        public ColourController DoubleHit { get; } = new ColourController(new Color(1.05f, 0, 2.188f, 1));

        public ColourController NonColoured { get; } = new ColourController(new Color(1, 1, 1, 1)); //Color.white

        public ColourController Super { get; set; } = new ColourController(new Color(1, 1, 0, 1));

        /*
         * OTHER
         */

        public ColourController BarrierColour { get; } = new ColourController(Color.red);

        public ColourController LaserPointerColour { get; set; } = new ColourController(Color.clear); //B;

        public ColourController SignA { get; set; } = new ColourController(Color.clear); //LightA;

        public ColourController SignB { get; set; } = new ColourController(Color.clear); //LightB;

        public ColourController Platform { get; set; } = new ColourController(Color.clear);

    }

}
