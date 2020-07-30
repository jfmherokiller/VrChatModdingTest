using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Adds an additional <see cref="BoxCollider"/> to a gameObject an sizes it to encompass it's parent object and all of it's children.
    /// Useful for raycasting and selecting complex models.
    /// </summary>
    public class SelectableCollider : MonoBehaviour
    {
        private int lastChildCount = 0;
        private BoxCollider bx = null;

        void UpdateBounds()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.one);

            Collider[] list = transform.GetComponentsInChildren<Collider>();
            foreach (Collider col in list) { bounds.Encapsulate(col.bounds); }

            if (bx == null)
            {
                bx = base.gameObject.AddComponent<BoxCollider>();
                bx.isTrigger = true;
                //bx.tag = "SelectableCollider";
            }

            bx.center = Vector3.zero;
            bx.extents = bounds.extents;
        }

        void Start() { UpdateBounds(); }

        void Update()
        {
            if(lastChildCount != base.transform.childCount)
            {
                lastChildCount = transform.childCount;
                UpdateBounds();
            }
        }
    }
}
