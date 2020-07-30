using System;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// A simple wrapper panel with the ability to be collapsed and expanded.
    /// </summary>
    public class uiCollapser : uiWrapperPanel, ICollapsable
    {
        #region State Sizes (Collapsed/Expanded)

        protected float? size_width_expanded = null;
        public float? Size_Width_Expanded { get { return size_width_expanded; } set { size_width_expanded = value; resize(); } }

        protected float? size_height_expanded = null;
        public float? Size_Height_Expanded { get { return size_height_expanded; } set { size_height_expanded = value; resize(); } }

        protected float? size_width_collapsed = null;
        public float? Size_Width_Collapsed { get { return size_width_collapsed; } set { size_width_collapsed = value; resize(); } }

        protected float? size_height_collapsed = null;
        public float? Size_Height_Collapsed { get { return size_height_collapsed; } set { size_height_collapsed = value; resize(); } }
        #endregion

        public event Action<uiCollapser> onCollapsed, onExpanded;

        protected bool collapsed = false;
        public bool isCollapsed { get { return collapsed; } }

        public override bool isVisible { get { return (!isCollapsed && base.isVisible); }  set { base.isVisible = value; } }


        public uiCollapser() : base() { init();  }
        public uiCollapser(uiControlType type) : base(type) { init(); }

        private void init()
        {
            Clickable = true;
        }


        /// <summary>
        /// Sets the collapsed state to <c>FALSE</c>.
        /// </summary>
        public bool Expand()
        {
            return Set_Collapsed(false);
        }

        /// <summary>
        /// Sets the collapsed state to <c>TRUE</c>.
        /// </summary>
        public bool Collapse()
        {
            return Set_Collapsed(true);
        }

        /// <summary>
        /// Sets the collapse state to the specified value.
        /// </summary>
        public bool Set_Collapsed(bool state)
        {
            bool changed = (state != collapsed);
            collapsed = state;
            resize();
            if(changed)
            {
                if (collapsed == true) onCollapsed?.Invoke(this);
                else onExpanded?.Invoke(this);
            }
            return collapsed;
        }

        /// <summary>
        /// Reapplies the current sizing logic for the control's current collapsed/expanded state
        /// </summary>
        protected void resize()
        {
            set_area_dirty();
            if (collapsed)
            {
                // Disable auto sizing and either size the control down to 0 or whatever collapsing size vlaue was given.
                _autosizing_supported = false;
                Unset_Size();

                if (size_width_collapsed.HasValue) Set_Width(size_width_collapsed.Value);
                else Set_Width(0);

                if (size_height_collapsed.HasValue) Set_Height(size_height_collapsed.Value);
                else Set_Height(0);
            }
            else
            {
                // Enable auto sizing again and size the control back to it's set uncollapsed size if specified.
                _autosizing_supported = true;
                Unset_Size();

                if (size_width_expanded.HasValue) Set_Width(size_width_expanded.Value);
                if (size_height_expanded.HasValue) Set_Height(size_height_expanded.Value);
            }
        }
    }
}
