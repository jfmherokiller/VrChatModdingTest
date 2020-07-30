using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SR_PluginLoader
{
    public static class DebugUI
    {
        private static Active_State_Tracker State = new Active_State_Tracker("DebugUI");
        /// <summary>
        /// The panel that holds all of the DebugUI controls, so we can easily deactivate it if needed.
        /// </summary>
        private static uiPanel Root = null;
        public static uiPanel ROOT { get { return Root; } }

        /// <summary>
        /// A convenient label to output the players coordinates onscreen
        /// </summary>
        internal static uiVarText lbl_player_pos = null;
        /// <summary>
        /// A convenient label to display the active camera's position
        /// </summary>
        internal static uiVarText lbl_cam_pos = null;
        /// <summary>
        /// A convenient label to display the active camera's rotation
        /// </summary>
        internal static uiVarText lbl_cam_rot = null;
        /// <summary>
        /// A label that shows the current debug drawing mode for uiControls
        /// </summary>
        internal static uiText lbl_debug_mode = null;
        
        public static void Setup()
        {
            SiscosHooks.register(HOOK_ID.Level_Loaded, onLevelLoaded);
            Init();

            Root = uiControl.Create<uiPanel>();
            Root.Name = "DebugUI";
            Root.Set_Padding(5);
            Root.FloodXY();
            Root.local_style.normal.background = null;
            Root.isVisible = false;//Because our State var is inactive by default

            var list = uiControl.Create<uiListView>(Root);// Using a uiListView to contain all of our debug var displays makes them all auto layout, which is nice
            list.alignLeftSide();
            list.alignTop(200);

            lbl_player_pos = uiControl.Create<uiVarText>(list);
            lbl_player_pos.Text = "Player Pos:";
            lbl_player_pos.Set_Margin(0);
            lbl_player_pos.Set_Padding(0);

            lbl_cam_pos = uiControl.Create<uiVarText>(list);
            lbl_cam_pos.Text = "Cam Pos:";
            lbl_cam_pos.Set_Margin(0);
            lbl_cam_pos.Set_Padding(0);

            lbl_cam_rot = uiControl.Create<uiVarText>(list);
            lbl_cam_rot.Text = "Cam Rot:";
            lbl_cam_rot.Set_Margin(0);
            lbl_cam_rot.Set_Padding(0);


            lbl_debug_mode = uiControl.Create<uiText>(list);
            //lbl_debug_mode.isVisible = false;//only shows when the debug drawing mode isnt NONE

            uiControl.dbg_mouse_tooltip_style = new GUIStyle();
            uiControl.dbg_mouse_tooltip_style.normal.textColor = Color.white;
            Util.Set_BG_Color(uiControl.dbg_mouse_tooltip_style.normal, new Color(0f, 0f, 0f, 0.5f));
        }

        private static Sisco_Return onLevelLoaded(ref object sender, ref object[] args, ref object return_value)
        {
            Init();
            return null;
        }

        public static void Init()
        {
            if (Camera.main == null) return;
            if (Camera.main.gameObject.GetComponent<DebugUI_Script>() != null) return;

            GameObject gm = Camera.main.gameObject;
            gm.AddComponent<DebugUI_Script>();
        }
        
        public static void Draw_Rect(Rect r)
        {
            const float Z = 0f;
            const float o = 0f;// 1f;
            float rx = (float)Math.Round(r.x)+0.5f;// center of the pixel
            float ry = (float)Math.Round(r.y)+0.5f;// center of the pixel
            float rw = (float)Math.Round(r.width)-1;
            float rh = (float)Math.Round(r.height)-1;

            // Left
            GL.Vertex3(rx + o, ry + 0, Z);//we don't add the offset to this point so that we don't get a single clear pixel at the upper left corner of the rectangle.
            GL.Vertex3(rx + o, ry + rh, Z);
            // Bottom
            GL.Vertex3(rx + o, ry + rh, Z);
            GL.Vertex3(rx + rw, ry + rh, Z);
            // Right
            GL.Vertex3(rx + rw, ry + rh, Z);
            GL.Vertex3(rx + rw, ry + 0, Z);
            // Top
            GL.Vertex3(rx + rw, ry + 0, Z);
            GL.Vertex3(rx + o, ry + 0, Z);
        }

        public static void Draw_Point(Vector2 pos)
        {
            const float Z = 0f;
            float rx = (float)Math.Round(pos.x) + 0.5f;// center of the pixel
            float ry = (float)Math.Round(pos.y) + 0.5f;// center of the pixel
            float rw = (float)Math.Round(6f) - 1;
            float rh = (float)Math.Round(6f) - 1;
            
            // Left
            GL.Vertex3(rx + 0 , ry - rh, Z);// Top
            GL.Vertex3(rx - rw, ry + rh, Z);// Left Bottom
            // Bottom
            GL.Vertex3(rx - rw, ry + rh, Z);// Left Bottom
            GL.Vertex3(rx + rw, ry + rh, Z);// Right Bottom
            // Right
            GL.Vertex3(rx + rw, ry + rh, Z);// Right Bottom
            GL.Vertex3(rx + 0 , ry - rh, Z);// Top
        }

        public static void Draw_GameObj_Bounds(GameObject obj)
        {
            if (obj == null) return;
            RectTransform trans = (obj.transform as RectTransform);

            Rect area = Util.Get_Unity_UI_Object_Area(obj);
            GL.Color(uiControl.color_purple);
            DebugUI.Draw_Rect(area);

            GL.Color(uiControl.color_orange);
            DebugUI.Draw_Point(Util.Get_Unity_UI_Object_AnchorPos(trans.gameObject));
        }

        public static bool isVisible { get { return Root.isVisible; } }

        public static void Hide() { Root.isVisible = State.Deactivate(); }
        public static void Show() { Root.isVisible = State.Activate(); }
    }

    public class DebugUI_Script : MonoBehaviour
    {
        private void Start() { /*DebugUI.Hide();*/ }

        private void Update()
        {
            // Implement some convinient debugging functions at the press of a button!

            if (Input.GetKeyUp(KeyCode.F3))
            {
                if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    Player.Toggle_Fly_Mode();
                }
            }

            if (Input.GetKeyUp(KeyCode.F4))// Toggle rendering uiControl area outlines on/off
            {
                if (!DebugUI.isVisible) DebugUI.Show();
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    uiControl.DEBUG_DRAW_MODE = uiDebugDrawMode.NONE;
                    DebugUI.Hide();
                }
                else
                {
                    int d = 1;
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) d = -1;

                    int mode = ((int)uiControl.DEBUG_DRAW_MODE + d) % uiControl.DEBUG_DRAW_MODE_MAX;
                    if (mode < 0) mode += uiControl.DEBUG_DRAW_MODE_MAX;

                    uiControl.DEBUG_DRAW_MODE = (uiDebugDrawMode)mode;
                }

                //DebugUI.lbl_debug_mode.isVisible = !(uiControl.DEBUG_DRAW_MODE == uiDebugDrawMode.NONE);
                DebugUI.lbl_debug_mode.Text = Enum.GetName(typeof(uiDebugDrawMode), uiControl.DEBUG_DRAW_MODE);
            }

            if (Input.GetKeyUp(KeyCode.F10))// Toggle the ingame HUD and Viewmodel on/off
            {
                HUD.Toggle();
                ViewModel.Toggle(HUD.isActive);
            }
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (uiControl.DEBUG_DRAW_MODE == uiDebugDrawMode.UNITY_RAYCAST_TARGET)// Special mode
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    List<RaycastResult> hits = new List<RaycastResult>();
                    var pointer = new PointerEventData(EventSystem.current);
                    pointer.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    EventSystem.current.RaycastAll(pointer, hits);


                    if (MaterialHelper.mat_line.SetPass(0))
                    {
                        GL.PushMatrix();
                        //DebugUI.DEBUG_LINE_MAT.SetPass(0);
                        GL.Begin(GL.LINES);
                        if (hits.Count > 0)
                        {
                            //PLog.Info("Drawing: {0} {{{1}}}", hits.Count, String.Join(", ", hits.Select(h => String.Format("{0}({1})[p:{2}]", h.gameObject.name, h.gameObject.GetType().Name, h.gameObject.transform.parent.gameObject.name)).ToArray()));
                            foreach (RaycastResult res in hits)
                            {
                                DebugUI.Draw_GameObj_Bounds(res.gameObject);
                            }
                        }


                        if (MainMenu.Instance != null)
                        {
                            Transform MenuPanel = MainMenu.Instance.transform.FindChild("StandardModePanel");
                            if (MenuPanel != null) DebugUI.Draw_GameObj_Bounds(MenuPanel.gameObject);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                }
            }
            else if (uiControl.DEBUG_DRAW_MODE == uiDebugDrawMode.SR_HUD)// Special mode
            {
                if (MaterialHelper.mat_line.SetPass(0))
                {
                    GL.PushMatrix();
                    //DebugUI.DEBUG_LINE_MAT.SetPass(0);
                    GL.Begin(GL.LINES);

                    var hud = GameObject.FindObjectOfType<HudUI>();
                    for (int i = 0; i < hud.transform.childCount; i++)
                    {
                        GameObject gm = hud.transform.GetChild(i).gameObject;
                        DebugUI.Draw_GameObj_Bounds(gm);
                    }

                    GL.End();
                    GL.PopMatrix();
                }
            }
            else if (uiControl.DEBUG_DRAW_MODE != uiDebugDrawMode.NONE)
            {
                var eType = Event.current.rawType;
                if (eType != EventType.Repaint) return;

                if (MaterialHelper.mat_line.SetPass(0))
                {
                    GL.PushMatrix();
                    //DebugUI.DEBUG_LINE_MAT.SetPass(0);
                    GL.Color(Color.white);

                    if (uiControl.DEBUG_DRAW_MODE != uiDebugDrawMode.NONE) uiControl.Draw_Debug_Outlines();

                    GL.PopMatrix();
                }

                uiControl.Debug_Draw_Tooltip();
            }
        }

        private void LateUpdate()
        {
            if (Player.isValid && DebugUI.lbl_player_pos.isVisible) DebugUI.lbl_player_pos.Value = Player.Pos.ToString();
            if (DebugUI.lbl_cam_pos.isVisible) DebugUI.lbl_cam_pos.Value = Camera.main.transform.position.ToString();

            // Figure out which uiControl the mouse is overtop
            uiControl.debug_current_mouse_over = null;
            uiControl.dbg_mouse_tooltip.text = null;

            if (uiControl.DEBUG_DRAW_MODE != uiDebugDrawMode.NONE && Input.mousePresent && DebugUI.ROOT.isVisible)
            {
                Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                IEnumerable<uiControl> list = uiControl.ALL.Select(o => o.Value).Where(o => !o.isChild).Where(o => o != DebugUI.ROOT);// It's for debug functionality screw it, we don't NEED to optimize it.
                //PLog.Info("{0}", String.Join(", ", list.Select(o => o.Name).ToArray()));
                const int MAX_LOOP = 999;// Limit for safety
                int loop = 0;

                while (++loop < MAX_LOOP)
                {
                    bool done = true;
                    // Since we early exit the loop once we find a control the mouse lies within;
                    // we need to counteract the fact that controls are effectively on layers according to their draw order with the last drawn controls on the 
                    // topmost layer and where input events cascade down through the layers from top to bottom.
                    foreach (uiControl c in list.Reverse())
                    {
                        if (c.couldTakeMouseEvent(mousePos))
                        {
                            uiControl.debug_current_mouse_over = c;
                            if (c.isParent)
                            {
                                list = (c as uiPanel).Get_Children();
                                done = false;
                            }
                            break;// exit foreach
                        }
                    }
                    if (done) break;// exit while
                }

                if (uiControl.debug_current_mouse_over != null)
                {
                    uiControl.dbg_mouse_tooltip.text = uiControl.debug_current_mouse_over.FullName;

                    const float tt_width_max = 600;
                    const float mouseHeight = 12;
                    Vector2 sz = uiControl.dbg_mouse_tooltip_style.CalcSize(uiControl.dbg_mouse_tooltip);
                    if (sz.x > tt_width_max) sz.x = tt_width_max;
                    sz.y = uiControl.dbg_mouse_tooltip_style.CalcHeight(uiControl.dbg_mouse_tooltip, sz.x);

                    uiControl.dbg_mouse_tooltop_area.Set(mousePos.x, mousePos.y + sz.y + 3 + mouseHeight, sz.x, sz.y);
                }
            }
        }
        
    }
    
}