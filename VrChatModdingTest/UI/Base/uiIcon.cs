using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SR_PluginLoader
{
    public class uiIcon : uiControl
    {
        private Texture2D img_mouse_over=null, img_mouse_down=null;

        private bool maintains_aspect = true;
        public bool MaintainAspectRatio { get { return maintains_aspect; }  set { maintains_aspect = value;  update_area(); } }

        public Texture2D Image { get { return (Texture2D)content.image; } set { content.image = value; update_size(); } }
        public Texture2D Image_MouseOver { get { return img_mouse_over; } set { img_mouse_over = value; update_size(); } }
        public Texture2D Image_MouseDown { get { return img_mouse_down; } set { img_mouse_down = value; update_size(); } }


        /// <summary>
        /// THIS IS THE GETTER THAT SHOULD BE USED BY THIS CONTROL TO OBTAIN THE IMAGE IT SHOULD CURRENTLY BE DISPLAYING!
        /// </summary>
        protected Texture2D CurrentImage {
            get
            {
                if (isMouseOver && img_mouse_over!=null) return img_mouse_over;
                else if (isMouseDown && img_mouse_down!=null) return img_mouse_down;
                return (Texture2D)content.image;
            }
        }
        private Texture2D last_img = null;// The image that we were displaying last time we checked the images status, used to trigger an area update when the image we are displaying changes.


        public override bool isDisplayed { get { if (CurrentImage == null) { return false; } return base.isDisplayed; } set { base.isDisplayed = value; } } 
        protected override Vector2 size { get { if (CurrentImage == null) { return new Vector2(0f, 0f); } return base.size; } }
        protected override Vector2 Get_Autosize(Vector2? starting_size = null)
        {
            if (CurrentImage != null)
            {
                Vector2 csz = content_size_to_inner(new Vector2(CurrentImage.width, CurrentImage.height));
                // This control's only INTENDED content is an image, which should maintain it's aspect ratio, unless told not to.
                if (maintains_aspect)
                {
                    if (Explicit_H ^ Explicit_W)
                    {
                        if (Explicit_W) csz.y = (_size.x * (CurrentImage.height / CurrentImage.width));
                        else csz.x = (_size.y * (CurrentImage.width / CurrentImage.height));
                    }
                    else// If we don't have an explicit Width/Height given to us, then we find our smallest dimension and size according to it because the smaller one is the one being constrained by some factor.
                    {
                        if (csz.x < csz.y) csz.y = (csz.x * (CurrentImage.height / CurrentImage.width));
                        else csz.x = (csz.y * (CurrentImage.width / CurrentImage.height));
                    }
                }
                return base.Get_Autosize(csz);
            }
            return base.Get_Autosize(starting_size);
        }


        public uiIcon() : base(uiControlType.Icon)
        {
            Autosize = true;//auto size by default until the user gives us an explicit size
            //Set_Background(Color.clear);
            disableBG = true;

            onThink += UiIcon_onThink;
        }

        private void UiIcon_onThink()
        {
            // We want to trigger a size update on ourself whenever the image we are drawing changes.
            Texture2D cimg = CurrentImage;
            if(last_img != cimg)
            {
                last_img = cimg;
                update_size();
            }
        }

        protected override void Display()
        {
            Display_BG();// Draw Background
            // Previously the DrawTexture call used 'draw_area' for the area, idk why, short sightedness probably.
            if(CurrentImage != null) GUI.DrawTexture(inner_area, CurrentImage, ScaleMode.ScaleToFit);
        }
    }
}
