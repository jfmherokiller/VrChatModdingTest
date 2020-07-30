using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// Helps uniquely identify an individually registered <see cref="HOOK_ID"/>.
    /// If two hooks are registered for the same event and each have the same exact callback delegate, they will both be given two different <see cref="HookUID"/>'s.
    /// </summary>
    public class HookUID
    {
        /// <summary>
        /// Tracks the value which was given to the last <see cref="HookUID"/> to be instantiated.
        /// </summary>
        private static int LastID = 1;
        /// <summary>
        /// The unique identifying value of this instance.
        /// </summary>
        internal int uid;
        /// <summary>
        /// The Assembly that registered this hook.
        /// </summary>
        internal object registrar;
        /// <summary>
        /// Which event this hook is for.
        /// </summary>
        internal HOOK_ID hook;
        /// <summary>
        /// Does this hook only fire a single time before being auto removed?
        /// </summary>
        internal bool SingleUse = false;
        /// <summary>
        /// Has this hook been called yet?
        /// </summary>
        internal bool hasFired = false;


        public HookUID(HOOK_ID evt, object reg, bool once=false)
        {
            uid = LastID++;
            hook = evt;
            registrar = reg;
            SingleUse = once;
        }

        /// <summary>
        /// Copies all of the relevant data from one instance into another.
        /// </summary>
        /// <param name="h"></param>
        public HookUID(HookUID h)
        {
            uid = h.uid;
            hook = h.hook;
            registrar = h.registrar;
        }


        public override string ToString() { return String.Format("HookUID<{0}>[{1}]", uid, hook); }
        public override int GetHashCode() { return uid; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Type oty = obj.GetType();
            if (typeof(HookUID) == oty) return (uid == (obj as HookUID).uid);
            if (typeof(int) == oty) return (uid == (int)obj);

            return false;
        }

        static public bool operator ==(HookUID A, HookUID B) { return A.Equals(B); }
        static public bool operator !=(HookUID A, HookUID B) { return !A.Equals(B); }

    }
}
