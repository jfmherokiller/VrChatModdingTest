using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Attach to a camera to allow viewing dGizmo objects.
    /// </summary>
    public class Camera_dGizmo_Renderer : MonoBehaviour
    {
        void OnPostRender()
        {
            foreach (dGizmo gizmo in dGizmo.ALL)
            {
                gizmo.Render();
            }
        }
    }
}
