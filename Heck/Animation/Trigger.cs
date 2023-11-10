using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heck.Animation
{
    internal class Trigger
    {
        public static Dictionary<string, Trigger> triggers = new Dictionary<string, Trigger>();

        public Trigger(string _name) {
            name = _name;
        }

        public bool isTriggered = false;

        public string name;

        public static IEnumerable<Trigger> GetTriggers(CustomData data, string field)
        {
            object? triggerNameRaw = data.Get<object>(field);
            if (triggerNameRaw == null)
            {
                return null;
            }

            IEnumerable<string> triggerNames;
            if (triggerNameRaw is List<object> listTrack)
            {
                triggerNames = listTrack.Cast<string>();
            }
            else
            {
                triggerNames = new[] { (string)triggerNameRaw };
            }

            HashSet<Trigger> result = new();
            foreach (string triggerName in triggerNames)
            {
                if (triggers.ContainsKey(triggerName))
                {
                    result.Add(triggers[triggerName]);
                }
                else
                {
                    Trigger trigger = new Trigger(triggerName);
                    result.Add(trigger);
                    triggers.Add(triggerName, trigger);
                }
            }

            return result;
        }
    }
}
