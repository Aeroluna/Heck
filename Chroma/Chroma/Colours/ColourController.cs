using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.Colours {

    public class ColourController {

        private readonly ColourSelector defaultColorSelector;
        private ColourSelector assignedSelector;
        List<ColourSelector> selectors = new List<ColourSelector>();

        public ColourSelector AssignedSelector {
            get { return assignedSelector; }
            set { assignedSelector = value; }
        }

        public ColourSelector DefaultColorSelector {
            get { return defaultColorSelector; }
        }

        public ColourController(Color defaultColor, params ColourSelector[] selectors) {
            this.defaultColorSelector = new SimpleColourSelector(defaultColor);
            this.assignedSelector = defaultColorSelector;
            for (int i = 0; i < selectors.Length; i++) {
                this.selectors.Add(selectors[i]);
            }
        }

        public ColourController(ColourSelector defaultColorSelector, params ColourSelector[] selectors) {
            this.defaultColorSelector = defaultColorSelector;
            this.assignedSelector = defaultColorSelector;
            for (int i = 0; i < selectors.Length; i++) {
                this.selectors.Add(selectors[i]);
            }
        }

        public void AddSelector(ColourSelector selector) {
            if (selectors.Contains(selector)) return;
            selectors.Add(selector);
            SortSelectors();
        }

        public void RemoveSelector(ColourSelector selector) {
            if (selectors.Remove(selector)) SortSelectors();
        }

        public void SortSelectors() {
            selectors = selectors.OrderBy(x=>x.priority).ToList();
        }

        public Color GetColor() {
            return GetColor(Time.time);
        }

        public Color GetColor(float time) {
            Color color = Color.clear;
            for (int i = 0; i < selectors.Count; i++) {
                if (selectors[i].ChanceSuccess()) color = selectors[i].GetColor(time);
                if (color != Color.clear) return color;
            }
            color = assignedSelector.GetColor();
            return color == Color.clear ? defaultColorSelector.GetColor(time) : color;
        }

        public Color GetReverseColor() {
            return GetReverseColor(Time.time);
        }

        public Color GetReverseColor(float time) {
            Color color = Color.clear;
            for (int i = selectors.Count - 1; i >= 0; i--) {
                if (selectors[i].ChanceSuccess()) color = selectors[i].GetColor(time);
                if (color != Color.clear) return color;
            }
            color = assignedSelector.GetColor();
            return color == Color.clear ? defaultColorSelector.GetColor(time) : color;
        }

    }

}
