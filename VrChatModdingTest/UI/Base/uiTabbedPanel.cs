using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Represents a uiPanel with multiple tabs.
    /// Any added controls get added to the currently active tab.
    /// </summary>
    class uiTabPanel : uiPanel
    {
        private int active_tab = -1;
        public uiTab CurrentTab { get { if (children.Count == 0) { return null; }  if (active_tab >= 0) { return (uiTab)children[active_tab]; } return (uiTab)children[0]; } }

        /// <summary>
        /// Fires when the currently active tab changes.
        /// </summary>
        public event Action<uiTabPanel, uiTab> onChanged;

        #region Constructors

        public uiTabPanel() : base(uiControlType.Panel_Dark) { init(); }
        public uiTabPanel(uiControlType type) : base(type) { init(); }

        private void init()
        {
            Autosize_Method = AutosizeMethod.FILL;
            Set_Margin(0);
            Set_Padding(0);
        }
        #endregion

        #region Tab Management

        /// <summary>
        /// Adds a new tab to the panel and sets it active.
        /// </summary>
        /// <param name="name">The name string to assign the tab, used to retreive it by name from the <c>uiTabbedPanel</c> control</param>
        /// <param name="title">The title text for this tab if tabs are being rendered</param>
        public uiTab Add_Tab(string name=null, string title=null)
        {
            uiTab panel = Create<uiTab>();
            // We don't check if the key already exists in the child map because we WANT it to throw an exception if it's a duplicate, it's the easiest way to let the developer know they need to fix something.
            if (name != null) { child_map.Add(name, panel); }
            panel.Name = name;
            panel.inherits_style = true;
            panel.isVisible = false;
            panel.Autosize_Method = AutosizeMethod.FILL;
            panel.Set_Padding(0);
            
            Add_Control(panel);

            Set_Active_Tab(children.Count-1);//set the active tab to the one was just added
            return panel;
        }

        /// <summary>
        /// Removing tabs by their index would be unreliable, so only removing by name will be supported.
        /// </summary>
        public void Remove_Tab(string name)
        {
            uiScrollPanel tab = (uiScrollPanel)this[name];
            Remove_Control(tab);
        }
        #endregion

        #region Tab Switching

        public void Set_Tab(uiTab tab)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == tab)
                {
                    Set_Active_Tab(i);
                    return;
                }
            }
        }

        public void Select_Tab(string name)
        {
            uiControl p;
            if (!child_map.TryGetValue(name, out p)) throw new KeyNotFoundException(nameof(uiTabPanel)+" Could not find tab with name: "+name);
            Set_Tab((uiTab)p);
        }

        private void Set_Active_Tab(int idx)
        {
            int last_tab = active_tab;
            if(last_tab >= 0) children[last_tab].isVisible = false;
            //foreach (uiScrollPanel tab in children) { tab.isVisible = false; }
            active_tab = idx;
            //PLog.Info("{0}  Switched Tabs  | New: {1} | Old: {2}", this, idx, last_tab);
            children[idx].isVisible = true;
            onChanged?.Invoke(this, (uiTab)children[idx]);
        }
        #endregion

        #region Child-Control Management

        public override uiControl Add(uiControl c) {
            if (CurrentTab == null) { throw new ArgumentNullException(this.ToString() + ": CurrentTab is NULL!"); }
            //PLog.Info("{0} ADDING: {1}", this, c);
            return CurrentTab.Add(c);
        }
        public override uiControl Add(string name, uiControl c) {
            if (CurrentTab == null) { throw new ArgumentNullException(this.ToString() + ": CurrentTab is NULL!"); }
            //PLog.Info("{0} ADDING: ({2}){1}", this, c, name);
            return CurrentTab.Add(name, c);
        }

        public override bool Remove(uiControl c) { if (CurrentTab == null) { throw new ArgumentNullException(this.ToString() + ": CurrentTab is NULL!"); } return CurrentTab.Remove(c); }
        public override bool Remove(string name) { if (CurrentTab == null) { throw new ArgumentNullException(this.ToString() + ": CurrentTab is NULL!"); } return CurrentTab.Remove(name); }
        #endregion
        

        protected override Vector2 Get_Autosize(Vector2? starting_size = null)
        {
            return base.Get_Autosize(starting_size);
        }

        protected override void Display()
        {
            if (CONFIRM_DRAW) { SLog.Info("[{0}](" + Typename + ") Display confirm  | {1}", this, inner_area); }
            Display_BG();// Draw Background
            GUI.BeginClip(inner_area);

            for (int i = 0; i < children.Count; i++)
            {
                if (CONFIRM_DRAW) SLog.Info("  - Drawing child: {0} {1}", children[i], children[i].Get_Status_String());
                children[i].TryDisplay();
            }
            if (CONFIRM_DRAW) SLog.Info(" - - - - - -");

            GUI.EndClip();
        }


    }


    public class uiTab : uiScrollPanel
    {
        public uiTab() : base() { Autosize_Method = AutosizeMethod.BLOCK; }
        public uiTab(uiControlType type) : base(type) { Autosize_Method = AutosizeMethod.BLOCK; }

        /// <summary>
        /// Sets this tab as the active one for it's parent uiTabbedPanel control.
        /// </summary>
        public void Select() { if (isChild) { (parent as uiTabPanel).Set_Tab(this); } }
    }

}
