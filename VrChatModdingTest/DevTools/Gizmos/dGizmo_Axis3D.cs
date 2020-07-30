using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// A gizmo that displays a traditional red/green/blue XYZ origin axis.
    /// </summary>
    public class dGizmo_Axis3D : dGizmo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">The length of each axis arm</param>
        public dGizmo_Axis3D(float size = 0.5f) : base(GizmoType.XYZ_AXIS_DISPLAY)
        {
            Lines.Add(new GizmoLine(Vector3.zero, Vector3.right * size, Color.red));// X axis
            Lines.Add(new GizmoLine(Vector3.zero, Vector3.up * size, Color.green));// Y axis
            Lines.Add(new GizmoLine(Vector3.zero, Vector3.forward * size, Color.blue));// Z axis
        }
    }
}
