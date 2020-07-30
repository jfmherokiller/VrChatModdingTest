
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Draws a line that points in a vector direction indicating the direction with a colored tip
    /// </summary>
    public class dGizmo_Vec : dGizmo
    {
        public dGizmo_Vec(Vector3 origin, Vector3 direction, float length = 0.5f) : base(GizmoType.VECTOR)
        {
            Add_Gradient_Line(origin, direction, Color.white, Color.red, length);
        }

        public dGizmo_Vec(Transform transform, float length = 0.5f) : this(transform.position, transform.forward, length) { }
    }
}
