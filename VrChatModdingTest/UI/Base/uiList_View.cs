using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class uiListView : uiScrollPanel
    {
        private uiControl current_selection = null;

        public uiListView() : base() { init(); }
        public uiListView(uiControlType type) : base(type) { init(); }

        private void init()
        {
            onChildAdded += UiListView_onChildAdded;
            Scrollable = true;
            Autosize_Method = AutosizeMethod.NONE;
            Layout = new Layout_Default();
            Set_Margin(1);
        }

        private void UiListView_onChildAdded(uiControl child)
        {
            child.onSelected += (uiControl c) => {
                change_selection(c);
            };
        }

        private void change_selection(uiControl select)
        {
            if (current_selection != select && current_selection != null) current_selection.Active = false;
            current_selection = select;
            if (select != null) select.Active = true;
        }
    }
}
