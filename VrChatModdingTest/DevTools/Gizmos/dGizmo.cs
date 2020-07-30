using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class GizmoLine
    {
        public Color c1 { get; private set; }
        public Color? c2 { get; private set; }
        public Vector3 v1 { get; private set; }
        public Vector3 v2 { get; private set; }

        public GizmoLine() { }
        public GizmoLine(Vector3 a, Vector3 b, Color clrA, Color? clrB=null)
        {
            v1 = a;
            v2 = b;

            c1 = clrA;
            c2 = clrB;
        }
    }
    /// <summary>
    /// Provides a gizmo-like object such as seen in the unity editor.
    /// Each gizmo creates it's own <see cref="GameObject"/> instance that should be parented to another GameObject if the gizmo should display relative to an ingame object.
    /// </summary>
    public abstract class dGizmo : IDisposable
    {
        private Color? color = null;
        internal static List<dGizmo> ALL = new List<dGizmo>();
        #region Variables
        public GameObject gameObject { get; private set; }
        public Transform transform { get { return gameObject.transform; } }
        public GizmoType Type { get; private set; }
        /// <summary>
        /// Do the lines of this gizmo use additive blending (makes them stand out and look like lasers)
        /// </summary>
        public bool Bright = true;
        protected List<GizmoLine> Lines = new List<GizmoLine>();
        onDeathEvent deathScript = null;
        #endregion

        public dGizmo(GizmoType type)
        {
            ALL.Add(this);
            gameObject = new GameObject(Enum.GetName(typeof(GizmoType), type));
            gameObject.AddComponent<onDeathEvent>().onDeath += Dispose;// Just because our gameObject dies doesn't me WE do, but we SHOULD.
        }

        ~dGizmo() { Dispose(); }

        public void Dispose()
        {
            ALL.Remove(this);
            Lines.Clear();
            UnityEngine.Object.Destroy(gameObject);
        }

        /// <summary>
        /// Parents the gizmo to a Transform and links the gizmo to the transform's GameObject such that when the object is destroyed the gizmo will be destroyed also.
        /// </summary>
        public void SetParent(Transform trans)
        {
            gameObject.transform.SetParent(trans, false);
            var gm = trans.gameObject;
            if(gm!=null)
            {
                if (deathScript != null) deathScript.onDeath -= Dispose;
                (deathScript = gm.AddOrGetComponent<onDeathEvent>()).onDeath += Dispose;
            }
        }

        /// <summary>
        /// Links the gizmo to a GameObject such that when said object is destroyed the gizmo wil lalso be destroyed
        /// </summary>
        public void DeathLink(GameObject gm)
        {
            if (deathScript != null) deathScript.onDeath -= Dispose;
            (deathScript = gm.AddOrGetComponent<onDeathEvent>()).onDeath += Dispose;
        }

        public void Render()
        {
            GL.PushMatrix();
            try
            {
                SLog.Assert(gameObject != null, "gameObject is NULL!");
                SLog.Assert(transform != null, "transform is NULL!");
                GL.MultMatrix(transform.localToWorldMatrix);

                switch(Bright)
                {
                    case true:
                        if (MaterialHelper.mat_bright.SetPass(0))
                        {
                            GL.Begin(GL.LINES);
                            Draw();
                            GL.End();
                        }
                        break;
                    case false:
                        if (MaterialHelper.mat_line.SetPass(0))
                        {
                            GL.Begin(GL.LINES);
                            Draw();
                            GL.End();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SLog.Error(ex);
            }
            finally
            {
                GL.PopMatrix();
            }
        }

        public void Draw()
        {
            foreach (GizmoLine line in Lines)
            {
                if(color.HasValue) GL.Color(line.c1 * color.Value);
                else GL.Color(line.c1);
                GL.Vertex(line.v1);

                if (line.c2.HasValue)
                {
                    if (color.HasValue) GL.Color(line.c2.Value * color.Value);
                    else GL.Color(line.c2.Value);
                }
                GL.Vertex(line.v2);
            }
        }


        protected void Add_Line(Vector3 v1, Vector3 v2, Color color)
        {
            Lines.Add(new GizmoLine(v1, v2, color));
        }

        /// <summary>
        /// Adds a line with gradient coloring to the gizmo.
        /// </summary>
        /// <param name="pos">Position of the line</param>
        /// <param name="dir">Direction the line will extend towards</param>
        /// <param name="c1">Color of the line at it's starting point</param>
        /// <param name="c2">Color of the line's end point</param>
        /// <param name="length">Length of the line</param>
        /// <param name="cap_pct">Percentage of the lines total length which the end cap will take up</param>
        /// <param name="fade_pct">Percentage of the lines total length which will be designated to the area of color fading between the lines body color and it's end point color</param>
        protected void Add_Gradient_Line(Vector3 pos, Vector3 dir, Color c1, Color c2, float length, float cap_pct=0.25f, float fade_pct=0.05f)
        {
            float fade_size = Mathf.Max(0f, length * fade_pct);
            float cap_size = Mathf.Max(0.025f, length * cap_pct);

            Vector3 fade_start = pos + (dir * (length - fade_size));
            Vector3 cap_start = fade_start + (dir * fade_size);

            Lines.Add(new GizmoLine(pos, fade_start, c1, c1));
            if(!Util.floatEq(0f, fade_pct)) Lines.Add(new GizmoLine(fade_start, cap_start, c1, c2));
            Lines.Add(new GizmoLine(cap_start, cap_start + (dir * cap_size), c2, c2));
        }

        protected void Add_Box(Vector3 center, Vector3 extents, Color color)
        {
            Vector3 hExtents = extents * 0.5f;

            var v3FrontTopLeft = new Vector3(center.x - hExtents.x, center.y + hExtents.y, center.z - hExtents.z);  // Front top left corner
            var v3FrontTopRight = new Vector3(center.x + hExtents.x, center.y + hExtents.y, center.z - hExtents.z);  // Front top right corner
            var v3FrontBottomLeft = new Vector3(center.x - hExtents.x, center.y - hExtents.y, center.z - hExtents.z);  // Front bottom left corner
            var v3FrontBottomRight = new Vector3(center.x + hExtents.x, center.y - hExtents.y, center.z - hExtents.z);  // Front bottom right corner
            var v3BackTopLeft = new Vector3(center.x - hExtents.x, center.y + hExtents.y, center.z + hExtents.z);  // Back top left corner
            var v3BackTopRight = new Vector3(center.x + hExtents.x, center.y + hExtents.y, center.z + hExtents.z);  // Back top right corner
            var v3BackBottomLeft = new Vector3(center.x - hExtents.x, center.y - hExtents.y, center.z + hExtents.z);  // Back bottom left corner
            var v3BackBottomRight = new Vector3(center.x + hExtents.x, center.y - hExtents.y, center.z + hExtents.z);  // Back bottom right corner


            Add_Line(v3FrontTopLeft, v3FrontTopRight, color);
            Add_Line(v3FrontTopRight, v3FrontBottomRight, color);
            Add_Line(v3FrontBottomRight, v3FrontBottomLeft, color);
            Add_Line(v3FrontBottomLeft, v3FrontTopLeft, color);

            Add_Line(v3BackTopLeft, v3BackTopRight, color);
            Add_Line(v3BackTopRight, v3BackBottomRight, color);
            Add_Line(v3BackBottomRight, v3BackBottomLeft, color);
            Add_Line(v3BackBottomLeft, v3BackTopLeft, color);

            Add_Line(v3FrontTopLeft, v3BackTopLeft, color);
            Add_Line(v3FrontTopRight, v3BackTopRight, color);
            Add_Line(v3FrontBottomRight, v3BackBottomRight, color);
            Add_Line(v3FrontBottomLeft, v3BackBottomLeft, color);

        }

        /// <summary>
        /// Creates a trapezoid shaped box where the front and back sides are different sizes
        /// </summary>
        /// <param name="hLength">Length of the shape between it's starting side and ending side</param>
        protected void Add_Box_Trapezoid(Vector3 center, float length, Vector2 start_size, Vector2 end_size, Color color)
        {
            float hLength = length * 0.5f;
            Vector3 hStart_Size = start_size * 0.5f;
            var v3FrontTopLeft = new Vector3(center.x - hStart_Size.x, center.y + hStart_Size.y, center.z - hLength);  // Front top left corner
            var v3FrontTopRight = new Vector3(center.x + hStart_Size.x, center.y + hStart_Size.y, center.z - hLength);  // Front top right corner
            var v3FrontBottomLeft = new Vector3(center.x - hStart_Size.x, center.y - hStart_Size.y, center.z - hLength);  // Front bottom left corner
            var v3FrontBottomRight = new Vector3(center.x + hStart_Size.x, center.y - hStart_Size.y, center.z - hLength);  // Front bottom right corner

            Vector3 hEnd_Size = end_size * 0.5f;
            var v3BackTopLeft = new Vector3(center.x - hEnd_Size.x, center.y + hEnd_Size.y, center.z + hLength);  // Back top left corner
            var v3BackTopRight = new Vector3(center.x + hEnd_Size.x, center.y + hEnd_Size.y, center.z + hLength);  // Back top right corner
            var v3BackBottomLeft = new Vector3(center.x - hEnd_Size.x, center.y - hEnd_Size.y, center.z + hLength);  // Back bottom left corner
            var v3BackBottomRight = new Vector3(center.x + hEnd_Size.x, center.y - hEnd_Size.y, center.z + hLength);  // Back bottom right corner


            Add_Line(v3FrontTopLeft, v3FrontTopRight, color);
            Add_Line(v3FrontTopRight, v3FrontBottomRight, color);
            Add_Line(v3FrontBottomRight, v3FrontBottomLeft, color);
            Add_Line(v3FrontBottomLeft, v3FrontTopLeft, color);

            Add_Line(v3BackTopLeft, v3BackTopRight, color);
            Add_Line(v3BackTopRight, v3BackBottomRight, color);
            Add_Line(v3BackBottomRight, v3BackBottomLeft, color);
            Add_Line(v3BackBottomLeft, v3BackTopLeft, color);

            Add_Line(v3FrontTopLeft, v3BackTopLeft, color);
            Add_Line(v3FrontTopRight, v3BackTopRight, color);
            Add_Line(v3FrontBottomRight, v3BackBottomRight, color);
            Add_Line(v3FrontBottomLeft, v3BackBottomLeft, color);

        }

        protected void Add_Cross(Vector3 origin, float size, Color color)
        {
            Lines.Add(new GizmoLine((Vector3.up * -size) + origin, (Vector3.up * size) + origin, color));
            Lines.Add(new GizmoLine((Vector3.right * -size) + origin, (Vector3.right * size) + origin, color));
            Lines.Add(new GizmoLine((Vector3.forward * -size) + origin, (Vector3.forward * size) + origin, color));
        }

        protected void Add_Star(Vector3 origin, float size, Color color)
        {
            const float zScale = 0.4f;
            Lines.Add(new GizmoLine((Vector3.up * -size * zScale) + origin, (Vector3.up * size * zScale) + origin, color));
            Lines.Add(new GizmoLine((Vector3.right * -size) + origin, (Vector3.right * size) + origin, color));
            Lines.Add(new GizmoLine(origin, (Vector3.forward * size) + origin, color));
        }
    }
    
}
