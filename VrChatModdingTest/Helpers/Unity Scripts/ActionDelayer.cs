using System;
using UnityEngine;

namespace SR_PluginLoader
{
    public class ActionDelayer : MonoBehaviour
    {
        public Action onStart = null;
        public Action onFirstUpdate = null;
        public Action onDestroyed = null;
        private bool ALL_EVENTS_HANDLED { get { return (onStart==null && onFirstUpdate==null && onDestroyed==null); } }
        /// <summary>
        /// When this script destroys itself do we also destroy the gameobject?
        /// </summary>
        private bool destroy_gameobject = false;

        /// <summary>
        /// Tells the script to destroy it's GameObject when it destroys itself.
        /// </summary>
        public ActionDelayer SelfDestruct() { destroy_gameobject = true; UnityEngine.GameObject.DontDestroyOnLoad(this.gameObject); return this; }

        private void Start()
        {
            onStart?.Invoke();
            onStart = null;
        }

        private void onDestroy()
        {
            onDestroyed?.Invoke();
        }

        private void Update()
        {
            onFirstUpdate?.Invoke();
            onFirstUpdate = null;

            // Check if all of our events are handled yet, if they are then we can destroy ourself
            if (ALL_EVENTS_HANDLED)
            {
                if (destroy_gameobject) GameObject.Destroy(this);
                else Destroy(this);
            }
        }
    }
}
