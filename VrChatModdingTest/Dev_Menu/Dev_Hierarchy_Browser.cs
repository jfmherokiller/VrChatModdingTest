using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class Dev_Hierarchy_Browser: uiPanel
    {
        GameObject selection = null;

        uiListView list = null;
        uiPanel info_panel = null;
        uiText lbl_components = null;
        uiListView var_components = null;
        uiVarText var_child_count = null;


        public Dev_Hierarchy_Browser()
        {
            onShown += (uiControl c) => { Refresh(); };
            Autosize = true;
            Autosize_Method = AutosizeMethod.FILL;

            list = uiControl.Create<uiListView>(this);
            list.Set_Width(300);
            list.Autosize = false;
            //list.CONFIRM_LAYOUT = true;


            info_panel = uiControl.Create<uiPanel>(this);
            info_panel.Scrollable = false;
            info_panel.Autosize = true;
            info_panel.Autosize_Method = AutosizeMethod.FILL;
            info_panel.onLayout += Info_panel_onLayout;
            info_panel.Set_Padding(3);

            lbl_components = Create<uiText>(info_panel);
            lbl_components.Text = "Scripts";

            var_components = Create<uiListView>(info_panel);
            //var_components.Autosize = false;

            var_child_count = Create<uiVarText>(info_panel);
            var_child_count.Text = "Children: ";
        }

        public void Refresh()
        {
            list.Clear_Children();

            HashSet<GameObject> rootObjects = new HashSet<GameObject>();
            foreach (GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (go.transform == null)
                {
                    rootObjects.Add(go);
                }
                else
                {
                    Transform xform = go.transform;
                    if (xform.parent == null && xform.gameObject != null)
                    {
                        rootObjects.Add(xform.gameObject);
                    }
                }
            }

            foreach (GameObject go in rootObjects)
            {
                var node = Spawn_Node(go, list);
                Populate_Single_Node(node);
            }

            set_layout_dirty();
            //set_area_dirty();
        }

        public void Collapse_All()
        {
            foreach(uiList_TreeNode node in list.Get_Children())
            {
                node.Collapse();
            }
        }

        private void Update_info()
        {
            var_child_count.Value = (selection == null ? 0 : selection.transform.childCount).ToString();
            var_components.Clear_Children();
            if(selection!=null)
            {
                Component[] comps = selection.GetComponents<Component>();
                foreach(var c in comps)
                {
                    var n = Create<uiList_TreeNode>(var_components);
                    n.Title = c.GetType().Name;
                    n.TextSize = 12;
                    n.Selectable = false;
                }
            }
        }

        private void Info_panel_onLayout(uiPanel c)
        {
            var_child_count.alignTop();

            lbl_components.moveBelow(var_child_count);

            var_components.moveBelow(lbl_components);
            var_components.FloodXY();
        }

        public override void doLayout()
        {
            base.doLayout();
            list.FloodY();
            info_panel.moveRightOf(list);
            info_panel.FloodXY();
        }
        
        private void Populate_Node(uiList_TreeNode node)
        {
            node.Clear_Children();
            if (node.tag == null) return;
            Populate_Single_Node(node);
            foreach(uiList_TreeNode n in node.Get_Children())
            {
                Populate_Single_Node(n);
            }
        }

        private void Populate_Single_Node(uiList_TreeNode node)
        {
            node.Clear_Children();
            if (node.tag == null) return;
            GameObject gm = (node.tag as GameObject);
            for (int i = 0; i < gm.transform.childCount; i++)
            {
                Transform trans = gm.transform.GetChild(i);
                if (trans.gameObject == trans.parent || trans.parent == null) continue;
                Spawn_Node(trans.gameObject, node);
            }
        }

        private uiList_TreeNode Spawn_Node(GameObject gm, uiPanel parent)
        {
            uiList_TreeNode node = Create<uiList_TreeNode>(parent);
            node.tag = gm;
            node.Title = gm.name;
            node.onExpanded += Populate_Node;
            node.onClicked += (uiControl c) => 
            {
                c.Parent.Collapse_All();
                var sel = ((c as uiList_TreeNode).tag as GameObject);
                if (sel != selection)
                {
                    selection = sel;
                }
                else if(!c.isChild)
                {
                    selection = null;
                }

                Update_info();
                if (selection != null) { (c as uiList_TreeNode).Expand(); }
            };
            return node;
        }
    }
}
