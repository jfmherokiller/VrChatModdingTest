using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Will render the bounds of an objects BoxCollider if it has one
    /// </summary>
    public class Debug_Visualizer : MonoBehaviour
    {
        public Color color = Color.red;
        static Color clr_faint_grey = new Color(0.3f, 0.3f, 0.3f, 1f);
        private List<dGizmo> gizmos = new List<dGizmo>();
        private GameObject GM = null;
        
        void Awake() { Retarget(gameObject); }                
        
        private void Clear_Gizmos()
        {
            foreach (dGizmo gizmo in gizmos) { gizmo.Dispose(); }
            gizmos.Clear();
        }

        public void Untarget()
        {
            Clear_Gizmos();
            GM = null;
        }
        
        public void Retarget(GameObject gm)
        {
            Clear_Gizmos();
            GM = gm;

            float s = 0.001f;
            Bounds bounds = new Bounds(GM.transform.position, new Vector3(s, s, s));

            Bounds? yellow_bounds = null;
            Collider tcol = GM.GetComponent<Collider>();
            if (tcol != null)
            {
                Bounds bnds = tcol.bounds;
                bnds.center -= GM.transform.position;
                dGizmo tcol_gizmo = new dGizmo_BB(bnds, Color.yellow);
                tcol_gizmo.SetParent(GM.transform);
                gizmos.Add(tcol_gizmo);

                yellow_bounds = bnds;
            }

            Collider[] list = GM.transform.GetComponentsInChildren<Collider>();
            if (list.Length > 1)// If we only find 1 collider then it's undoubtedly going to be the same size as the yellow bounding frame we already draw by default and we don't want to draw this one overtop of that...
            {
                foreach (Collider col in list)
                {
                    bounds.Encapsulate(col.bounds);
                    /*
                    Bounds bb = col.bounds;
                    bb.center -= trans.position;
                    boxes.Add(new BBDraw(bb, Color.grey));
                    */
                }

                bounds.center -= GM.transform.position;
                if (!yellow_bounds.HasValue || bounds.HasSameSize(yellow_bounds.Value))// Only show this grey box IF we have no default BB to show OR if our child bounds encompass a greater total area.
                {
                    dGizmo col_gizmo = new dGizmo_BB(bounds, clr_faint_grey);
                    col_gizmo.Bright = false;

                    col_gizmo.SetParent(GM.transform);
                    gizmos.Add(col_gizmo);
                }
            }


            dGizmo vis_gizmo = new dGizmo_BB(GM, color);
            vis_gizmo.SetParent(GM.transform);
            gizmos.Add(vis_gizmo);
        }

        void OnRenderObject()
        {
            Draw();
        }

        public void Draw(Color? clr=null)
        {
            if (GM == null) return;
            Color c = color;
            if (clr.HasValue) c = clr.Value;

            GL.PushMatrix();
            try
            {
                if (MaterialHelper.mat_bright.SetPass(0))
                {
                    GL.MultMatrix(GM.transform.localToWorldMatrix);
                    GL.Begin(GL.LINES);

                    //GL.Color(c);
                    foreach (dGizmo gizmo in gizmos) { gizmo.Draw(); }

                    GL.End();
                }
            }
            catch(Exception ex)
            {
                SLog.Error(ex);
            }
            finally
            {
                GL.PopMatrix();
            }
        }
    }
    
}
