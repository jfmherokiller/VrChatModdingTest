
using System;

namespace SR_PluginLoader
{
    /// <summary>
    /// Provides a simple way to track the active state of something which can be activated/deactivated multiple times and which should only be active if the number of times it was activated & deactivated are equal.
    /// (NOTE: Default state is inactive)
    /// </summary>
    public class Active_State_Tracker
    {
        private string Name = "";
        private int state = 0;
        private bool isActive { get { return (state <= 0); } }
        private bool AllowNegative = false;
        /// <summary>
        /// When set to true, the tracker will log a message whenever it is Activated/Deactivated
        /// </summary>
        public bool Debug = false;

        private void Log(string format, params object[] args)
        {
            string msg = String.Format(format, args);
            SLog.Info("["+nameof(Active_State_Tracker)+"]({0}) {1}", Name, msg);
        }

        public Active_State_Tracker(string name, bool allow_negative_values = false) { Name = name; AllowNegative = allow_negative_values; }
        public Active_State_Tracker(string name, bool defaults_to_active, bool allow_negative_values = false) { Name = name; AllowNegative = allow_negative_values; if (defaults_to_active) { state = 1; } }
        /// <summary>
        /// "Activates" the state and returns <c>True</c> if the state is now active.
        /// </summary>
        /// <returns></returns>
        public bool Activate()
        {
            state--;
            if (state < 0 && !AllowNegative) state = 0;
            if (Debug) Log("Activated | State = {0}", state);
            return isActive;
        }

        /// <summary>
        /// "Deactivates" the state and returns <c>True</c> if the state should still be active.
        /// </summary>
        public bool Deactivate()
        {
            state++;
            if (state < 0 && !AllowNegative) state = 0;
            if (Debug) Log("Deactivated | State = {0}", state);
            return isActive;
        }

        /// <summary>
        /// Completely resets the state so that it's balanced (True) again.
        /// </summary>
        public bool Reset()
        {
            state = 0;
            return isActive;
        }
    }
}
