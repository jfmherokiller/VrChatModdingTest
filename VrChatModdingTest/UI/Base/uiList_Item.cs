using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// An item that goes into a uiListView.
    /// Has: an icon, title, and description
    /// </summary>
    public class uiListItem : uiPanel
    {
        protected uiText title = null;
        protected uiIcon icon = null;
        protected uiTextArea description = null;

        /*
        public override int TextSize { get { return title.TextSize; } set { title.TextSize = value; } }
        public override Color TextColor { get { return title.TextColor; } set { title.TextColor = value; } }
        public override FontStyle TextStyle { get { return title.TextStyle; } set { title.TextStyle = value; } }
        public override TextAnchor TextAlign { get { return title.TextAlign; } set { title.TextAlign = value; } }
        public override Color TextColor_Hover { get { return title.TextColor_Hover; } set { title.TextColor_Hover = value; } }
        */

        public string Title { get { return title.Text; } set { title.Text = value; } }
        public string Description { get { return description.Text; } set { description.Text = value; } }
        public Texture2D Icon { get { return icon.Image; } set { icon.Image = value; } }



        public uiListItem() : base(uiControlType.ListItem) { init(); }
        public uiListItem(uiControlType ty) : base(ty) { init(); }


        private void init()
        {
            TextStyle = FontStyle.Bold;
            Clickable = true;
            Selectable = true;
            onClicked += (uiControl c) => { if (_selectable) { Selected = !Selected; } };
            Autosize = true;
            //Autosize_Method = AutosizeMethod.BLOCK;

            Set_Padding(2);
            Set_Margin(2);

            Util.Set_BG_Color(local_style.normal, new Color32(32, 32, 32, 150));
            Util.Set_BG_Color(local_style.hover, new Color32(36, 36, 36, 255));
            Util.Set_BG_Color(local_style.active, new Color(0.2f, 0.4f, 1f, 1f));
            //Util.Set_BG_Color(local_style.active, new Color32(55, 55, 55, 255));

            Border.active.color = new Color32(255, 255, 255, 200);

            icon = uiControl.Create<uiIcon>("icon", this);
            icon.SizeConstraint = uiSizeConstraint.WIDTH_MATCHES_HEIGHT;
            icon.Set_Padding(2);
            icon.Autosize = true;

            title = uiControl.Create<uiText>("title", this);
            //title.TextSize = 12;
            title.inherits_text_style = true;


            description = uiControl.Create<uiTextArea>("desc", this);
            description.Set_Margin(1, 1, 1, 1);
            description.TextSize = 12;
            description.TextStyle = FontStyle.Italic;
            description.TextColor = new Color32(255, 255, 255, 180);
            description.Text = null;// collapsed by default.

        }
        
        protected override Vector2 Get_Autosize(Vector2? starting_size = null)
        {
            var sz = base.Get_Autosize(starting_size);
            if(parent != null) sz.x = outter_area_to_inner(parent.Area).width;

            return sz;
        }

        public override void doLayout()
        {
            icon.alignTop();
            icon.alignLeftSide();

            title.alignTop();
            title.moveRightOf(icon);
            title.FloodX();

            description.alignLeftSide();
            description.moveBelow(title);
        }
    }
}
