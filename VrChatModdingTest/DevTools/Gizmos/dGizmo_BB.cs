using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class dGizmo_BB : dGizmo
    {
        Color color;
        #region Verticies

        private Vector3 v3FrontTopLeft;
        private Vector3 v3FrontTopRight;
        private Vector3 v3FrontBottomLeft;
        private Vector3 v3FrontBottomRight;
        private Vector3 v3BackTopLeft;
        private Vector3 v3BackTopRight;
        private Vector3 v3BackBottomLeft;
        private Vector3 v3BackBottomRight;
        #endregion


        public dGizmo_BB(GameObject gm, Color color) : base(GizmoType.BOUNDING_BOX)
        {
            this.color = color;
            Bounds bounds = new Bounds(gm.transform.position, Vector3.one);

            MeshRenderer[] list = gm.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in list) { bounds.Encapsulate(mr.bounds); }

            bounds.center -= gm.transform.position;

            Build(bounds);
        }

        public dGizmo_BB(Bounds bounds, Color color) : base(GizmoType.BOUNDING_BOX)
        {
            this.color = color;
            Build(bounds);
        }

        void Build(Bounds bounds)
        {
            Vector3 v3Center = bounds.center;
            Vector3 v3Extents = bounds.extents;

            Add_Box(v3Center, v3Extents, color);

            const float CROSS_SIZE = 0.15f;
            Color clr = new Color(1f, 0.3f, 0.3f, 0.5f);
            Add_Cross(v3Center, CROSS_SIZE, clr);
        }

        private void BuildLine(Vector3 v1, Vector3 v2)
        {
            Lines.Add(new GizmoLine(v1, v2, color));
        }
    }
}
