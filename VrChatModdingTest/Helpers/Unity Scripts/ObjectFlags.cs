using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    internal class flag_lifetime
    {
        public bool Expires;
        public float DeathTime;

        public flag_lifetime() { Expires = false; }
        public flag_lifetime(float life) { Expires = true; DeathTime = (GameTime.Instance.WorldTime() + life); }
    }
    /// <summary>
    /// When attached to a <c>GameObject</c> this script will allow "flags" to be set for the object in the form of text strings.
    /// These flags can then be checked for later to alter program behaviour.
    /// Flags can have an optional lifetime set, after which they will expire and be removed from the object automatically.
    /// 
    /// </summary>
    class ObjectFlags : MonoBehaviour
    {
        private Dictionary<string, flag_lifetime> Flags = new Dictionary<string, flag_lifetime>();
        public bool HasFlag(string flag) { return Flags.ContainsKey(flag); }

        public void SetFlag(string flag) { Flags.Add(flag, new flag_lifetime()); }
        public void SetFlag(string flag, float lifetime) { Flags.Add(flag, new flag_lifetime(lifetime)); }

        private void Update()
        {
            var expired = Flags.Where(kvp => (kvp.Value!=null && kvp.Value.Expires && Time.time > kvp.Value.DeathTime));

            foreach(KeyValuePair<string, flag_lifetime> kvp in expired)
            {
                Flags.Remove(kvp.Key);
            }
        }
    }
}
