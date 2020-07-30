using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public static class Directors
    {
        public static LookupDirector lookupDirector { get { return SRSingleton<GameContext>.Instance.LookupDirector; } }
        public static MessageDirector messageDirector { get { return SRSingleton<GameContext>.Instance.MessageDirector; } }
        public static OptionsDirector optionsDirector { get { return SRSingleton<GameContext>.Instance.OptionsDirector; } }
        public static EconomyDirector economyDirector { get { return SRSingleton<SceneContext>.Instance.EconomyDirector; } }
        public static AutoSaveDirector autosaveDirector { get { return SRSingleton<GameContext>.Instance.AutoSaveDirector; } }

        public static TimeDirector timeDirector { get { return SRSingleton<SceneContext>.Instance.TimeDirector; } }

        public static Overlay overlay { get { return SRSingleton<Overlay>.Instance; } }
    }
}
