using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    class Dev_MenuTab
    {
        public string Title, Description;
        public uiTab Tab = null;
        public Dev_Menu_Type Type = Dev_Menu_Type.NONE;

        public Dev_MenuTab(string title, string desc)
        {
            Title = title;
            Description = desc;
        }
    }

    enum Dev_Menu_Type
    {
        NONE=0,
        HIERARCHY,
        SPAWN,
    }

    class DevMenu : uiWindow
    {
        private uiListView list;
        private uiTabPanel tabPanel = null;
        private List<uiControl> panels = new List<uiControl>();
        private Dev_Menu_Type activeMenu = Dev_Menu_Type.NONE;
        private Dictionary<Dev_Menu_Type, Dev_MenuTab> Menus = new Dictionary<Dev_Menu_Type, Dev_MenuTab>()
        {
            { Dev_Menu_Type.HIERARCHY, new Dev_MenuTab("Hierarchy", "Browse the hierarchy of all current GameObjects.") },
            { Dev_Menu_Type.SPAWN, new Dev_MenuTab("Spawn", "Pick and spawn items from a list.") },
        };


        public DevMenu()
        {
            onLayout += DevMenu_onLayout;
            Title = "DevMenu";
            Set_Size(800, 600);
            Center();

            list = uiControl.Create<uiListView>(this);
            list.alignTop();
            list.alignLeftSide();
            list.Set_Margin(0, 4, 0, 0);

            tabPanel = uiControl.Create<uiTabPanel>(this);
            tabPanel.Autosize_Method = AutosizeMethod.FILL;
            //tabPanel.local_style.normal.background = null;

            foreach (var kvp in Menus)
            {
                string eStr = Enum.GetName(typeof(Dev_Menu_Type), kvp.Key);
                var tab = tabPanel.Add_Tab();
                tab.Scrollable = false;
                Menus[kvp.Key].Tab = tab;
                Menus[kvp.Key].Type = kvp.Key;

                switch (kvp.Key)
                {
                    case Dev_Menu_Type.HIERARCHY:
                        Create<Dev_Hierarchy_Browser>(tab);
                        break;
                    case Dev_Menu_Type.SPAWN:
                        Create<Dev_SpawnMenu>(tab);
                        break;
                    default:
                        SLog.Warn("Unhandled Dev_Menu type: {0}", eStr);
                        break;
                }
                
                var itm = uiControl.Create<uiListItem>();
                itm.Title = kvp.Value.Title;
                itm.Description = kvp.Value.Description;
                itm.onSelected += (uiControl c) => { Set_Active_Menu(kvp.Value.Type); };
                list.Add(itm);
            }
        }

        private void DevMenu_onLayout(uiPanel c)
        {
            list.Set_Width(200f);
            list.FloodY();

            tabPanel.alignTop();
            tabPanel.moveRightOf(list);
        }

        private void Set_Active_Menu(Dev_Menu_Type newMenu)
        {
            //SLog.Info("Set_Active_Menu: {0}", Enum.GetName(typeof(Dev_Menu_Type), newMenu));
            activeMenu = newMenu;
            Dev_MenuTab menu = null;
            if (Menus.TryGetValue(newMenu, out menu)) menu.Tab.Select();
        }
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))// ` eg: ~
            {
                ToggleShow();
            }
        }
        
    }
}
