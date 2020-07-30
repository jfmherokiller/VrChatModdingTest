using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    /// <summary>
    /// This is a helper class for controlling the game's pause state
    /// Using this helps prevent multiple code breaks if slimerancher alters the location or name of the <see cref="TimeDirector"/> instance
    /// </summary>
    public static class GameTime
    {
        public static TimeDirector Instance { get { return SRSingleton<SceneContext>.Instance.TimeDirector; } }        
        public static bool isPaused { get { return Instance.HasPauser(); } }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public static void Pause()
        {
            Instance.Pause();
        }

        /// <summary>
        /// Unpauses the game.s
        /// </summary>
        public static void Unpause()
        {
            Instance.Unpause();
        }
        
    }
}
