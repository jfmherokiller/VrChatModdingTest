using SR_PluginLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SlimeTool
{
    public class WorldText: MonoBehaviour
    {
        private TextMesh txt = null;
        public TextMesh textObj { get { return txt; } }
        private float last_alpha = 1f;
        public float MIN_ALPHA = 0.4f;
        /// <summary>
        /// The text to display. (Shortcut for <see cref="textObj.text"/>)
        /// </summary>
        public string Text { get { return txt.text; } set { txt.text = value; } }
        public Color Color { get { return txt.color; } set { txt.color = value; } }


        private void OnRenderObject()
        {
            Camera main = Camera.main;
            //base.transform.LookAt(main.transform, main.transform.rotation * Vector3.up);
            base.transform.LookAt(base.transform.position + main.transform.rotation * Vector3.forward, main.transform.rotation * Vector3.up);
        }

        // Fade the text depending on how much we are looking toward it
        private void Update()
        {
            Vector3 d = (transform.position - Camera.main.transform.position);
            Vector3 dir = new Vector2(d.x, d.z).normalized;
            Vector2 vdir = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z).normalized;
            float dot = Vector2.Dot(dir, vdir);

            if (dot < 0f) return;// The text is offscreen, don't even bother altering any of it's properties. waste of resources.
            dot = (float)Math.Pow(dot, 6f);
            dot = Math.Max(MIN_ALPHA, dot);
            //SLog.Info("DOT: {0}", dot);

            float delta = Math.Abs(dot - last_alpha);
            if (delta < 0.001f) return;

            last_alpha = dot;
            var renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.material.SetColor("_Color", new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, dot));
        }

        private void Start()
        {
            txt = base.gameObject.AddComponent<TextMesh>();
            txt.fontSize = 64;
            txt.characterSize = 0.02f;
            txt.anchor = TextAnchor.MiddleCenter;

            //var renderer = gameObject.GetComponent<MeshRenderer>();
            //renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        }
    }
}
