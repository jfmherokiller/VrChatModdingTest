using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Provides helpful extensions to Unity's GameObject class
    /// </summary>
    public static class GameObjectExt
    {
        /// <summary>
        /// Returns the GameObjects instance of a specified script, adding a new instance if none exists.
        /// </summary>
        public static Script AddOrGetComponent<Script>(this GameObject obj) where Script : Component
        {
            Script s = obj.GetComponent<Script>();
            if (s == null) s = obj.AddComponent<Script>();
            return s;
        }

        public static Script AddOnce<Script>(this GameObject obj) where Script : Component
        {
            Script s = obj.GetComponent<Script>();
            if (s == null) s = obj.AddComponent<Script>();
            return s;
        }
    }
}
