using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Provides methods to alter player state and obtain player info.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Is the camera in free-fly mode?
        /// </summary>
        public static bool isFlying { get { return DevCamera.FLYING; } }
        private static PlayerState player { get { return SRSingleton<SceneContext>.Instance.PlayerState; } }
        public static vp_FPInput Input { get { return gameObject.GetComponentInChildren<vp_FPInput>(); } }

        public static GameObject gameObject { get { return SRSingleton<SceneContext>.Instance.Player; } }
        public static PlayerState state { get { return player; } }
        public static WeaponVacuum Weapon { get { if(gameObject==null) { return null; } return gameObject.GetComponentInChildren<WeaponVacuum>(); } }
        public static EnergyJetpack Jetpack { get { if (gameObject == null) { return null; } return gameObject.GetComponent<EnergyJetpack>(); } }

        #region Getters / Setters

        /// <summary>
        /// Returns <c>True</c> if the Player has a valid <c>GameObject</c>, <c>False</c> otherwise.
        /// </summary>
        public static bool isValid { get { return (gameObject!=null); } }

        /// <summary>
        /// The players current health. To alter health <see cref="Player.Damage(int)"/>
        /// </summary>
        public static int Health { get { return player.GetCurrHealth(); } set { player.SetHealth(value); } }

        /// <summary>
        /// The players current energy level. To alter energy level <see cref=""/>
        /// </summary>
        public static int Energy { get { return player.GetCurrEnergy(); } set { player.SetEnergy(value); } }

        /// <summary>
        /// The players current radiation level. To alter radiation level <see cref=""/>
        /// </summary>
        public static int Rads { get { return player.GetCurrRad(); } set { player.SetRad(value); } }

        /// <summary>
        /// How much currency the player has. To add or remove currency <see cref="Player.SpendCurrency(int, bool)"/>
        /// </summary>
        public static int Currency { get { return player.GetCurrency(); } }

        /// <summary>
        /// How many keys the player has. <seealso cref="Player.AddKeys(int)"/> <seealso cref="Player.SpendKeys(int)"/>
        /// </summary>
        public static int Keys { get { return player.GetKeys(); } set { player.SetKeys(value); } }

        /// <summary>
        /// Player's position within the game world.
        /// </summary>
        public static Vector3 Pos { get { return gameObject.transform.position; } set { gameObject.transform.position.Set(value.x, value.y, value.z); } }

#if !SR_VANILLA
        public static int MaxHealth { get { return player.maxHealth; } set { player.maxHealth = value; Health = Health; } }
        public static int MaxEnergy { get { return player.maxEnergy; } set { player.maxEnergy = value; Energy = Energy; } }
        public static int MaxRads { get { return player.maxRads; } set { player.maxRads = value; Rads = Rads; } }
        public static int MaxAmmo { get { return player.maxAmmo; } set { player.maxAmmo = value; } }
#else
        public static int MaxHealth, MaxEnergy, MaxRads, MaxAmmo;// When compiling for a vanilla assembly without the extra variable addons we cant actually manipulate any of these values, but we cant leave them out of the build either, so we just make them ints because we will be recompiling in Release mode immediately after this build is done anyway...
#endif
        #endregion

        #region Inventory Helpers

        /// <summary>
        /// Returns the number of a certain item that the player has in their inventory.
        /// </summary>
        /// <returns></returns>
        public static int Get_Inv_Item_Count(Identifiable.Id id)
        {
#if !SR_VANILLA
            for (int s = 0; s< player.Ammo.slotCount; s++)
            {
                Identifiable.Id sid = player.Ammo.GetSlotName(s);
                if(sid == id)
                {
                    return player.Ammo.GetSlotCount(s);
                }
            }
#endif
            return 0;
        }
#endregion

        #region VacPak Helpers

        /// <summary>
        /// Returns an array of GameObject which are currently being sucked in by the players weapon.
        /// </summary>
        /// <returns></returns>
        public static List<Identifiable> Get_Captive_Items()
        {
            List<Identifiable> ret = new List<Identifiable>();
#if !SR_VANILLA
            foreach (Joint joint in Player.Weapon.Get_Joints())
            {
                if (joint == null || joint.connectedBody == null) continue;

                Identifiable ident = joint.connectedBody.GetComponent<Identifiable>();
                if (ident != null)
                {
                    ret.Add(ident);
                }
            }
#endif
            return ret;
        }
        
        /// <summary>
        /// Returns the number of objects which are currently being sucked in by the players weapon.
        /// </summary>
        /// <param name="id">The type of object to count</param>
        /// <returns></returns>
        public static int Get_Captive_Item_Count(Identifiable.Id id)
        {
            return Get_Captive_Items().Count(o => o.id == id);
        }

        #endregion

        #region Upgrade Helpers

        public static void GiveUpgrade(PlayerUpgrade upgrade)
        {
            Upgrades.PlayerUpgrades.Add(upgrade);
            upgrade.Apply(player.gameObject);
        }

        public static bool HasUpgrade(string ID) { return (Upgrades.PlayerUpgrades.Exists(u => (String.Compare(u.ID, ID)==0)) || Upgrades.Player_Upgrades_Missing.Exists(o => (String.Compare(o, ID)==0))); }
        public static bool HasUpgrade(IUpgrade upgrade) { return HasUpgrade(upgrade.ID); }
        public static bool HasUpgrade(PlayerState.Upgrade upgrade) { return player.HasUpgrade(upgrade); }

        public static bool CanBuyUpgrade(string ID)
        {
            PlayerUpgrade up = (PlayerUpgrade)Upgrades.Get_Upgrade(Upgrade_Type.PLAYER_UPGRADE, ID);
            if (up == null) return false;

            return CanBuyUpgrade(up);
        }

        public static bool CanBuyUpgrade(PlayerUpgrade upgrade)
        {
            return upgrade.CanBuy(null);
        }

        public static bool CanBuyUpgrade(PlayerState.Upgrade upgrade) { return player.CanGetUpgrade(upgrade); }
    #endregion
        
        #region Misc

        public static void Damage(int dmg) { player.Damage(dmg); }
        public static void AddRads(float rads) { player.AddRads(rads); }

        public static void SpendEnergy(float energy) { player.SpendEnergy(energy); }
        public static void SpendCurrency(int adjust, bool forcedLoss = false) { player.SpendCurrency(adjust, forcedLoss); }

        public static void AddKeys(int num = 1) { for (int i = 0; i < num; i++) { player.AddKey(); } }
        public static bool SpendKeys(int num = 1) {
            if (Player.Keys < num) return false;
            for (int i = 0; i < num; i++) { player.SpendKey(); }
            return true;
        }

        /// <summary>
        /// Moves the player to a specified position and makes them face a given direction.
        /// </summary>
        public static void Teleport(Vector3 pos, Vector3? rot, bool playFX=false)
        {
            if(Weapon!=null) Weapon.DropAllVacced();
            vp_FPPlayerEventHandler vp_evt = gameObject.GetComponentInChildren<vp_FPPlayerEventHandler>();
            if (vp_evt != null)
            {
                vp_evt.Position.Set(pos);
                if(rot.HasValue) vp_evt.Rotation.Set(rot.Value);
            }

            if (playFX) Directors.overlay.PlayTeleport();
        }

        /// <summary>
        /// Returns the <c>Vector3</c> position that the player is currently looking at.
        /// </summary>
        public static Vector3? RaycastPos()
        {
            if (Weapon == null) return null;
            Ray ray = new Ray(Weapon.vacOrigin.transform.position, Weapon.vacOrigin.transform.up);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, float.MaxValue, -1610612997))
            {
                return raycastHit.point;
            }

            return null;
        }

        /// <summary>
        /// Returns the players current view raycast.
        /// </summary>
        public static RaycastHit? Raycast()
        {
            if (Weapon == null) return null;
            Ray ray = new Ray(Weapon.vacOrigin.transform.position, Weapon.vacOrigin.transform.up);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, float.MaxValue, -1610612997))
            {
                return raycastHit;
            }

            return null;
        }
        #endregion

        #region Input

        /// <summary>
        /// Freezes all user input to the player object/camera
        /// </summary>
        public static void Freeze()
        {
            var lod = gameObject.GetComponent<LockOnDeath>();
            if (lod != null) lod.Freeze();
        }

        /// <summary>
        /// unfreezes all user input to the player object/camera
        /// </summary>
        public static void Unfreeze()
        {
            var lod = gameObject.GetComponent<LockOnDeath>();
            if (lod != null) lod.Unfreeze();
        }
        #endregion

        #region DEBUG FUNCTIONALITY

        public static void Toggle_Fly_Mode()
        {
            DevCamera.Instance.Toggle();
        }
        #endregion
    }
}
