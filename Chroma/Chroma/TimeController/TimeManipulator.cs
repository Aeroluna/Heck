using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.TimeController {

    public class TimeManipulator {

        public enum Operation {
            ADD,
            DIVIDE,
            MULTIPLY,
            SUBTRACT
        }

        private float _value;
        public float Value {
            get { return _value; }
            set {
                _value = value;
                controller.UpdateManipulatedTime();
            }
        }

        private TimeController controller;
        private readonly Operation operation;

        public TimeManipulator(TimeController controller, float value, Operation operation) {
            this.controller = controller;
            this.operation = operation;
            this.controller = controller;
            controller.AddManipulator(this);
            Value = value;
        }

        public void Destroy() {
            controller.RemoveManipulator(this);
        }

        public void Manipulate(ref float t) {
            switch (operation) {
                case Operation.ADD: t += Value; break;
                case Operation.DIVIDE: t /= Value; break;
                case Operation.MULTIPLY: t *= Value; break;
                case Operation.SUBTRACT: t -= Value; break;
            }
        }

    }

}
