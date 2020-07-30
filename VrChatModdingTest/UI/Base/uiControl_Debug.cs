using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{

    public enum uiDebugDrawMode
    {
        /// <summary>
        /// Just don't
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Shows the gameobject currently intercepting the unity GUI raycast event.
        /// </summary>
        UNITY_RAYCAST_TARGET,
        /// <summary>
        /// Shows all of the hud elements for SlimeRancher
        /// </summary>
        SR_HUD,
        SELECTED_ONLY,
        /// <summary>
        /// Only active controls will be outlined.
        /// </summary>
        ACTIVE_ONLY,
        BORDER_AREAS,
        DRAW_AREAS,
        INNER_AREAS,
        /// <summary>
        /// Active controls will be outlined with white, while inactive ones are drawn semi-transparent.
        /// </summary>
        ACTIVE_AND_INACTIVE,
        /// <summary>
        /// Parent type controls will be drawn with an outline.
        /// </summary>
        HIGHLIGHT_PARENTS,
        /// <summary>
        /// Parent type controls will be drawn with a red outline and their inner areas wil lalso be outlined in blue.
        /// </summary>
        HIGHLIGHT_PARENT_INNER_AREAS,
    }

    public abstract partial class uiControl
    {
        #region DEBUG DRAWING
        internal const float trClr_G = 0.5f;
        internal const float TRANS_ALPHA = 0.6f;

        #region COLORS

        internal static Color white_trans = new Color(1f, 1f, 1f, TRANS_ALPHA);
        internal static Color red_trans = new Color(1f, 0f, 0f, TRANS_ALPHA);
        internal static Color green_trans = new Color(0f, 1f, 0f, TRANS_ALPHA);
        internal static Color blue_trans = new Color(0f, 0f, 1f, TRANS_ALPHA);
        internal static Color yellow_trans = new Color(1f, 1f, 0f, TRANS_ALPHA);
        internal static Color purple_trans = new Color(1f, 0f, 1f, TRANS_ALPHA);

        internal static Color red_dark = new Color(trClr_G, 0f, 0f, 1f);
        internal static Color blue_dark = new Color(0f, 0f, trClr_G, 1f);
        internal static Color white_dark = new Color(trClr_G, trClr_G, trClr_G, 1f);
        internal static Color grey_dark = new Color(0.5f * trClr_G, 0.5f * trClr_G, 0.5f * trClr_G, 1f);

        internal static Color color_cyan = new Color(0f, 1f, 1f, 1f);
        internal static Color color_orange = new Color(1f, 0.5f, 0f, 1f);
        internal static Color color_purple = new Color(1f, 0f, 1f, 1f);
        #endregion

        #region DEBUG VARS

        internal static GUIContent dbg_mouse_tooltip = new GUIContent(GUIContent.none);
        internal static Rect dbg_mouse_tooltop_area = new Rect();
        internal static GUIStyle dbg_mouse_tooltip_style = null;
        #endregion

        internal static void Draw_Debug_Outlines()
        {
            GL.Begin(GL.LINES);
            foreach (uiControl control in uiControl.ALL.Values)
            {
                Rect area = control.absArea;
                bool DrawInner = false;
                if(DEBUG_DRAW_MODE == uiDebugDrawMode.SELECTED_ONLY)
                {
                    if (control != uiControl.debug_current_mouse_over) continue;
                    // Draw all areas of the selected control

                    GL.Color(blue_trans);
                    DebugUI.Draw_Rect(control.internalToAbsolute(control.inner_area));

                    GL.Color(purple_trans);
                    DebugUI.Draw_Rect(control.internalToAbsolute(control.draw_area));

                    GL.Color(white_trans);
                    //DebugUI.Draw_Rect(control.absArea);
                    DebugUI.Draw_Rect(control.internalToAbsolute(control.Area));
                    /*
                    GL.Color(yellow_trans);
                    DebugUI.Draw_Rect(control.internalToAbsolute(control.content_area));
                    */
                    GL.Color(Color.clear);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.ACTIVE_ONLY)
                {
                    if (!control.isVisible) continue;

                    GL.Color(white_trans);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.BORDER_AREAS)
                {
                    if (!control.isVisible) continue;

                    GL.Color(yellow_trans);
                    area = control.internalToAbsolute(control.border_area);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.DRAW_AREAS)
                {
                    if (!control.isVisible) continue;

                    GL.Color(purple_trans);
                    area = control.internalToAbsolute(control.draw_area);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.INNER_AREAS)
                {
                    if (!control.isVisible) continue;

                    GL.Color(blue_trans);
                    area = control.internalToAbsolute(control.inner_area);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.ACTIVE_AND_INACTIVE)
                {
                    if (control.isVisible) GL.Color(Color.white);
                    else GL.Color(white_dark);
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.HIGHLIGHT_PARENTS)
                {
                    if (control.isParent)
                    {
                        if (control.isVisible) GL.Color(Color.white);
                        else GL.Color(white_dark);
                    }
                    else continue;
                }
                else if (DEBUG_DRAW_MODE == uiDebugDrawMode.HIGHLIGHT_PARENT_INNER_AREAS)
                {
                    if (control.isParent)
                    {
                        DrawInner = true;
                        if (control.isVisible) GL.Color(Color.red);
                        else GL.Color(red_dark);
                    }
                    else continue;
                }
                
                if (area.x > area.xMax || area.y > area.yMax) GL.Color(grey_dark);
                DebugUI.Draw_Rect(area);

                if (DrawInner)
                {
                    if (control.isVisible) GL.Color(Color.blue);
                    else GL.Color(blue_dark);

                    if (control.absInnerArea.x > control.absInnerArea.xMax || control.absInnerArea.y > control.absInnerArea.yMax) GL.Color(Color.black);
                    DebugUI.Draw_Rect(control.absInnerArea);
                }
            }

            if (uiControl.debug_last_consumer_click != null)
            {
                GL.Color(Color.green);
                DebugUI.Draw_Rect(uiControl.debug_last_consumer_click.absArea);
            }

            if(uiControl.debug_current_mouse_over != null && DEBUG_DRAW_MODE != uiDebugDrawMode.SELECTED_ONLY)
            {
                GL.Color(Color.yellow);
                DebugUI.Draw_Rect(uiControl.debug_current_mouse_over.absArea);
            }

            GL.End();
            GL.Color(Color.white);// Always set the GL color back to white before returning from any debug drawing routine!
        }

        internal static void Debug_Draw_Tooltip()
        {
            if (uiControl.dbg_mouse_tooltip_style == null) throw new ArgumentNullException("uiControl.dbg_mouse_tooltip_style is NULL!");
            if(uiControl.dbg_mouse_tooltip!=null && uiControl.dbg_mouse_tooltip.text!=null)
            {
                uiControl.dbg_mouse_tooltip_style.Draw(uiControl.dbg_mouse_tooltop_area, uiControl.dbg_mouse_tooltip, 0);
            }
        }
        #endregion

    }
}
