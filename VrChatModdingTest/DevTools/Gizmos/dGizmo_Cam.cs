using UnityEngine;

namespace SR_PluginLoader
{
    public class dGizmo_Cam : dGizmo
    {
        public dGizmo_Cam() : base(GizmoType.CAMERA)
        {
            Add_Star(Vector3.zero, 0.025f, Color.red);
            const float width = 0.2f;
            const float length = 0.3f;
            const float height = 0.15f;
            
            Vector3 bxOrigin = -Vector3.forward * (length * 0.5f);
            Add_Box(bxOrigin, new Vector3(width, height, length), Color.white);

            const float lens_length = 0.1f;
            const float lens_start_width = (width - 0.1f);
            const float lens_start_height = (height - 0.1f);
            const float lens_end_width = (width - 0.05f);
            const float lens_end_height = (height - 0.05f);
            Add_Box_Trapezoid(Vector3.forward*(lens_length*0.5f), lens_length, new Vector2(lens_start_width, lens_start_height), new Vector2(lens_end_width, lens_end_height), Color.white);
        }

        public dGizmo_Cam(Camera cam) : this()
        {
            SetParent(cam.transform);
        }
    }
}
