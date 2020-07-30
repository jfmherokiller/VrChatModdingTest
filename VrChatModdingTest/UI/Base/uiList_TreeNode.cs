using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// A collapsable tree-view node, such as one would see in a file browser.
    /// </summary>
    public class uiList_TreeNode : uiWrapperPanel, ICollapsable
    {
        public object tag = null;
        public event Action<uiList_TreeNode> onCollapsed, onExpanded;
        public string Title { get { return title.Text; } set { title.Text = value; } }

        #region Controls

        uiText title = null;
        uiIcon icon = null;
        uiCollapser collapser = null;
        uiListView list = null;
        #endregion

        protected override List<uiControl> children { get { if (list == null) { return null; } return list.Get_Children(); } }
        public override void Clear_Children() { list.Clear_Children(); update_icon(); }
        public override uiControl Add(uiControl c) { list.Add(c); update_icon(); return c; }

        public uiList_TreeNode() : base(uiControlType.ListItem) { init(); }
        public uiList_TreeNode(uiControlType ty) : base(ty) { init(); }
        

        private void init()
        {
            Clickable = true;
            Selectable = true;
            Autosize_Method = AutosizeMethod.BLOCK;
            Autosize = true;
            onClicked += UiList_TreeNode_onClicked;

            Util.Set_BG_Color(local_style.normal, Color.clear);
            Util.Set_BG_Color(local_style.hover, new Color(0.2f, 0.2f, 0.2f, 0.4f));

            Set_Padding(2);
            Set_Margin(0);

            title = uiControl.Create<uiText>("title");
            Add_Control(title);
            title.disableBG = true;
            title.TextSize = 12;

            icon = Create<uiIcon>();
            Add_Control(icon);
            icon.Set_Margin(0, 4);

            collapser = uiControl.Create<uiCollapser>();
            Add_Control(collapser);
            collapser.Autosize_Method = AutosizeMethod.BLOCK;
            collapser.Autosize = true;
            collapser.Set_Margin(6, 0, 0, 0);
            collapser.Set_Padding(0);
            collapser.onCollapsed += (uiCollapser) => { onCollapsed?.Invoke(this); update_icon(); };
            collapser.onExpanded += (uiCollapser) => { onExpanded?.Invoke(this); update_icon(); };
            collapser.Collapse();

            list = uiControl.Create<uiListView>(collapser);
            list.Scrollable = false;
            list.Autosize_Method = AutosizeMethod.BLOCK;
            list.Autosize = true;
            list.disableBG = true;
        }

        private void update_icon()
        {
            var clist = Get_Children();
            if (clist == null || clist.Count <= 0) { icon.Image = null; }
            else
            {
                if (collapser.isCollapsed) { icon.Image = TextureHelper.icon_node_arrow_right; }
                else { icon.Image = TextureHelper.icon_node_arrow_down; }
            }
            set_layout_dirty();
        }

        private void UiList_TreeNode_onClicked(uiControl c) { collapser.Set_Collapsed(!collapser.isCollapsed); }

        public override void doLayout()
        {
            base.doLayout();

            icon.alignTop();
            icon.Set_Height(title.Get_Height());
            title.alignTop();
            title.moveRightOf(icon);
            //title.FloodX();

            collapser.moveBelow(title);
        }

        public bool Collapse() { return collapser.Collapse(); }
        public bool Expand() { return collapser.Expand(); }

    }
}
