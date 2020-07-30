using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public interface ILayoutDirector
    {
        void Handle(uiPanel parent, uiControl[] list);
    }


    public class Layout_Default : ILayoutDirector
    {
        public Layout_Default() { }

        public void Handle(uiPanel parent, uiControl[] controls)
        {
            uiControl last = null;
            foreach (uiControl c in controls.Where(c => c!=null))
            {
                if (last == null) c.alignTop();
                else c.moveBelow(last);

                last = c;
            }
        }
    }

    /// <summary>
    /// Orders all items in the parent control into a list and makes them all to be the same size.
    /// </summary>
    public class Layout_IconList : ILayoutDirector
    {
        /// <summary>
        /// All controls managed by this layout will be the same size...
        /// </summary>
        private Vector2 SIZE = Vector2.zero;
        public Layout_IconList(float sz) { SIZE = new Vector2(sz, sz); }
        public Layout_IconList(float w, float h) { SIZE = new Vector2(w, h); }

        public void Handle(uiPanel parent, uiControl[] controls)
        {
            var inner_area = parent.Get_Content_Area();
            int COLS = Math.Max(1, (int)Math.Floor(inner_area.width / SIZE.x));
            float cw = (float)COLS * SIZE.x;// content width
            float exSpace = (inner_area.width - cw);// left over space
            float off = exSpace * 0.5f;// How much offset to add to X on each row

            for(int i=0; i<controls.Length; i++)
            {
                uiControl c = controls[i];
                int x = (i % COLS);
                int y = (i / COLS);

                c.Set_Size(SIZE);
                float X = ((float)x * SIZE.x) + off;
                float Y = ((float)y * SIZE.y);
                c.Set_Pos(X, Y);
            }
            
        }
    }
}
