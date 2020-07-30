using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace SR_PluginLoader
{
    public class ButtonStyler_Overrider : SRBehaviour
    {
        private Button button;
        public Color? normalColor = null, highlightedColor = null, pressedColor = null;
        public Color? normalText = null, highlightedText = null, pressedText = null;
        private bool apply_next_frame = false;

        public ButtonStyler_Overrider() { }

        public void OnEnable()
        {
            button = base.GetComponent<Button>();
            apply_next_frame = true;
        }

        private void Update()
        {
            if (!apply_next_frame) return;
            apply_next_frame = false;
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            List<Text> list = new List<Text>();
            Text[] componentsInChildren = base.GetComponentsInChildren<Text>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Text text = componentsInChildren[i];
                if (!text.GetComponent<TextStyler>())
                {
                    list.Add(text);
                }
            }
            foreach (Text current in list)
            {
                StyleText(current);
            }


            ColorBlock colors = button.colors;
            if (normalColor.HasValue) colors.normalColor = normalColor.Value;
            if (highlightedColor.HasValue) colors.highlightedColor = highlightedColor.Value;
            if (pressedColor.HasValue) colors.pressedColor = pressedColor.Value;

            button.colors = colors;
        }

        private void StyleText(Text text)
        {
            if (normalText.HasValue)
            {
                var clr = new UIStyleDirector.ColorSetting() { apply = true, value = normalText.Value };
                var fnt = new UIStyleDirector.FontSetting() { apply = true, value = text.font };
                var fsz = new UIStyleDirector.IntSetting() { apply = false };
                var fss = new UIStyleDirector.FontStyleSetting() { apply = false };
                var foc = new UIStyleDirector.ColorSetting() { apply = true };
                var fow = new UIStyleDirector.FloatSetting() { apply = true };

                TextStyler.ApplyTextStyle(text, clr, fnt, fsz, fss, foc, fow);
            }
        }
    }
}
