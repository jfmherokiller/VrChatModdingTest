using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Helper class for playing common sounds ingame.
    /// </summary>
    public static class Sound
    {
        private static Dictionary<SoundId, Func<SECTR_AudioCue>> SOUND_MAP = new Dictionary<SoundId, Func<SECTR_AudioCue>>()
        {
            { SoundId.PURCHASED_PLOT, () => { return SRSingleton<GameContext>.Instance.UITemplates.purchasePlotCue; } },
            { SoundId.PURCHASED_PLOT_UPGRADE, () => { return SRSingleton<GameContext>.Instance.UITemplates.purchaseUpgradeCue; } },
            { SoundId.PURCHASED_PLAYER_UPGRADE, () => { return SRSingleton<GameContext>.Instance.UITemplates.purchasePersonalUpgradeCue; } },

            { SoundId.ERROR, () => { return SRSingleton<GameContext>.Instance.UITemplates.errorCue; } },
            { SoundId.NEGATIVE, () => { return SRSingleton<GameContext>.Instance.UITemplates.errorCue; } },
            { SoundId.BTN_CLICK, () => { return SRSingleton<GameContext>.Instance.UITemplates.clickCue; } },
            { SoundId.POSITIVE, () => { return SRSingleton<GameContext>.Instance.UITemplates.purchasePersonalUpgradeCue; } },
        };


        public static void Play(SoundId snd)
        {
            Sound.Play(snd, Vector3.zero);
        }

        public static void Play(SoundId snd, Vector3 pos)
        {
            //PLog.Info("[Sound] Playing: SoundId.{0}", snd.ToString());
            Func<SECTR_AudioCue> cue;
            if (SOUND_MAP.TryGetValue(snd, out cue))
            {
                SECTR_AudioSystem.Play(cue(), pos, false);
            }
            else
            {
                SLog.Warn("[Sound] No sound listed for SoundId.{0}", snd.ToString());
            }
        }


        /// <summary>
        /// Plays a sound and sets a timed flag on a given object.
        /// This is useful for playing a sound in response to an object.
        /// Example:
        /// The player fires an object into a garden input that isnt a valid fruit/vegetable.
        /// The kiosk can use this function to play a negative sound for the item without it playing like 10 times due to the item still being in range of the input area!
        /// </summary>
        /// <param name="snd">The sound to play</param>
        /// <param name="obj">The GameObject to lock the sound so it doesn't play more then once.</param>
        /// <param name="lockSecs">Number of seconds before the sound can play again.</param>                   
        public static void PlayOnce(SoundId snd, GameObject obj=null, float lockSecs=1.0f)
        {
            ObjectFlags locker = obj.GetComponent<ObjectFlags>();
            if (locker == null) locker = obj.AddComponent<ObjectFlags>();
            string flag = String.Concat("soundlock_", snd.ToString());

            if (!locker.HasFlag(flag))
            {
                locker.SetFlag(flag, lockSecs);
                Sound.Play(snd);
            }
        }

    }


    public class SoundId : SafeEnum
    {
        public static SoundId BTN_CLICK = new SoundId();
        public static SoundId NEGATIVE = new SoundId();
        public static SoundId POSITIVE = new SoundId();
        public static SoundId ERROR = new SoundId();

        public static SoundId PURCHASED_PLOT = new SoundId();
        public static SoundId PURCHASED_PLOT_UPGRADE = new SoundId();
        public static SoundId PURCHASED_PLAYER_UPGRADE = new SoundId();
    }
}
