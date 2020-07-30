
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// An implementation of a Parenting control which, by default, autosizes to fit it's children.
    /// </summary>
    public class uiPanel : uiControl
    {
        #region Variables

        private List<uiControl> _children = new List<uiControl>();
        protected virtual List<uiControl> children { get { return _children; } }
        protected Dictionary<string, uiControl> child_map = new Dictionary<string, uiControl>();
        #endregion

        #region Accessors

        public override bool isEmpty { get { return (children.Count <= 0); } }
        public virtual uiControl this[string key] { get { uiControl c; if (child_map.TryGetValue(key, out c)) { return c; } return null; } }
        #endregion

        #region Scrollbar

        public bool Scrollable = false;
        protected override bool hasScrollbar { get { if (!Scrollable) { return false; } return (content_area.height > inner_area.height); } }
        protected Vector2 _scroll_pos = Vector2.zero;
        public virtual Vector2 ScrollPos { get { return _scroll_pos; } set { _scroll_pos = value; update_area(); } }
        public Vector2 Get_ScrollPos() { return ScrollPos; }
        #endregion

        public event Action<uiControl> onChildAdded;
        /// <summary>
        /// Panels are one type of parenting control which can have a layout director assigned to them.
        /// A layout director manages positioning all of the child controls in a predetermined fashion.
        /// EG: icon list, cascading list, etc.
        /// </summary>
        public ILayoutDirector Layout { get { return layout; } set { layout = value; update_area(); } }
        private ILayoutDirector layout = null;

        #region Constructors
        protected uiPanel() : base(uiControlType.Panel) { init(); }
        protected uiPanel(uiControlType ty) : base(ty) { init(); }

        private void init()
        {
            Clickable = true;
            Autosize_Method = AutosizeMethod.GROW;
        }
        #endregion

        /// <summary>
        /// Autosizing means that a control's size is dependant on it's contents. however; a panels "contents" are actually all of the controls it contains, so we need to adjust it's autosizing function here.
        /// </summary>
        protected override Vector2 Get_Autosize(Vector2? starting_size = null)
        {
            /*
            Vector2 sz = content_size_to_inner(content_area.size);
            return base.Get_Autosize(sz);
            */
            return base.Get_Autosize(content_area.size);
        }

        #region Child Management

        public List<uiControl> Get_Children() { return children; }
        public virtual void Set_Child_Index(uiControl child, int i)
        {
            int cidx = children.IndexOf(child);
            if(cidx > -1)
            {
                if (i < 0) i += cidx;
                if(children.Remove(child)) children.Insert(i, child);
            }
        }

        public virtual void Clear_Children()
        {
            foreach (uiControl c in children) { GameObject.Destroy(c); }
            children.Clear();
            child_map.Clear();
            update_area();
        }

        public virtual uiControl Add(uiControl c)
        {
            Add_Control(c);
            return c;
        }

        public virtual uiControl Add(string name, uiControl c)
        {
            Add(c);
            uiControl tmp;
            if (child_map.TryGetValue(name, out tmp))
            {
                if (tmp != c) SLog.Info("[Plugin UI] Warning: assigning new control to a name that is already taken: {0}", name);
            }

            child_map[name] = c;
            c.Name = name;
            return c;
        }

        public virtual bool Remove(uiControl c)
        {
            return Remove_Control(c);
        }

        public virtual bool Remove(string name)
        {
            uiControl c;
            if (child_map.TryGetValue(name, out c))
            {
                child_map.Remove(name);
                return Remove_Control(c);
            }

            return false;
        }

        /// <summary>
        /// Collapses any children that are able
        /// </summary>
        public virtual void Collapse_All()
        {
            foreach (uiControl c in children)
            {
                if (c is ICollapsable) (c as ICollapsable).Collapse();
            }
        }

        /// <summary>
        /// Expands any children that are able
        /// </summary>
        public virtual void Expand_All()
        {
            foreach (uiControl c in children)
            {
                if (c is ICollapsable) (c as ICollapsable).Expand();
            }
        }
        #endregion

        #region Base child-control management (cannot be overriden, because it should already handle anything that could be needed)

        protected void Add_Control(uiControl c)
        {
            if (c == null) return;
            if (children.Contains(c)) return;

            set_layout_dirty();
            children.Add(c);

            c.Set_Parent(this);
            if (c.gameObject != null && this.gameObject != null)
            {
                c.gameObject.transform.SetParent(base.gameObject.transform, false);
                //c.rect.SetParent(base.gameObject.transform, false);
            }

            onChildAdded?.Invoke(c);
            update_area();
        }

        protected bool Remove_Control(uiControl c)
        {
            set_layout_dirty();
            return children.Remove(c);
        }
        #endregion

        #region Misc

        /// <summary>
        /// Child controls can use this function to query what kind of size constraints the parent will impose on them.
        /// </summary>
        internal virtual Vector2 Constrain_Child_Size(uiControl c, Vector2 requested_size)
        {
            Vector2 max = available_area;// This is the maximum size available to this control
            // We want to determine if the calling control's maximum extents would go outside the maximum possible extents of this control.
            // And if so, scale back the requested size so it fits within our limits.
            if (isSizeConstrained) max.x = Get_Content_Area().width;

            if (CONFIRM_SIZE) SLog.Info("{0}  Confirm Child Size Constraint  |  Available Area: {1}  |  Requested Size: {2}  ", this, max, requested_size);

            if ((requested_size.x + c.Pos.x) > max.x) requested_size.x = (max.x - c.Pos.x);
            if ((requested_size.y + c.Pos.y) > max.y) requested_size.y = (max.y - c.Pos.y);

            return requested_size;
        }

        public virtual bool withinChild(Vector2 p)
        {
            foreach (var child in children)
            {
                if (child.Area.Contains(p)) return true;
            }

            return false;
        }
        #endregion

        #region Parent to Child Events

        internal virtual void handle_enabled_change() { if (isFirstFrame) { return; } foreach (var child in children) { child.parent_enable_updated(); } }

        internal virtual void handle_visibility_change() { if (isFirstFrame) { return; } foreach (var child in children) { child.parent_visibility_updated(); } }
        #endregion

        #region Event Management

        protected override void doLayout_Post()
        {
            // Now that the control has positioned all of it's children within itself we need to calculate the total area they occupy!
            float xMax = 0f, yMax = 0f;
            bool hadScrollbar = hasScrollbar;

            foreach (uiControl child in children)
            {
                // We need to account for the padding that is added to the inner_area when using align- functions on child controls as it affects the controls positioning and would push its maximum extents beyond those of the content area even though the control doesn't actually go OOB!
                //float cmX = (child.area.xMax - Padding.horizontal);
                //float cmY = (child.area.yMax - Padding.vertical);

                float cmX = child.Area.xMax;
                float cmY = child.Area.yMax;
                //if (CONFIRM_LAYOUT && cmY > _content_area.yMax) PLog.Info("{0}  Child Y OOB  |  yMax: {1}  |  area: {2}  |  child.yMax: {3}", this, _content_area.yMax, area, cmY);
                xMax = Math.Max(xMax, cmX);
                yMax = Math.Max(yMax, cmY);
            }

            //Rect last = new Rect(_content_area);
            Rect last = _content_area;
            _content_area = new Rect(0f, 0f, xMax, yMax);


            if (CONFIRM_LAYOUT) SLog.Info("{0}  Confirmed Layout", this);
            if (!last.Compare(_content_area) || hasScrollbar != hadScrollbar)
            {
                set_area_dirty();
                if (CONFIRM_LAYOUT) SLog.Info("{0}  Confirmed Layout Changed Area  |  new_content_area: {1}  |  last_content_area: {2}", this, _content_area, last);
            }
        }

        protected override void post_update_area()
        {
            foreach (uiControl child in children)
            {
                child.parent_area_updated();
            }
        }

        protected void propagateEventToChildren()
        {
            Event e = Event.current;
            // Decided we need to pass all events to the childrens

            //EventType ty = Event.current.GetTypeForControl(this.id);
            //if (ty == EventType.Layout || e.isMouse)
            //{
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null) continue;
                children[i].TryHandleEvent();
            }
            //}
        }

        public override void handleEvent_Base()
        {
            // Since this control has children that *MUST* get control of any & all event's it receives before it does we need to propagate each event to all of the children FIRST!
            propagateEventToChildren();
            base.handleEvent_Base();
        }
        #endregion


        public override void doLayout()
        {
            base.doLayout();
            if(layout != null && children!=null && children.Count>0) layout.Handle(this, children.ToArray());
        }
        
        protected override void Display()
        {
            if (CONFIRM_DRAW) { SLog.Info("[{0}](" + Typename + ") Display confirm  | {1}", this, inner_area); }
            Display_BG();// Draw Background
            // Draw children within scrollable area
            Vector2 pre_scroll = _scroll_pos;//track what the scroll pos was before this frame

            // We still must implement handling for if the control has Scrollable turned off, just in case!
            if (Scrollable)
                _scroll_pos = GUI.BeginScrollView(inner_area, _scroll_pos, _content_area);
            else
                GUI.BeginClip(inner_area);

            for (int i = 0; i < children.Count; i++)
            {
                if (CONFIRM_DRAW) SLog.Info("  - Drawing child: {0} {1}", children[i], children[i].Get_Status_String());
                children[i].TryDisplay();
            }
            if (CONFIRM_DRAW) SLog.Info(" - - - - - -");

            if (Scrollable)
            {
                GUI.EndScrollView(true);
                if (!Util.floatEq(_scroll_pos.x, pre_scroll.x) || !Util.floatEq(_scroll_pos.y, pre_scroll.y))
                {
                    if (CONFIRM_SCROLL) SLog.Info("{0}  Confirm Scroll  |  Scroll: {1}  |  pre_scroll: {2}", this, _scroll_pos, pre_scroll);
                    foreach (var child in children)
                    {
                        child.parent_scroll_updated();
                    }
                }
            }
            else
                GUI.EndClip();
        }

    }
}
