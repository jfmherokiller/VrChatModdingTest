using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public class FollowPlayerSight : MonoBehaviour
    {
        /// <summary>
        /// Should th eobject also render it's current position on the hud?
        /// </summary>
        public bool show_pos = false;
        private Rect text_rect = new Rect(0, Screen.height/2, 500f, 100f);
        private GUIContent text_content = new GUIContent();

        private void Update()
        {
            Vector3? pos = Player.RaycastPos();
            if(pos.HasValue)
            {
                base.gameObject.transform.position = pos.Value;
                MeshRenderer rend = base.gameObject.GetComponent<MeshRenderer>();

                text_content.text = String.Format("{0}\nVisible: {1}", pos.Value.ToString(), rend.isVisible?"True":"False");
            }
        }

        private void OnGUI()
        {
            if (!show_pos) return;
            GUI.Label(text_rect, text_content);
        }
    }
}
