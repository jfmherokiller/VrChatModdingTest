using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiBorderStyleState
    {
        public int? stipple_size = null;// The size of each stipple segment
        public int? stipple_gap = null;// Size of the space between each stippled segment (defaults to 1 if null)
        public RectOffset size = null;
        private Color? _color = null;
        public Texture2D texture = null;
        //public Color color { get { return (this.texture == null ? Color.clear : this.texture.GetPixel(0, 0)); } set { _color = value; if (this.texture == null) { this.texture = new Texture2D(1, 1); } texture.SetPixel(0, 0, value); texture.Apply(); } }
        public Color? color { get { return _color; } set { _color = value; texture = null; if (size == null) { size = new RectOffset(1, 1, 1, 1); } } }

        public uiBorderStyleState()
        {
        }

        public uiBorderStyleState(uiBorderStyleState st)
        {
            this.size = st.size;
            this.color = st.color;
            this.stipple_size = st.stipple_size;
            this.stipple_gap = st.stipple_gap;
        }


        public void take(uiBorderStyleState st)
        {
            if (st.size != null) size = st.size;
            if (st.color.HasValue) color = st.color;
            if (st.texture != null) texture = st.texture;
            if (st.stipple_size.HasValue) stipple_size = st.stipple_size;
            if (st.stipple_gap.HasValue) stipple_gap = st.stipple_gap;
        }

        public void reset()
        {
            size = new RectOffset();
            texture = null;
            color = null;
        }

        public void prepare_texture(Vector2 sz)
        {
            if ((int)sz.x <= 0 || (int)sz.y <= 0) return;
            if (!color.HasValue) return;
            texture = new Texture2D((int)sz.x, (int)sz.y);
            //PLog.Info("Preparing texture ({0}, {1}), {2}", texture.width, texture.height, color);

            int stpl_sz = 0;
            if (stipple_size.HasValue) stpl_sz = stipple_size.Value;
            int stpl_gap = 1;
            if (stipple_gap.HasValue) stpl_gap = stipple_gap.Value;

            int stpl_sum = (stpl_sz + stpl_gap);

            for (int x = 0; x < texture.width; x++)
            {
                bool mask_x = (x % stpl_sum < stpl_sz || stpl_sz == 0);// stippling mask for X value
                bool xleft = false;
                bool xright = false;
                if (size.left > 0 && x >= 0 && x < size.left) xleft = true;
                if (size.right > 0 && x >= (texture.width - size.right) && x < texture.width) xright = true;

                for (int y = 0; y < texture.height; y++)
                {
                    bool mask_y = (y % stpl_sum < stpl_sz || stpl_sz == 0);// stippling mask for Y value
                    bool ytop = false;
                    bool ybottom = false;
                    if (size.top > 0 && y >= 0 && y < size.top) ytop = true;
                    if (size.bottom > 0 && y >= (texture.height - size.bottom) && y < texture.height) ybottom = true;

                    bool withinBounds = ((xleft || xright || ytop || ybottom) && ((xleft ^ xright) && mask_y) || ((ytop ^ ybottom) && mask_x));//does the current pixel fall on the border line?

                    texture.SetPixel(x, y, (withinBounds ? color.Value : Color.clear));
                }
            }

            texture.Apply();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            uiBorderStyleState o = (uiBorderStyleState)obj;
            bool sizeEq = (((o.size == null) == (this.size == null)) && Util.floatEq(o.size.left, size.left) && Util.floatEq(o.size.right, size.right) && Util.floatEq(o.size.top, size.top) && Util.floatEq(o.size.bottom, size.bottom));
            bool colorEq = (o.color.HasValue == this.color.HasValue);
            //if both colors have a value then we want to be more specific with our equality check.
            if (colorEq && this.color.HasValue) colorEq = (Util.floatEq(o.color.Value.r, this.color.Value.r) && Util.floatEq(o.color.Value.b, this.color.Value.b) && Util.floatEq(o.color.Value.g, this.color.Value.g) && Util.floatEq(o.color.Value.a, this.color.Value.a));

            return (sizeEq && colorEq);
        }
    }
}
