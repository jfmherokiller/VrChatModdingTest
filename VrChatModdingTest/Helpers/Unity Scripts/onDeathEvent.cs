using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// When this script is destroyed it will fire an event.
    /// </summary>
    public class onDeathEvent : MonoBehaviour
    {
        public event Action onDeath;
        void OnDestroy()
        {
            onDeath?.Invoke();
        }
    }
}
