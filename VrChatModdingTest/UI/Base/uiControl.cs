using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    // TODO: uiTextArea seems to ignore it's padding when rendering text.

    public delegate void controlEvent<T>(T c) where T : uiControl;
    public delegate void parentEvent(uiPanel c);

    public abstract partial class uiControl : SRBehaviour
    {
        #region Confirmation Settings
        // These are all flags that can be set to True to enable log messages which will tell us which areas of a specific uiControl are firing properly!
        internal bool CONFIRM_DRAW = false;
        internal bool CONFIRM_AREA = false;
        internal bool CONFIRM_EVENTS = false;
        internal bool CONFIRM_POS = false;
        internal bool CONFIRM_SIZE = false;
        internal bool CONFIRM_LAYOUT = false;
        internal bool CONFIRM_MARGIN = false;
        internal bool CONFIRM_BORDER = false;
        internal bool CONFIRM_PADDING = false;
        internal bool CONFIRM_SCROLL = false;
        internal bool CONFIRM_POSITIONER = false;

        internal Vector2? _confirm_autosize_last = null;
        #endregion

        #region Debug Variables
        /// <summary>
        /// A reference to the last control that consumed the mouse click event.
        /// </summary>
        internal static uiControl debug_last_consumer_click = null;
        /// <summary>
        /// A reference to the control which the mouse is currently overtop.
        /// </summary>
        internal static uiControl debug_current_mouse_over = null;
        #endregion

        #region CLASS VARS
        internal static uiDebugDrawMode DEBUG_DRAW_MODE = uiDebugDrawMode.NONE;// If > 0 then we are drawing debug stuff for all uiControls, specific numeric values will determine WHAT debug info is drawn.
        internal static int DEBUG_DRAW_MODE_MAX = Enum.GetValues(typeof(uiDebugDrawMode)).Length;
        public static Dictionary<int, uiControl> ALL = new Dictionary<int, uiControl>();
        private static int lastUID = 0;
        /// <summary>
        /// The Unity GUISkin that holds styling data for all of our control types
        /// </summary>
        protected static readonly Dictionary<uiSkinPreset, GUISkin> SKINS = null;
        #endregion

        #region VARIABLES
        private GUIContent _content = new GUIContent();
        protected virtual GUIContent content { get { return _content; } set { _content = value; } }

        /// <summary>
        /// The area our content currently takes up.
        /// </summary>
        public virtual Rect content_area { get { return _content_area; } }
        protected Rect _content_area = new Rect();

        #region Identifier Variables

        /// <summary>
        /// The id number of this control within our 'ALL' Dictionary.
        /// </summary>
        internal int ID { get; private set; }

        /// <summary>
        /// The assigned Unity GUI control id.
        /// </summary>
        public int unity_id { get { if (_unity_id <= 0) { this._unity_id = GUIUtility.GetControlID(this.FocusType); } return _unity_id; } }
        private int _unity_id = 0;

        private string _name = null;
        /// <summary>
        /// The name of this control or it's typename if no name was assigned.
        /// </summary>
        public string Name { get { return _name == null ? Typename : _name; } set { _name = value; } }
        /// <summary>
        /// The name of this control and all the parenting controls above it as well. Effectively the pathname to this control within the hierarchy
        /// </summary>
        public string FullName { get { return isChild ? String.Concat(parent.FullName, "/", Name) : Name; } }
        #endregion

        #region Control Type variables
        /// <summary>
        /// What kind of control we are.
        /// </summary>
        public readonly uiControlType Kind;
        protected string _typename = null;
        public string Typename { get { if (_typename == null) { return this.GetType().Name; } return _typename; } }
        protected FocusType FocusType = FocusType.Passive;// Almost all controls will be using this FocusType, the only different ones will be ones that need keyboard input.

        /// <summary>
        /// Only clickable controls can fire clicked events.
        /// </summary>
        public bool Clickable = false;
        /// <summary>
        /// Only controls where isClickable returns true will perform handling for mouse events
        /// </summary>
        protected bool isClickable { get { return (Clickable || onClicked != null); } }
        //protected bool isClickable { get { return ((type == uiControlType.Button || type == uiControlType.Checkbox || type == uiControlType.Textbox || type == uiControlType.TextArea || type == uiControlType.Panel || type == uiControlType.Panel_Dark || type == uiControlType.Window) || onClicked != null); } }
        /// <summary>
        /// Can the user click and drag this control?
        /// </summary>
        protected bool isDraggable = false;// XXX: THIS SHOULD BE A GETTER THAT CHECKS IF 'DragArea' HAS A VALUE!

        /// <summary>
        /// NOT YET IMPLEMENTED!
        /// The relative area within this control where the user can click to begin dragging it.
        /// </summary>
        protected virtual Rect? DragArea { get { return null; } }
        #endregion

        #region State-Tracking variables

        /// <summary>
        /// On the very first frame of the controls life it will force an area update on itself so all controls can start with a valid area even if they are born with <see cref="isVisible"/> set to FALSE.
        /// This variable tracks the status of said process.
        /// </summary>
        private bool _first_frame = true;
        protected bool isFirstFrame { get { return _first_frame; } }

        /// <summary>
        /// Is this control in an activated / selected state?
        /// </summary>
        private bool _active = false;
        /// <summary>
        /// Can this control be interacted with?
        /// </summary>
        private bool _disabled = false;
        private bool? _parent_disabled = null;

        /// <summary>
        /// Can this control be interacted with?
        /// </summary>
        public bool isDisabled { get { if (isChild) { if (!_parent_disabled.HasValue) { _parent_disabled = parent.isDisabled; } return (_parent_disabled.Value || _disabled); }  return _disabled; } set { _disabled = value; if (isParent) { ((uiPanel)this).handle_enabled_change(); } update_area(); } }

        /// <summary>
        /// Is this control in an activated / selected state?
        /// </summary>
        public bool Active { get { return _active; } set { bool was_active = _active; _active = value; update_area(); if (_active != was_active && _active) { onSelected?.Invoke(this); } } }

        // This whole "Selectable" business is normally for things that go into ListView type controls where the user makes selections from the list
        public bool Selected { get { return Active; } set { Active = value; } }
        public bool Selectable { get { return _selectable; } set { _selectable = value; if (!value) { Selected = false; } } }
        protected bool _selectable = false;

        /// <summary>
        /// Should this control take up space and be visible? This property ties itself to the <see cref="isVisible"/> property and will "bump" it's value to cause it to reevaluate itself whenever this property changes.
        /// </summary>
        public virtual bool isDisplayed {
            get
            {
                /* if (isChild) {
                    if (!_parent_displayed.HasValue) _parent_displayed = parent.isDisplayed;
                    return (_parent_displayed.Value && _displayed);
                } */
                return _displayed;
            }
            set {
                _displayed = value;
                //isVisible = _visible;// We don;t need this, the isVisible function already checks US instead.(each frame)
            }
        }
        private bool _displayed = true;
        private bool? _parent_displayed = null;

        /// <summary>
        /// Should this control be rendered? (Note: the control will still take up space even if this is false, to cause a control to no longer occupy an area set <see cref="isDisplayed"/> to FALSE)
        /// </summary>
        public virtual bool isVisible {
            get {
                if (!isDisplayed) return false;// if we are not currently displayed, we are not visible either.
                if (isChild) {
                    if (!_parent_visible.HasValue) _parent_visible = parent.isVisible;
                    return (_parent_visible.Value && _visible);
                }
                return _visible;
            }
            set {
                _visible = value;
                //PLog.Info("{0}  _visible: {1}  isVisible: {2}", this, _visible, isVisible);
                if (isParent) {
                    ((uiPanel)this).handle_visibility_change();
                    if(_visible == true) set_layout_dirty();// If we are now visible then we should re-layout all of our children.
                }
                update_area();
            }
        }
        private bool _visible = true;
        private bool? _parent_visible = null;
        private bool _was_visible = true;// was this control visible last frame?

        public virtual string Get_Status_String() { return String.Format("Visible: {0}  Disabled: {1}", isVisible, isDisabled); }
        #endregion

        #region Skin

        protected uiSkinPreset skin = uiSkinPreset.DEFAULT;
        public uiSkinPreset Skin { get { return skin; } set { skin = value; update_style(); update_area(); } }
        public GUISkin mySkin { get { if (!SKINS.ContainsKey(skin)) { throw new KeyNotFoundException(String.Format("No skin created for type: ", Enum.GetName(typeof(uiSkinPreset), skin))); } return SKINS[skin]; } }
        #endregion

        #region Style

        public Color? Tint = null;
        protected GUIStyle _local_style = null;
        protected GUIStyle _style_text = null;
        protected GUIStyle _style_combo = null;

        /// <summary>
        /// If <c>TRUE</c> then on the next frame the local style will be checked for changes.
        /// </summary>
        private bool dirty_style = false;
        /// <summary>
        /// Disables drawing the controls background
        /// </summary>
        public bool disableBG = false;
        
        /// <summary>
        /// Readonly reference to the GUIStyle this particular control instance should be using to draw itself.
        /// </summary>
        public GUIStyle Style {
            get
            {
                if (inherits_style && isChild) { return styleCombo; }
                else if (inherits_text_style && isChild) { return styleText; }
                return (_local_style != null ? _local_style : get_skin_style_for_type(Kind));
            }
        }

        /// <summary>
        /// Returns the current <c>GUIStyleState</c> the control should be using.
        /// </summary>
        public GUIStyleState StyleState {
            get
            {
                if (isHovered) return Style.hover;
                else if (isActive) return Style.active;
                else if (isFocused) return Style.focused;
                return Style.normal;
            }
        }

        public GUIStyle defaultStyle { get { return get_skin_style_for_type(Kind); } }
        public GUIStyle local_style { get { if (_local_style == null) { _local_style = new GUIStyle(Style); update_area(); } set_style_dirty(); return _local_style; } set { _local_style = value; _style_text = null; _style_combo = null; } }
        public GUIStyle styleText { get { if (_style_text == null) { styleText = null; } return _style_text; }
            set
            {
                if (inherits_text_style && isChild) _style_text = new GUIStyle(parent.styleText);
                else _style_text = new GUIStyle(_local_style != null ? _local_style : get_skin_style_for_type(Kind));

                _style_text.normal.background = null;
                _style_text.active.background = null;
                _style_text.hover.background = null;
                _style_text.focused.background = null;

                _style_text.wordWrap = defaultStyle.wordWrap;
                set_area_dirty();
            }
        }
        /// <summary>
        /// inherits our parents background style
        /// </summary>
        public GUIStyle styleCombo
        {
            get { if (_style_combo == null) { styleCombo = null; } return _style_combo; }
            set
            {
                //_style_combo = new GUIStyle(parent.Style);
                if (inherits_text_style && isChild) _style_combo = new GUIStyle(styleText);
                else _style_combo = new GUIStyle(_local_style != null ? _local_style : get_skin_style_for_type(Kind));

                _style_combo.normal.background = parent.Style.normal.background;
                _style_combo.active.background = parent.Style.normal.background;
                _style_combo.hover.background = parent.Style.normal.background;
                _style_combo.focused.background = parent.Style.normal.background;

                set_area_dirty();
            }
        }

        public uiBorderStyle Border = new uiBorderStyle();
        /// <summary>
        /// Returns the border style that should currently be used.
        /// Used by the internal drawing functions.
        /// </summary>
        private uiBorderStyleState borderStyle
        {
            get
            {
                Border._cached.reset();
                if (Border.type != uiBorderType.NONE)
                {
                    Border._cached.take(Border.normal);
                    if (isHovered) Border._cached.take(Border.hover);
                    if (isActive) Border._cached.take(Border.active);
                    if (isFocused) Border._cached.take(Border.focused);
                }
                return Border._cached;
            }
        }
        private uiBorderStyleState cached_borderStyle = null;

        /// <summary>
        /// Immediately copies a specified controls GUI-Styling settings.
        /// </summary>
        public void Clone_Style(uiControl c) { local_style = new GUIStyle(c.Style); }
        /// <summary>
        /// Immediately copies a specified controls GUI-Styling settings and clears the background colors/images from it.
        /// </summary>
        public void Clone_Text_Style(uiControl c) { local_style = new GUIStyle(c.styleText); }
        #endregion

        #region Text Settings

        /// <summary>
        /// The tooltip text for this control.
        /// </summary>
        public string Tooltip = null;

        public virtual string Text { get { return _content.text; } set { _content.text = value; update_area(); update_style(); } }
        public virtual int TextSize { get { return Style.fontSize; } set { local_style.fontSize = value; update_area(); update_style(); } }
        public virtual FontStyle TextStyle { get { return Style.fontStyle; } set { local_style.fontStyle = value; update_area(); update_style(); } }
        public virtual TextAnchor TextAlign { get { return Style.alignment; } set { local_style.alignment = value; update_area(); update_style(); } }
        public virtual bool AllowOverflow { get { return (Style.clipping == TextClipping.Overflow); } set { local_style.clipping = (value ? TextClipping.Overflow : TextClipping.Clip); update_area(); update_style(); } }

        public virtual Color TextColor { get { return Style.normal.textColor; } set { local_style.normal.textColor = value; update_area(); update_style(); } }
        public virtual Color TextColor_Hover { get { return Style.hover.textColor; } set { local_style.hover.textColor = value; update_area(); update_style(); } }
        #endregion

        #region Style-State-Determining flags

        protected bool isMouseDown = false;
        protected bool isMouseOver = false;

        /// <summary>
        /// Focused means the control is receiving user keyboard input
        /// </summary>
        protected bool isFocused { get { return (GUIUtility.hotControl == this.unity_id || GUIUtility.keyboardControl == this.unity_id); } }
        /// <summary>
        /// Hovered means that the mouse is overtop the control and it will use it's 'hover' <see cref="GUIStyleState"/> to draw with.
        /// </summary>
        protected virtual bool isHovered { get { return (isMouseOver || _active); } }
        /// <summary>
        /// The meaning of 'Active' differs with each control type but means something similar to being toggled 'on' or being the active selection in a list.
        /// Eg: Visually Activated
        /// </summary>
        protected virtual bool isActive { get { return _active; } }
        /// <summary>
        /// Depressed, here, means that the control is in a visually active/toggled/inset state.
        /// </summary>
        protected virtual bool isDepressed { get { return ((isMouseDown && isClickable) || _active); } }
        protected virtual bool hasScrollbar { get { return false; } }
        #endregion

        #region Autosize variables

        /// <summary>
        /// Specifies how to calculate the controls size.
        /// </summary>
        public AutosizeMethod Autosize_Method = AutosizeMethod.NONE;
        /// <summary>
        /// If <c>True</c> the control will autosize itself so it fit's to it's contents, each control type can have custom autosizing logic, the user can use this field to force any control to attempt autosizing.
        /// Most controls will autosize by default.
        /// The only way to prevent a control autosizing its width or height respectively is to explicitly give them a value. Until the width/height is given an explicit value they will each be autosized when autosizing is enabled.
        /// However; Autosizing can be completely disabled for a control by setting this property to <c>FALSE</c>.
        /// </summary>
        public bool Autosize { get { return (_autosize && _autosizing_supported); } set { _autosize = value; update_area(); } }
        private bool _autosize = true;//this is the value which is explicitly set by the user, if the control should defy it's default nature and always stick to autosizing then the user indirectly set's this to TRUE.
        /// <summary>
        /// This is set by a control internally to determine if it SHOULD support autosizing.
        /// EG: Used by collapsable controls to ensure they maintain a zero size when collapsed.
        /// </summary>
        protected bool _autosizing_supported = true;
        /// <summary>
        /// Is this controls size limited by it's parent's OR does this control push the parent so it grows in size?
        /// </summary>
        public bool isSizeConstrained { get { return (Autosize && !(Autosize_Method == AutosizeMethod.GROW )); } }
        /// <summary>
        /// Returns the controls size with autosizing calculated for width or height or both depending on which of them should affect the controls size.
        /// </summary>
        protected Vector2 size_auto { get { if (has_explicit_W && has_explicit_H) { return Area.size; } var sz = Get_Autosize(); if (has_explicit_W) { sz.x = Area.width; } if (has_explicit_H) { sz.y = Area.height; } return sz; } }
        //protected Vector2 size_auto { get { if (has_explicit_W && has_explicit_H) { return set_area.size; } var sz = Get_Autosize(); if (has_explicit_W) { sz.x = set_area.width; } if (has_explicit_H) { sz.y = set_area.height; } return sz; } }

        private bool has_explicit_W = false;// Tracks whether the control has had an explicit width set
        private bool has_explicit_H = false;// Tracks whether the control has had an explicit height set

        protected bool Explicit_W { get { return has_explicit_W; } }
        protected bool Explicit_H { get { return has_explicit_H; } }

        #endregion

        #region Event variables

        public event controlEvent<uiControl> onClicked;
        public event controlEvent<uiControl> onSelected;
        /// <summary>
        /// Fires whenever this controls isVisible value changes to True
        /// </summary>
        public event controlEvent<uiControl> onShown;
        /// <summary>
        /// Fires whenever this controls isVisible value changes to False
        /// </summary>
        public event controlEvent<uiControl> onHidden;
        public event parentEvent onLayout;// Fired by parent controls so child controls can do their positioning logic.
        /// <summary>
        /// Fires each frame, used to process custom logic for controls.
        /// </summary>
        public event Action onThink;
        #endregion

        #region Area
        
        /// <summary>
        /// Allows us to ignore area updates while true, so that child updates dont refire the whole process all over again!
        /// </summary>
        private bool lock_area_update = true;// Prevent area updates until the first-frame fires because modifying almost ANY property of a control will trigger an area update and we don't want to waste time during the initial control creation and setup phases.
        protected bool dirty_layout = true;
        protected bool dirty_area = false;
        private Rect? _last_area_value = null;
        private Rect? _last_cached_area = null;
        private Rect? _last_inner_area = null;


        /// <summary>
        /// The area this control will draw it's contents within
        /// this field is overrideable so that controls derived from the uiWindow class can position their components using the inner_area reference without said reference also encompasing the uiWindow's titlebar which screws up positioning...
        /// </summary>
        protected virtual Rect inner_area { get { return _inner_area; } }
        protected Rect _inner_area = new Rect();
        
        /// <summary>
        /// The area this control will draw its background within
        /// </summary>
        protected Rect draw_area = new Rect();

        /// <summary>
        /// The area this control will draw its border within
        /// </summary>
        protected Rect border_area = new Rect();

        /// <summary>
        /// The control's visible area
        /// (Note: when drawing the control use "draw_area" as the intended area is altered by padding and margin values)
        /// </summary>
        public Rect Area { get { if (area.HasValue) { return area.Value; } return set_area; } set { area = null; set_area = value; update_area(); } }


        private Rect? area = null;// This is the "Area" propertys proxy value.
        /// <summary>
        /// This is the field that stores the position this control was SET to occupy and the size that was SET explicitly for it
        /// </summary>
        private Rect set_area = new Rect(0, 0, 0, 0);

        /// <summary>
        /// The absolute area this control occupies on screen.
        /// </summary>
        public Rect absArea { get { if (!_absArea.HasValue) { _absArea = new Rect(Area.position + parentPosInner - parentScroll, Area.size); } return _absArea.Value; } set { _absArea = null; } }
        private Rect? _absArea = null;

        /// <summary>
        /// The absolute area where this control will render it's content, including child controls.
        /// </summary>
        public Rect absInnerArea { get { if (!_absInnerArea.HasValue) { _absInnerArea = new Rect(_inner_area.position + parentPosInner - parentScroll, inner_area.size); } return _absInnerArea.Value; }  set { _absInnerArea = null; } }
        private Rect? _absInnerArea;
        #endregion

        #region Position
        /// <summary>
        /// The absolute position where this control will render it's content, including child controls.
        /// </summary>
        public Vector2 innerPos { get { return inner_area.position; } }

        /// <summary>
        /// The absolute position where this control will appear on screen.
        /// </summary>
        public Vector2 absPos { get { return absArea.position; } }

        /// <summary>
        /// The (relative) visible position of this control. Shortcut for <see cref="Area.position"/>
        /// </summary>
        public Vector2 Pos { get { return Area.position; } }

        /// <summary>
        /// The position of this control as set by the user, or the auto positioning system.
        /// </summary>
        public virtual Vector2 _pos { get { return set_area.position; } }

        private ControlPositioner vertical_positioner = null;
        private ControlPositioner horizontal_positioner = null;
        #endregion

        #region Size
        
        private uiSizeConstraint _sizeConstraint = uiSizeConstraint.NONE;
        public uiSizeConstraint SizeConstraint { get { return _sizeConstraint; } set { _sizeConstraint = value; update_area(); } }
        /// <summary>
        /// The unconstrained size which this control should currently be BEFORE adding margin, padding, or border offsets.
        /// When the controls Area is updated it's size will be set to this value and constrained to be within the controls set min/max size values.
        /// </summary>
        protected virtual Vector2 size { get { if (!isDisplayed) { return Vector2.zero; } if (Autosize) { return size_auto; } return set_area.size; } }
        //protected virtual Vector2 size { get { if (!isDisplayed) { return Vector2.zero; } if (Autosize) { return size_auto; } if (area.HasValue) { return area.Value.size; } return set_area.size; } }

        /// <summary>
        /// The base size of this control without padding, border, or margins accounted for.
        /// </summary>
        public Vector2 _size { get { return set_area.size; } set { set_area.size = value; update_area(); } }

        /// <summary>
        /// Minimum Width this control can be.
        /// </summary>
        public float? Min_Width = null;
        /// <summary>
        /// Minimum Height this control can be.
        /// </summary>
        public float? Min_Height = null;


        /// <summary>
        /// Maximum Width this control can be.
        /// </summary>
        public float? Max_Width = null;
        /// <summary>
        /// Maximum Height this control can be.
        /// </summary>
        public float? Max_Height = null;

        /// <summary>
        /// Handles adjustments to the controls width if it's size was assigned a dynamic value, <see cref="FloodX(float)"/>
        /// </summary>
        private ControlSizer width_sizer = null;
        /// <summary>
        /// Handles adjustments to the controls height if it's size was assigned a dynamic value, <see cref="FloodY(float)"/>
        /// </summary>
        private ControlSizer height_sizer = null;
        #endregion

        #region Offsets: margins & paddings

        private RectOffset _margin = new RectOffset(0, 0, 0, 0);
        public RectOffset Margin { get { return _margin; } set { _margin = value; update_area(); } }

        public void Set_Margin(int all) { Margin = new RectOffset(all, all, all, all); }
        public void Set_Margin(int x, int y) { Margin = new RectOffset(x, x, y, y); }
        public void Set_Margin(int left, int right, int top, int bottom) { Margin = new RectOffset(left, right, top, bottom); }

        private RectOffset _padding = new RectOffset(0, 0, 0, 0);
        public RectOffset Padding { get { return _padding; } set { _padding = value; update_area(); } }

        public void Set_Padding(int all) { Padding = new RectOffset(all, all, all, all); }
        public void Set_Padding(int x, int y) { Padding = new RectOffset(x, x, y, y); }
        public void Set_Padding(int left, int right, int top, int bottom) { Padding = new RectOffset(left, right, top, bottom); }

        protected RectOffset _selfPadding = new RectOffset(0, 0, 0, 0);// Padding that is applied only to autosized controls.
        /// <summary>
        /// To be honest I completely forget why this is even a thing, Perhaps it was a work-around for the older versions of this control system idk. It will likely be removed in the future.
        /// </summary>
        protected RectOffset selfPadding { get { return _selfPadding; } set { _selfPadding = value; update_area(); } }
        #endregion

        #region Parent related variables

        public uiPanel Parent { get { return parent; } }
        protected List<uiPanel> parent_chain = new List<uiPanel>();
        protected uiPanel parent { get { return _parent; } set { if (value != this) _parent = value; else _parent = null; handle_ParentChange(); } }
        private uiPanel _parent = null;
        /// <summary>
        /// Does this control inherit it's style from it's parent control (if any)?
        /// </summary>
        public bool inherits_style = false;
        /// <summary>
        /// Does this control inherit it's text style from it's parent control (if any)?
        /// </summary>
        public bool inherits_text_style = false;

        /// <summary>
        /// Is the control inside another control?
        /// </summary>
        public bool isChild { get { return (parent != null); } }

        /// <summary>
        /// Is this control able to have other controls added to it?
        /// </summary>
        public bool isParent { get { return (this is uiPanel); } }

        /// <summary>
        /// Does the control have anything to display?
        /// Such as: children, text, or images
        /// </summary>
        public virtual bool isEmpty { get { return (content == null || (content.image==null && (content.text==null || content.text.Length <= 0) )); } }
        
        protected Vector2 parentPos { get { return isChild ? parent.absArea.position : Vector2.zero; } }
        protected Vector2 parentPosInner { get { return isChild ? parent.absInnerArea.position : Vector2.zero; } }

        private Vector2? _parentScroll = null;
        protected Vector2 parentScroll { get { if (_parentScroll.HasValue) { return _parentScroll.Value; } Vector2 spos = ((this is uiScrollPanel) ? ((uiScrollPanel)this).ScrollPos : Vector2.zero); foreach(uiPanel p in parent_chain){ spos += p.ScrollPos; }; _parentScroll = spos; return _parentScroll.Value; } }


        private bool dirty_parent_chain = false;
        private bool dirty_parent_scroll = false;
        /// <summary>
        /// The total space within the parent control or screen that this control may potentially occupy.
        /// </summary>
        protected Vector2 available_area { get { return Get_Max_Potential_Size(); } }
        private Vector2? _potential_area_max_cached = null;// Just a cached value from   uiControl.Get_Max_Potential_Size()
        #endregion

        #endregion

        #region Overrides

        public override string ToString() { return String.Format("[<b>{0}</b>]<{1}>({2}) {3}", Typename, ID, FullName, (isChild ? "[Sub]" : "[Root]")); }

        public override int GetHashCode() { return ID; }

        public override bool Equals(object o) { return (ID == ((uiControl)o).ID); }
        #endregion

        #region Constructors

        static uiControl()
        {
            if (SKINS != null) return;
            try
            {
                SKINS = new Dictionary<uiSkinPreset, GUISkin>()
                {
                    { uiSkinPreset.DEFAULT, SkinPresets.Create_Default() },
                    { uiSkinPreset.FLAT, SkinPresets.Create_Flat() },
                };
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
            }
        }

        public uiControl(uiControlType ty = uiControlType.Generic)
        {
            ID = ++lastUID;
            ALL.Add(ID, this);

            Kind = ty;
        }

        protected virtual void OnDestroy()
        {
            ALL.Remove(ID);
        }

        public static T Create<T>(uiPanel parent = null) where T : uiControl { return Create<T>(null, parent); }

        public static T Create<T>(string name, uiPanel parent = null) where T : uiControl
        {
            GameObject obj = new GameObject(typeof(T).Name);
            obj.SetActive(true);
            obj.layer = 5;//GUI layer
            UnityEngine.GameObject.DontDestroyOnLoad(obj);

            T c = obj.AddComponent<T>();
            if (parent != null)
            {
                if (name != null) parent.Add(name, c);
                else parent.Add(c);
            }
            return c;
        }
        #endregion

        #region Accessor Functions

        public void Set_Style(GUIStyle style)
        {
            this.local_style = style;
        }

        internal void Set_Parent(uiControl parent)
        {
            this.parent = (uiPanel)parent;
            this.parent_area_updated();
        }

        /// <summary>
        /// This is the method child controls should use to obtain the area they MAY occupy within their parent control.
        /// </summary>
        public Rect Get_Content_Area()
        {
            if (hasScrollbar)
            {
                RectOffset re = new RectOffset(0, 16, 0, 0);
                return Padding.Remove(re.Remove(inner_area));
                //return re.Remove(inner_area);
            }

            return Padding.Remove(inner_area);
            //return inner_area;
        }
        
        #endregion

        #region Dirty Flags

        /// <summary>
        /// Flags the control so it will update it's area next frame.
        /// </summary>
        public void set_area_dirty() { dirty_area = true; }

        /// <summary>
        /// Flags the control so it will update it's layout next frame.
        /// </summary>
        public void set_layout_dirty() { dirty_layout = true; }

        /// <summary>
        /// Flags the control so it will check it's local_style for changes and update any dependant fields if needed.
        /// </summary>
        public void set_style_dirty() { dirty_style = true; }
        #endregion

        #region Area Updating

        /// <summary>
        /// Call each time ANY factor that determines the controls size is changed.
        /// NOTE: Layout logic CANNOT be performed until *AFTER* the first frame (because the controls children cannot be guaranteed to exist yet and the variables referencing them would be undefined.)
        /// </summary>
        public virtual bool update_area(bool force = false)
        {
            if (!force)
            {
                //if (!isVisible) return false;// We don't update area stuff while invisible, we will do it once isVisible changes to TRUE
                //if (CONFIRM_AREA) PLog.Info("{0}  Area {1} | last_area_value {2}", this.ToString(), _area, _last_area_value.Value);

                if (lock_area_update)
                {
                    if (area.HasValue && !set_area.Compare(_last_area_value.Value))
                    {
                        dirty_area = true;// fire the update_area() function again next frame.
                    }
                    return false;//we are in lock so just stahp
                }
            }

#if DEBUG
            var stack = new StackTrace();
            if (stack.FrameCount >= 100)
            {
                System.IO.File.WriteAllText("stack.txt", stack.ToString());
            }
#endif

            lock_area_update = true;//we set this to true until we are completely done updating the area. This is to prevent any child controls or Auto positioners/sizers from entering us into an infinite update loop.(it happened)
            try
            {
                reset_cached_abs_areas();
                cached_borderStyle = null;// we want the border texture to be recreated when our size changes...
                _potential_area_max_cached = null;// Yeah regen this one too.
                _last_cached_area = null;

                if (area.HasValue) _last_cached_area = area.Value;// Track what the area WAS so we can tell when it updates and only trigger extra logic when it's needed.
                
                Vector2 nowpos = set_area.position;
                Vector2 nowsize = constrain_size(apply_size_constraints(size));

                area = null;// For SOME REASON Rect.Set(x,y, w,h) doesn't seem to actually change it's values. so we have to trash memory a bit and create a new Rect anytime we wanna get something done.
                area = new Rect(nowpos, nowsize);
                // update our visible area
                /*
                if (!area.HasValue) area = new Rect(nowpos, nowsize);
                else area.Value.Set(nowpos.x, nowpos.y, nowsize.x, nowsize.y);
                */

                bool area_updated = false;
                if (force) area_updated = true;

                if (!_last_cached_area.HasValue || !area.Value.Compare(_last_cached_area.Value))
                {
                    area_updated = true;
                    Vector2 old_size = Vector2.zero;
                    if (_last_cached_area.HasValue) old_size = _last_cached_area.Value.size;
                    if (CONFIRM_SIZE && _last_cached_area.HasValue && area.Value.size != old_size) SLog.Info("{0}  Confirm Size  |  Size Changed  |  new size: {1} | old size: {2}", this, area.Value.size, old_size);
                }
                _last_area_value = set_area;
                                
                border_area = _margin.Remove(area.Value);// The area where our BORDERS will be drawn, it is the position and size we were given - our MARGINS.
                draw_area = borderStyle.size.Remove(border_area);// The area where our BACKGROUND will be drawn, it is the position and size we were given - our MARGINS - BORDER size.
                _inner_area = _padding.Remove(draw_area);// The area where our CONTENT will be drawn, it is the position and size we were given - our MARGINS - BORDER size - PADDING.

                if (!_last_inner_area.HasValue || _inner_area.Compare(_last_inner_area.Value))
                {
                    area_updated = true;
                    Vector2 old_size = Vector2.zero;
                    if (_last_inner_area.HasValue) old_size = _last_inner_area.Value.size;
                    if (CONFIRM_SIZE && _last_inner_area.HasValue && _inner_area.size != old_size) SLog.Info("{0}  Confirm Size  |  Inner Size Changed  |  new size(inner): {1} | old size(inner): {2}", this, _inner_area.size, old_size);
                }
                _last_inner_area = _inner_area;

                if (CONFIRM_MARGIN) SLog.Info("{0}  Confirm Margins  |  Area without margins {1}  | Area with margins {2}", this, area.Value, border_area);
                if (CONFIRM_BORDER) SLog.Info("{0}  Confirm Borders  |  Area without borders {1}  | Area with borders {2}", this, border_area, draw_area);
                if (CONFIRM_PADDING) SLog.Info("{0}  Confirm Padding  |  Area without padding {1}  | Area with padding {2}", this, draw_area, _inner_area);

                if (CONFIRM_AREA)
                {
                    SLog.Info("{0}  Confirm Area Update  | _Area {1} | inner_area: {2} | border: {3} | draw: {4}", this, set_area, inner_area, border_area, draw_area);
                    //PLog.Info("TRACE: {0}", new StackFrame(1).ToString());
                    //PLog.Info("{0}", new StackTrace().ToString());
                }
                
                Apply_Positioners();// Only "Free Floating" controls can use auto-positioners because positioning a control based on another controls location would not give the intended results due to the update/layout order of controls within parent containers.
                Apply_Sizers();

                if(area_updated)
                {
                    if (CONFIRM_LAYOUT) SLog.Info("{0}  Confirm Layout  |  Area changed, layout set to dirty.{1}", this, force?"(FORCED)":"");

                    set_layout_dirty();
                    if (isChild) parent.set_layout_dirty();//cause the parent to update due to this control altering it's position or size
                    //if (isChild) parent.set_area_dirty();// CAUSES AN INFINITE LOOP
                }
            }
            finally
            {
                post_update_area();
                lock_area_update = false;
            }


            if (dirty_layout && !_first_frame)
            {
                dirty_layout = false;
                handleLayout();
            }
            
            return true;
        }

        /// <summary>
        /// Called after update_area()
        /// </summary>
        protected virtual void post_update_area() { }

        private void reset_cached_abs_areas()
        {
            _absArea = null;// Cause our absolute area to be recalculated.
            _absInnerArea = null;// Cause our absolute inner area to be recalculated.
        }

        /// <summary>
        /// This should *ONLY* be used internally by <see cref="uiControl"/>.
        /// This function will alter the set_area value (without wasting memory or creating a new <see cref="Rect"/> instance)
        /// To alter the Size or Position of a <see cref="uiControl"/> from outside the class itself use the provided sizing and positioning functions below
        /// </summary>
        private void _alter_area(float nx, float ny, Vector2 newsize)
        {
            area = null;// I forget why, but if we don't set this to null here we get an infinite loop that stack overflows immediately on program start.
            set_area.Set(nx, ny, newsize.x, newsize.y);// Set the new area values
            //set_area = new Rect(nx, ny, newsize.x, newsize.y);
            update_area();// Finalize our changes
        }
        #endregion

        #region Style Updating

        public void update_style()
        {
            _style_combo = null;
            _style_text = null;
        }
        #endregion

        #region Area Calculations

        /// <summary>
        /// Takes a position which is in absolute screen coordinates and returns it relative to this control.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected Vector2 absToRelativePos(Vector2 pos)
        {
            //return (pos - absArea.position + area.position);
            return (pos - absArea.position);
        }

        /// <summary>
        /// Takes a position which is relative to this control and returns it in absolute screen coordinates.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected Vector2 relToAbsolutePos(Vector2 p)
        {
            //return (p + absArea.position - area.position);
            return (p + absArea.position);
        }

        /// <summary>
        /// Takes an area which is internal to this control and returns it in absolute screen coordinates.
        /// Internal areas are areas which are relatively offset within the control but which are also offset by the controls relative position.
        /// Examples include: <see cref="border_area"/>, <see cref="draw_area"/>, <see cref="_inner_area"/>
        /// </summary>
        /// <returns></returns>
        protected Vector2 internalToAbsolutePos(Vector2 p)
        {
            return (p + absArea.position - Area.position);
        }

        /// <summary>
        /// Takes an area which is relative to this control and returns it in absolute screen coordinates.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected Rect absToRelative(Rect r)
        {
            return new Rect(absToRelativePos(r.position), r.size);
        }

        /// <summary>
        /// Takes an area which is relative to this control and returns it in absolute screen coordinates.
        /// </summary>
        /// <returns></returns>
        protected Rect relToAbsolute(Rect r)
        {
            return new Rect(relToAbsolutePos(r.position), r.size);
        }

        /// <summary>
        /// Takes an area which is internal to this control and returns it in absolute screen coordinates.
        /// Internal areas are areas which are relatively offset within the control but which are also offset by the controls relative position.
        /// Examples include: <see cref="border_area"/>, <see cref="draw_area"/>, <see cref="_inner_area"/>
        /// </summary>
        /// <returns></returns>
        protected Rect internalToAbsolute(Rect r)
        {
            return new Rect(internalToAbsolutePos(r.position), r.size);
        }

        /// <summary>
        /// Takes a content size and adds the controls padding to it to get the inner size of the control.
        /// </summary>
        protected Vector2 content_size_to_inner(Vector2 content_size)
        {
            //return content_size;
            return _padding.Add(new Rect(Vector2.zero, content_size)).size;
        }

        /// <summary>
        /// Takes an inner size and removes the controls padding to it to get the TRUE content area size of the control.
        /// </summary>
        protected Vector2 inner_size_to_content(Vector2 sz)
        {
            return sz;
            //return _padding.Remove(new Rect(Vector2.zero, sz)).size;
        }

        /// <summary>
        /// Takes the final area and removes all the margin/padding/border offsets to it to return the inner area.
        /// </summary>
        protected Rect outter_area_to_inner(Rect final_area)
        {
            return _margin.Remove(borderStyle.size.Remove(_padding.Remove(final_area)));
        }

        /// <summary>
        /// Takes the final size and removes all the margin/padding/border offsets to it to return the inner size.
        /// </summary>
        protected Vector2 outter_size_to_inner(Vector2 outter_size)
        {
            return _margin.Remove(borderStyle.size.Remove(_padding.Remove(new Rect(Vector2.zero, outter_size)))).size;
        }

        /// <summary>
        /// Takes the final size and removes all the margin/padding/border offsets to it to return the inner size.
        /// </summary>
        protected Vector2 outter_pos_to_inner(Vector2 outter_pos)
        {
            return _margin.Remove(borderStyle.size.Remove(_padding.Remove(new Rect(outter_pos, Vector2.zero)))).position;
        }

        /// <summary>
        /// Takes an inner area size and adds all the margin/padding/border offsets to it and returns the final "outter" area.
        /// </summary>
        protected Rect inner_area_to_outter(Rect inner_area)
        {
            return _margin.Add(borderStyle.size.Add(_padding.Add(inner_area)));
        }

        /// <summary>
        /// Takes an inner area size and adds all the margin/padding/border offsets to it and returns the final "outter" area size.
        /// </summary>
        protected Vector2 inner_size_to_outter(Vector2 inner_size)
        {
            return _margin.Add(borderStyle.size.Add(_padding.Add(new Rect(Vector2.zero, inner_size)))).size;
        }

        /// <summary>
        /// Takes an inner area size and adds all the margin/padding/border offsets to it and returns the final "outter" area size.
        /// </summary>
        protected Vector2 inner_pos_to_outter(Vector2 inner_pos)
        {
            return _margin.Add(borderStyle.size.Add(_padding.Add(new Rect(inner_pos, Vector2.zero)))).position;
        }

        #endregion

        #region Parent Communication

        public void parent_area_updated()
        {
            //update_area();
            reset_cached_abs_areas();
            set_area_dirty();// we need to cause absArea/absInnerArea to update.
        }

        public void parent_scroll_updated()
        {
            if (isFirstFrame)
            {
                dirty_parent_scroll = true;
                return;
            }
            dirty_parent_scroll = false;
            _parentScroll = null;
            reset_cached_abs_areas();
            //set_area_dirty();// we need to cause absArea/absInnerArea to update.
            if(isParent)
            {
                foreach (uiControl child in (this as uiPanel).Get_Children()) { child.parent_scroll_updated(); }
            }

            update_area();

            if(CONFIRM_SCROLL) SLog.Info("{0}  Parent Scrolled  | parentScroll: {1}  |  parent.ScrollPos: {2}  |  Area: {3}", this, parentScroll, (parent as IScrollableUI).Get_ScrollPos(), absArea);
        }

        // One of the controls within this control's parent-chain changed.
        public void parent_chain_updated()
        {
            if (isFirstFrame)
            {
                dirty_parent_chain = true;
                return;
            }
            dirty_parent_chain = false;
            parent_chain.Clear();
            uiPanel p = parent;
            while (p != null)
            {
                parent_chain.Add(p);
                p = p.parent;
            }

            if (isParent)
            {
                foreach (uiControl child in (this as uiPanel).Get_Children())
                {
                    child.parent_chain_updated();
                }
            }
        }

        public void parent_visibility_updated() { _parent_visible = null; isDisplayed = _displayed;/*Refresh the value*/ }

        public void parent_enable_updated() { _parent_disabled = null; isDisabled = _disabled;/*Refresh the value*/ }

        #endregion

        #region Position
        
        /// <summary>
        /// Updates the controls position to match a given position if it's not equal to the current position.
        /// All coordinates are restricted such that they are above 0.
        /// </summary>
        protected bool maybeUpdate_Pos(float x, float y)
        {
            if (x < 0f) x = 0;
            if (y < 0f) y = 0;

            if (!Util.floatEq(set_area.position.x, x) || !Util.floatEq(set_area.position.y, y))
            {
                if (CONFIRM_POS) SLog.Info("{0}  Pos Changed | pos {1} | new_pos ({2}, {3})", this, set_area.position, x, y);
                _alter_area(x, y, size);
                //_alter_area(newpos, set_area.size);
                //Area = new Rect(newpos, size);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clamps a vector's coordinates so they are never below zero.
        /// </summary>
        protected void clamp_pos(ref float x, ref float y)
        {
            if (x < 0) x = 0f;
            if (y < 0) y = 0f;
        }
        
        public void Apply_Positioners()
        {
            if (isDraggable)// Draggable components shouldnt have their positions affected by auto-positioners
            {
                vertical_positioner = null;
                horizontal_positioner = null;
                return;
            }

            if (vertical_positioner != null) vertical_positioner.Apply(this);
            if (horizontal_positioner != null) horizontal_positioner.Apply(this);
        }

        public void print_positioners()
        {
            SLog.Info("{0}  {1}", this.vertical_positioner, this.horizontal_positioner);
        }
        
        public void Set_Pos(float x, float y)
        {
            maybeUpdate_Pos(x, y);
            //area = new Rect(new Vector2(x, y), _area.size);
        }

        public void Set_Pos(Vector2 pos)
        {
            maybeUpdate_Pos(pos.x, pos.y);
            //area = new Rect(pos, _area.size);
        }

        public void Set_PosX(float x)
        {
            maybeUpdate_Pos(x, set_area.y);
            //area = new Rect(new Vector2(x, this._area.y), _area.size);
        }

        public void Set_PosY(float y)
        {
            maybeUpdate_Pos(set_area.x, y);
            //area = new Rect(new Vector2(this._area.x, y), _area.size);
        }

        /// <summary>
        /// Positions the control so it's top edge is yOff away below another given control.
        /// </summary>
        public void moveBelow(uiControl targ, float yOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");
            
            maybeUpdate_Pos(Area.x, targ.Area.yMax + yOff);
        }

        /// <summary>
        /// Positions the control so it's bottom edge is yOff above the top edge of another given control.
        /// </summary>
        public void moveAbove(uiControl targ, float yOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(Area.position.x, targ.Area.yMin - size.y - yOff);
        }

        /// <summary>
        /// Positions the control so it's right edge is xOff away from the right edge of another given control.
        /// </summary>
        public void moveRightOf(uiControl targ, float xOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMax + xOff, Area.position.y);
            //if (horizontal_positioner == null || !horizontal_positioner.Equals(targ, xOff, cPosDir.RIGHT)) horizontal_positioner = new ControlPositioner(targ, xOff, cPosDir.RIGHT);
        }

        /// <summary>
        /// Positions the control so it's right edge is xOff away from the left edge of another given control.
        /// </summary>
        public void moveLeftOf(uiControl targ, float xOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMin - size.x - xOff, Area.y);
            //if (horizontal_positioner == null || !horizontal_positioner.Equals(targ, xOff, cPosDir.LEFT)) horizontal_positioner = new ControlPositioner(targ, xOff, cPosDir.LEFT);
        }
        
        /// <summary>
        /// Adjusts X & Y position of the control so it sites directly above another given control.
        /// </summary>
        public void sitAbove(uiControl targ, float yOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMin, targ.Area.yMin - Area.height - yOff);
            horizontal_positioner = null;
            //if (vertical_positioner == null || !vertical_positioner.Equals(targ, yOff, cPosDir.ABOVE)) vertical_positioner = new ControlPositioner(targ, yOff, cPosDir.SIT_ABOVE);
        }

        /// <summary>
        /// Adjusts X & Y position of the control so it sites directly below another given control.
        /// </summary>
        public void sitBelow(uiControl targ, float yOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMin, targ.Area.yMax + yOff);
            horizontal_positioner = null;
            //if (vertical_positioner == null || !vertical_positioner.Equals(targ, yOff, cPosDir.BELOW)) vertical_positioner = new ControlPositioner(targ, yOff, cPosDir.SIT_BELOW);
        }
        
        /// <summary>
        /// Adjusts X & Y position of the control so it sites directly to the right of another given control.
        /// </summary>
        public void sitRightOf(uiControl targ, float xOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMax + xOff, targ.Area.yMin);
            vertical_positioner = null;
            //if (horizontal_positioner == null || !horizontal_positioner.Equals(targ, xOff, cPosDir.RIGHT)) horizontal_positioner = new ControlPositioner(targ, xOff, cPosDir.SIT_RIGHT_OF);
        }

        /// <summary>
        /// Adjusts X & Y position of the control so it sites directly to the left of another given control.
        /// </summary>
        public void sitLeftOf(uiControl targ, float xOff = 0f)
        {
            if (targ == null) throw new ArgumentNullException(Name + " target cannot be NULL!");
            if (targ.parent != parent) throw new ArgumentException(Name + " target control must be parented to the same control!");

            maybeUpdate_Pos(targ.Area.xMin - Area.width - xOff, targ.Area.position.y);
            vertical_positioner = null;
            //if (horizontal_positioner == null || !horizontal_positioner.Equals(targ, xOff, cPosDir.LEFT)) horizontal_positioner = new ControlPositioner(targ, xOff, cPosDir.SIT_LEFT_OF);
        }

        /// <summary>
        /// Repositions the control so it's top edge sits on it's parent's top edge.
        /// </summary>
        /// <param name="yOff">Offset from the edge where we will be positioned</param>
        public void alignTop(float yOff = 0f)
        {
            float x = Area.position.x;
            float y = yOff;
            clamp_pos(ref x, ref y);
            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.
            if (vertical_positioner == null || !vertical_positioner.Equals(null, yOff, cPosDir.TOP_OF)) vertical_positioner = new ControlPositioner(null, yOff, cPosDir.TOP_OF);
        }

        /// <summary>
        /// Repositions the control so it's bottom edge sits on it's parent's bottom edge.
        /// </summary>
        /// <param name="yOff">Offset from the edge where we will be positioned</param>
        public void alignBottom(float yOff = 0f)
        {
            float val = Screen.height;
            if (isChild) val = parent.Get_Content_Area().height;// parent.inner_area.height;

            float x = Area.x;
            float y = (val - size.y - yOff);
            clamp_pos(ref x, ref y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.
            if (CONFIRM_POSITIONER) SLog.Info("{0}  Confirm Positioner  |  align bottom  |  offset: {1}  |  val: {2}  |  area: {3}  |  size: {4}", this, new Vector2(x,y), val, Area, size);

            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.

            if (isChild) return;
            else if (vertical_positioner == null || !vertical_positioner.Equals(null, yOff, cPosDir.BOTTOM_OF)) vertical_positioner = new ControlPositioner(null, yOff, cPosDir.BOTTOM_OF);
        }

        /// <summary>
        /// Repositions the control so it's left edge sits on it's parent's left edge.
        /// </summary>
        /// <param name="xOff">Offset from the edge where we will be positioned</param>
        public void alignLeftSide(float xOff = 0f)
        {
            float x = xOff;
            float y = Area.position.y;
            clamp_pos(ref x, ref y);
            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.

            if (horizontal_positioner == null || !horizontal_positioner.Equals(null, xOff, cPosDir.LEFT_SIDE_OF)) horizontal_positioner = new ControlPositioner(null, xOff, cPosDir.LEFT_SIDE_OF);
        }

        /// <summary>
        /// Repositions the control so it's right edge sits on it's parent's right edge.
        /// </summary>
        /// <param name="xOff">Offset from the edge where we will be positioned</param>
        public void alignRightSide(float xOff = 0f)
        {
            float val = Screen.width;
            if(isChild) val = parent.Get_Content_Area().width;

            float x = (val - size.x - xOff);
            float y = Area.position.y;
            clamp_pos(ref x, ref y);
            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.

            if (isChild) return;
            else if (horizontal_positioner == null || !horizontal_positioner.Equals(null, xOff, cPosDir.RIGHT_SIDE_OF)) horizontal_positioner = new ControlPositioner(null, xOff, cPosDir.RIGHT_SIDE_OF);
        }

        /// <summary>
        /// Aligns the control to the middle of the parent area's Vertical axis.
        /// </summary>
        /// <param name="yOff">An offset from position this control should occupy</param>
        public void CenterVertically(float yOff = 0f)
        {
            float val = Screen.height;
            if (isChild) val = parent.Get_Content_Area().height;

            float x = Area.position.x;
            float y = ((val / 2) - (size.y / 2) - yOff);
            clamp_pos(ref x, ref y);
            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.

            if (isChild) return;
            else if (vertical_positioner == null || !vertical_positioner.Equals(null, 0f, cPosDir.CENTER_Y)) vertical_positioner = new ControlPositioner(null, 0f, cPosDir.CENTER_Y);
        }
        
        /// <summary>
        /// Aligns the control to the middle of the parent area's Horizontal axis.
        /// </summary>
        /// <param name="xOff">An offset from position this control should occupy</param>
        public void CenterHorizontally(float xOff = 0f)
        {
            float val = Screen.width;
            if (isChild) val = parent.Get_Content_Area().width;

            float x = ((val / 2) - (size.x / 2) - xOff);
            float y = Area.position.y;
            clamp_pos(ref x, ref y);
            maybeUpdate_Pos(x, y);// we use 'area' instead of '_area' because we want to keep the control at the current position it is at.

            if (isChild) return;
            else if (horizontal_positioner == null || !horizontal_positioner.Equals(null, 0f, cPosDir.CENTER_X)) horizontal_positioner = new ControlPositioner(null, 0f, cPosDir.CENTER_X);
        }

        #endregion
        
        #region AutoSizing

        /// <summary>
        /// Performs autosizing logic and returns what the outter size of the control should be.
        /// </summary>
        /// <param name="inner_size"></param>
        /// <returns>The final outter size for the control</returns>
        protected virtual Vector2 Get_Autosize(Vector2? inner_size = null)
        {
            Vector2 sz = Vector2.zero;
            if (inner_size.HasValue) sz = inner_size_to_outter(inner_size.Value);// If we were given a starting size then use it
            else if (content != null)// otherwise we will TRY and use the size of our contents.
            {
                Vector2 csz = styleText.CalcSize(content);
                if(isChild)
                {
                    if (isSizeConstrained)
                    {
                        // Constrain our content width to the parents bounds
                        Vector2 cosz = inner_size_to_outter(content_size_to_inner(csz));// content outter size
                        Vector2 cpsz = parent.Constrain_Child_Size(this, cosz);// content parent constrained size
                        csz = outter_size_to_inner(cpsz);

                        if (styleText.wordWrap)
                        {
                            float h = styleText.CalcHeight(content, csz.x - _padding.horizontal);
                            //if (type == uiControlType.TextArea) PLog.Info("{0}  Autosize  |  Content Height: {1}  |  Height(unwrapped): {2}", this, h, csz.y);
                            csz.y = h;
                        }
                    }
                }
                sz = inner_size_to_outter(content_size_to_inner(csz));
            }
            Vector2 ssz = new Vector2(sz.x, sz.y);

            switch (Autosize_Method)
            {
                case AutosizeMethod.ICON_FILL:// Expand to fill the remaining space within the parent's Y axis
                    if (isChild)
                        sz.y = (parent.Get_Content_Area().height - Area.y);
                    else
                        sz.y = (Screen.height - Area.y);

                    sz.x = sz.y;
                    break;
                case AutosizeMethod.FILL:// Expand to fill the remaining space within the parent's X axis
                    if (isChild)
                    {
                        sz.x = (parent.Get_Content_Area().width - Area.x);
                        sz.y = (parent.Get_Content_Area().height - Area.y);
                    }
                    else
                    {
                        sz.x = (Screen.width - Area.x);
                        sz.y = (Screen.height - Area.y);
                    }
                    break;
                case AutosizeMethod.BLOCK:// Expand to fill the remaining space within the parent's X axis
                    if (isChild) sz.x = (parent.Get_Content_Area().width - Area.x);
                    else sz.x = (Screen.width - Area.x);
                    break;
                case AutosizeMethod.GROW:// Treat the user-specified size as the MINIMUM size we can be.
                    if (sz.x < _size.x) sz.x = _size.x;
                    if (sz.y < _size.y) sz.y = _size.y;
                    break;
                case AutosizeMethod.SHRINK:// Treat the user-specified size as the MAXIMUM size we can be.
                    // Yea this one is a little broken, dunno what I was envisioning it would do but in reality this method just makes the control size to 0 or below until a size is explicitly set with Set_Size()
                    if (sz.x > _size.x) sz.x = _size.x;
                    if (sz.y > _size.y) sz.y = _size.y;
                    break;
            }
            if (isChild) sz = parent.Constrain_Child_Size(this, sz);


            if (CONFIRM_SIZE)
            {
                if (!_confirm_autosize_last.HasValue || sz != _confirm_autosize_last.Value)
                {
                    Vector2 last_autosize = Vector2.zero;
                    if (_confirm_autosize_last.HasValue) last_autosize = _confirm_autosize_last.Value;
                    SLog.Info("{0}  Confirm AutoSize Changed  |  new_autosize: {1}  |  last_autosize: {2}  |  starting_size: {3}", this, sz, last_autosize, ssz);
                }
                _confirm_autosize_last = sz;
                //PLog.Info("TRACE: {0}", new StackFrame(1).ToString());
                //PLog.Info("TRACE: {0}", new StackTrace().ToString());
            }

            return sz;
        }
        #endregion

        #region Size

        protected bool maybeUpdate_Size(Vector2 newsize)
        {
            // Check if the size that was SET for the control matches the new size we WANT to set.
            if (!Util.floatEq(set_area.width, newsize.x) || !Util.floatEq(set_area.height, newsize.y))
            {
                if (CONFIRM_SIZE) SLog.Info("{0}  Size Changed | size {1} | new_size {2}", this, set_area.size, newsize);
                _alter_area(set_area.position.x, set_area.position.y, newsize);
                //Area = new Rect(set_area.position, newsize);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the controls size has changed due to something like autosizing and if so then updates the size to match.
        /// </summary>
        protected bool update_size() { return maybeUpdate_Size(size); }

        public void Apply_Sizers()
        {
            if (width_sizer != null) width_sizer.Apply(this);
            if (height_sizer != null) height_sizer.Apply(this);
        }

        /// <summary>
        /// Returns the maximum size this control could be sized to according to it's set Max-Size and the maximum potential size of it's parenting control.
        /// Currently does NOT return correct results...
        /// </summary>
        private Vector2 Get_Max_Potential_Size()
        {
            if (_potential_area_max_cached.HasValue) { return _potential_area_max_cached.Value; }

            if (isChild)
            {
                Vector2 sz = parent.available_area;
                sz = parent.outter_size_to_inner(sz);
                if (Max_Width.HasValue && sz.x > Max_Width.Value) sz.x = Max_Width.Value;
                if (Max_Height.HasValue && sz.y > Max_Height.Value) sz.y = Max_Height.Value;

                // Update the cached value!
                _potential_area_max_cached = sz;
                return sz;
            }

            // Update the cached value!
            _potential_area_max_cached = new Vector2(Screen.width, Screen.height);
            return _potential_area_max_cached.Value;
        }

        /// <summary>
        /// Returns the current area size of the control. 
        /// </summary>
        public Vector2 Get_Size() { return Area.size; }

        /// <summary>
        /// Returns the current area width of the control. 
        /// </summary>
        public float Get_Width() { return Area.width; }

        /// <summary>
        /// Returns the current area height of the control. 
        /// </summary>
        public float Get_Height() { return Area.height; }
        

        public void Set_Size(float w, float h)
        {
            has_explicit_W = true;
            has_explicit_H = true;
            maybeUpdate_Size(new Vector2(w, h));
            //area = new Rect(_area.position, new Vector2(w, h));
        }

        public void Set_Size(Vector2 sz)
        {
            has_explicit_W = true;
            has_explicit_H = true;
            maybeUpdate_Size(sz);
            //area = new Rect(_area.position, sz);
        }

        public void Set_Width(float val)
        {
            has_explicit_W = true;
            Area = new Rect(set_area.position, new Vector2(val, set_area.size.y));
        }

        public void Set_Height(float val)
        {
            has_explicit_H = true;
            Area = new Rect(set_area.position, new Vector2(set_area.size.x, val));
        }

        /// <summary>
        /// Unsets the controls given size values such that they will not override autosizing logic.
        /// </summary>
        public void Unset_Size()
        {
            has_explicit_W = false;
            has_explicit_H = false;
            Area = new Rect(set_area.position, size);
        }

        /// <summary>
        /// Unsets the controls given width value such that it will not override autosizing logic.
        /// </summary>
        public void Unset_Width()
        {
            has_explicit_W = false;
            Area = new Rect(set_area.position, new Vector2(size.x, set_area.size.y));
        }

        /// <summary>
        /// Unsets the controls given height value such that it will not override autosizing logic.
        /// </summary>
        public void Unset_Height()
        {
            has_explicit_H = false;
            Area = new Rect(set_area.position, new Vector2(set_area.size.x, size.y));
        }

        /// <summary>
        /// Resizes the control so it's bottom edge is yOff away from the top edge of another given control.
        /// </summary>
        public void snapBottomSideTo(uiControl targ, float yOff = 0f)
        {
            maybeUpdate_Size(new Vector2(_size.x, targ.Area.yMin - this.Area.yMin - yOff));
        }
        
        /// <summary>
        /// Resizes the control so it's right edge is xOff away from the left edge of another given control.
        /// </summary>
        public void snapRightSideTo(uiControl targ, float xOff = 0f)
        {
            maybeUpdate_Size(new Vector2(targ.Area.xMin - this.Area.xMin - xOff, this._size.y));
        }

        public void FloodX(float xOff = 0f)
        {
            float val = Screen.width;
            if (isChild) val = parent.Get_Content_Area().width;

            maybeUpdate_Size(new Vector2(val - Area.x - xOff, _size.y));
            if (width_sizer == null || !width_sizer.Equals(xOff, cSizeMode.FLOOD_X)) width_sizer = new ControlSizer(xOff, cSizeMode.FLOOD_X);
        }

        public void FloodY(float yOff = 0f)
        {
            float val = Screen.height;
            if (isChild) val = parent.Get_Content_Area().height;

            maybeUpdate_Size(new Vector2(_size.x, val - Area.y - yOff));
            if (height_sizer == null || !height_sizer.Equals(yOff, cSizeMode.FLOOD_Y)) height_sizer = new ControlSizer(yOff, cSizeMode.FLOOD_Y);
        }
        
        /// <summary>
        /// Shortcut for <see cref="FloodX(float)"/> & <see cref="FloodY(float)"/>
        /// </summary>
        public void FloodXY(float xOff=0f, float yOff=0f)
        {
            this.FloodX(xOff);
            this.FloodY(yOff);
        }

        /// <summary>
        /// Restricts a given size to be within this controls set min/max size range if any.
        /// </summary>
        /// <param name="sz"></param>
        protected Vector2 constrain_size(Vector2 sz)
        {
            float x = sz.x;
            float y = sz.y;

            if (Min_Width.HasValue) x = Mathf.Max(x, Min_Width.Value);
            if (Min_Height.HasValue) y = Mathf.Max(y, Min_Height.Value);

            if (Max_Width.HasValue) x = Mathf.Min(x, Max_Width.Value);
            if (Max_Height.HasValue) y = Mathf.Min(y, Max_Height.Value);

            sz.x = x;
            sz.y = y;

            return sz;
        }

        protected Vector2 apply_size_constraints(Vector2 sz)
        {
            switch(_sizeConstraint)
            {
                case uiSizeConstraint.WIDTH_MATCHES_HEIGHT:
                    sz.x = sz.y;
                    break;
                case uiSizeConstraint.HEIGHT_MATCHES_WIDTH:
                    sz.y = sz.x;
                    break;
            }

            return sz;
        }
        #endregion

        #region HELPER STUFF

        public bool isPointWithin(Vector2 p) { return absArea.Contains(p); }
        #endregion

        #region Styling Logic

        protected void check_style()
        {
            dirty_style = false;
            bool dirty = false;

            if (styleText.normal.textColor != _style_text.normal.textColor) dirty = true;
            if (styleText.active.textColor != _style_text.active.textColor) dirty = true;
            if (styleText.hover.textColor != _style_text.hover.textColor) dirty = true;
            if (styleText.focused.textColor != _style_text.focused.textColor) dirty = true;

            if (styleText.font != _style_text.font) dirty = true;
            if (styleText.fontSize != _style_text.fontSize) dirty = true;
            if (styleText.fontStyle != _style_text.fontStyle) dirty = true;

            if (dirty) styleText = null;//recache
        }

        protected GUIStyle get_skin_style_for_type(uiControlType ty)
        {
            //if (uses_parent_text_style && parent != null) return parent.styleNoBG;

            switch (ty)
            {
                case uiControlType.Window:
                    return mySkin.window;
                case uiControlType.Panel:
                    return mySkin.GetStyle("panel");
                case uiControlType.Panel_Dark:
                    return mySkin.box;
                case uiControlType.Button:
                    return mySkin.button;
                case uiControlType.Text:
                    return mySkin.label;
                case uiControlType.Textbox:
                    return mySkin.textField;
                case uiControlType.TextArea:
                    return mySkin.textArea;
                case uiControlType.Checkbox:
                    return mySkin.toggle;
                default:
                    return mySkin.box;
            }
        }

        public void Set_Background(Color clr) { Util.Set_BG_Color(local_style.normal, clr); }

        public void Set_Background(Color clrA, Color clrB, GRADIENT_DIR dir, float? exp = null) { Util.Set_BG_Gradient(local_style.normal, 128, dir, clrA, clrB, exp); }

        public void Set_Background(Texture2D tex) { local_style.normal.background = tex; }
        #endregion

        #region Layout Logic

        /// <summary>
        /// Usage: Responding to changes in child-control position/sizes
        /// </summary>
        protected virtual void doLayout_Post() { }

        /// <summary>
        /// Usage: positioning child controls
        /// </summary>
        public virtual void doLayout() { }

        protected void handleLayout()
        {
            if (CONFIRM_LAYOUT) SLog.Info("{0}  Confirm Layout", this);
            doLayout();
            onLayout?.Invoke(this as uiPanel);// used by custom controls which cannot override the base function so they can position any child controls.
            doLayout_Post();
            // we want to set this layout var to false AFTER we do the layout logic because child controls which move during said layout functions will 
            // inevitably cause this var to set to true, which is invalid in this context because we just updated all the ontrols around them anyway
            // which is the entire point of them doing so.
            dirty_layout = false;
            //area = null;
            if (dirty_area)
            {
                dirty_area = false;
                update_area();
            }
        }
        #endregion

        #region GUI Event Handling

        /// <summary>
        /// Returns <c>TRUE</c> if this control could potentially consume a given event if it was a mouse event.
        /// </summary>
        public bool couldTakeMouseEvent(Vector2 mousePos)
        {
            if (!isVisible || isDisabled) return false;
            if (!absArea.Contains(mousePos)) return false;

            return true;
        }

        public virtual void handleEvent() { }

        public void TryHandleEvent()
        {
            if (!isVisible) return;
            handleEvent_Base();
        }
        
        /// <summary>
        /// 'base' event handlling logic for setting state flags for the control
        /// </summary>
        public virtual void handleEvent_Base()
        {
            if (isDisabled) return;


            Event evt = Event.current;
            EventType et = evt.GetTypeForControl(unity_id);
            // XXX: Try removing the check for EventType == layout and see if it alleviates lag while still operating correctly.
            if (dirty_layout || et == EventType.Layout) handleLayout();
            
            //if(this.typename.Length>0 && evt.isMouse && et!= EventType.Repaint && et!= EventType.Used && et!= EventType.Ignore && et!= EventType.Layout)
                //PLog.Info("[{0}] Event: {1}", this.typename, evt);

            isMouseOver = absArea.Contains(evt.mousePosition);
            bool use_event = false;
            switch (et)
            {
                case EventType.MouseDown:
                    if (isMouseOver && isClickable)
                    {
                        if (GUIUtility.hotControl == 0)
                        {
                            GUIUtility.hotControl = unity_id;
                            use_event = true;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    bool wasDown = isMouseDown;
                    isMouseDown = false;
                    if (isMouseOver && isClickable && wasDown) {
                        use_event = true;
                        onClicked?.Invoke(this);
                    }

                    if (GUIUtility.hotControl == unity_id) { GUIUtility.hotControl = 0; }
                    break;
                //case EventType.MouseMove:// This event is never sent to any controls except unity's EditorWindow type controls
                case EventType.Ignore:
                case EventType.Used:
                    return;
                case EventType.Layout:
                    Event.current.Use();
                    return;
            }

            handleEvent();// Give this control a chance to perform it's custom logic, for things like windows, buttons, etc.
            if (use_event && Event.current.GetTypeForControl(this.unity_id) != EventType.Used)
            {
                switch(et)
                {
                    case EventType.MouseDown:
                        isMouseDown = (GUIUtility.hotControl == unity_id);
                        break;
                    case EventType.MouseUp:
                        debug_last_consumer_click = this;
                        break;
                }
                evt.Use();// We need to check and make sure that none of the custom logic for this control used the event before we try to.
            }
        }
        #endregion

        #region Internal Events
        // These are events that are generated by a control for itself to keep its logic flow easier to understand.
        
        private void handle_ParentChange()
        {
            parent_chain_updated();
        }
        #endregion

        #region Rendering

        private static Color DISABLED_TINT = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        public virtual void TryDisplay()
        {
            if (!isVisible) return;
            if (CONFIRM_DRAW) { SLog.Info("{0}  Confirm Display  |  area: {1}", this, Area); }

            Color? prevClr = null;
            if(Tint.HasValue)
            {
                prevClr = GUI.color;
                GUI.color = Tint.Value;
            }

            try
            {
                Display();
                if (isDisabled) mySkin.GetStyle("disabled").Draw(border_area, GUIContent.none, false, false, false, false);
            }
            finally
            {
                if (prevClr.HasValue)
                    GUI.color = prevClr.Value;
                else if(Tint.HasValue)// if we have a tint but 'prevClr' is not set then we DID change the GUI color but have no clue what it was before. so default it to white.
                    GUI.color = Color.white;
            }

        }

        /// <summary>
        /// Draw the background of the control
        /// </summary>
        protected virtual void Display_BG()
        {
            if(!disableBG) Style.Draw(draw_area, GUIContent.none, isHovered, isActive, false, isFocused);
            draw_border();

            if (isMouseOver && Tooltip != null) GUI.tooltip = Tooltip;
        }

        protected virtual void Display_Text()
        {
            if(content != null && content.text != null && content.text.Length>0) styleText.Draw(inner_area, content, isHovered, isActive, false, isFocused);
        }

        protected abstract void Display();

        protected void draw_border()
        {
            if (Border.type == uiBorderType.NONE) return;
            if (cached_borderStyle == null) return;
            if (cached_borderStyle.size.vertical == 0 && cached_borderStyle.size.horizontal == 0) return;
            if (cached_borderStyle.texture == null) cached_borderStyle.prepare_texture(Area.size);
            if (cached_borderStyle.texture == null) return;

            GUI.DrawTexture(border_area, cached_borderStyle.texture);
        }
#endregion

        #region First Frame

        private void Start() { handle_FirstFrame(); }

        /// <summary>
        /// Performs initial calls to handleLayout() and update_area() so the control can have a valid area.
        /// </summary>
        public virtual void handle_FirstFrame()
        {
            //PLog.Info("{0}  Confirm FIRST FRAME: {1}  |  Area: {2}", this, _first_frame, area);
            if (!_first_frame) return;// We already did this stuff, so skip it.
            if (CONFIRM_AREA || CONFIRM_LAYOUT) SLog.Info("{0}  Confirm FIRST FRAME!", this);
            
            lock_area_update = false;// Unlock area updates now.

            if (isParent)
            {
                /// In order to make sure our child-controls HAVE a size so we can calculate a valid content-area for ourselves.
                /// We need to force all of our child-controls firstFrame setup logic to fire now
                foreach (uiControl child in (this as uiPanel).Get_Children()) { child.handle_FirstFrame(); }
            }

            update_area(true);
            if(isParent) handleLayout();
            _first_frame = false;
        }
        #endregion
        
        private void OnGUI()
        {
            bool was_vis = _was_visible;
            _was_visible = isVisible;
            //see if our visibility has changed since last frame, if so and we are a parent control then notify our children.
            // We do this because some controls override the isVisible field accessor and make their visibility dependent on other variables which can't be tracked accurately.
            if (was_vis != _was_visible)// Visibility Changed
            {
                if (isParent) { (this as uiPanel).handle_visibility_change(); }

                if (_was_visible) onShown?.Invoke(this);
                else onHidden?.Invoke(this);
            }
            if (!isVisible) return;//non-visible controls should also not get events, so don't...

            // Abort drawing the panel IF we are not currently visible
            bool was_repaint = (Event.current.type == EventType.Repaint);

            handleEvent_Base();

            if (dirty_area) update_area();

            if (cached_borderStyle == null || !cached_borderStyle.Equals(borderStyle))// If the border SIZE changed then update the controls area again
            {
                update_area();
                cached_borderStyle = new uiBorderStyleState(borderStyle);
                cached_borderStyle.prepare_texture(size);
            }

            if(dirty_style) check_style();

            EventType et = Event.current.GetTypeForControl(this.unity_id);
            if (!was_repaint)// If this WAS a Repaint event then no matter what our control could only be rendered with the code below. So even if the event type has changed since then (and was allegedly handled elsewhere) we will go ahead and draw it anyhow!
            {
                if (et == EventType.Ignore || et == EventType.Used) return;
            }
            else if ((was_repaint || et == EventType.Repaint) && isChild) return;// If this is a repaint event and this control is a child control then we need to ignore this event as our parent control will tell us when to render.
            

            GUISkin prevSkin = GUI.skin;
            GUI.skin = mySkin;
            
            this.TryDisplay();

            GUI.skin = prevSkin;
        }

        private void Update()
        {
            onThink?.Invoke();
        }
    }

}
