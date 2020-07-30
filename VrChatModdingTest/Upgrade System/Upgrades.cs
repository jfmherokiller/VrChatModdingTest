using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace SR_PluginLoader
{
    public class Upgrades
    {
        private static bool setup = false;
        private static List<HookUID> hooks = new List<HookUID>();// We are responsible for tracking any and all hooks we have registered
        /// <summary>
        /// A complete map of all existing custom upgrades.
        /// Since this upgrade list is only populated when plugins load/unload we do not want to reset it in the Upgrade_system's Setup logic.
        /// </summary>
        private static Dictionary<Upgrade_Type, Dictionary<string, IUpgrade>> ALL = new Dictionary<Upgrade_Type, Dictionary<string, IUpgrade>>()
        {
            { Upgrade_Type.PLOT_UPGRADE, new Dictionary<string, IUpgrade>() },
            { Upgrade_Type.PLAYER_UPGRADE, new Dictionary<string, IUpgrade>() },
        };
        /// <summary>
        /// The list of custom upgrades the player has bought
        /// </summary>
        public static List<IUpgrade> PlayerUpgrades { get; protected set; }
        /// <summary>
        /// A list of all the player upgrade id strings we werent able to load an upgrade instance for, we keep these so that if a plugin fails to load but a player had the upgrade for it, they will not lose the upgrade when they DO get the plugin to load again.
        /// </summary>
        public static List<string> Player_Upgrades_Missing { get; protected set; }
        /// <summary>
        /// Okay, so SlimeRancher actually has the LandPlot.allPlots list full of placeholder LandPlot instances upon GameLoad, probably from their placement within the editor.
        /// SR searches through, deletes, and replaces these LandPlot placeholders with new instances loaded from the save file.
        /// The problem is that those placeholders do not remove themselves from LandPlots.allPlots until NEXT FRAME when Unity fires OnDestroyed() for these LandPlot instances, after which the new LandPlots replacing them have Start() called and they add themselves to the LandPlots.allPlots list...
        /// So we need to cache our plot upgrades data until the CORRECT LandPlot instances fire "SetUpgrades()" which will trigger our hooks whereafter we will be able to locate the cached upgrades data and attach it to the plot!
        /// </summary>
        public static List<Plot_Upgrades> Plot_Upgrades_Cache;
        private static Dictionary<PlotID, Dictionary<string, Dictionary<string, byte[]>>> Plot_Upgrade_Data;


        internal static void Setup()
        {
            if (setup) return;
            setup = true;

            PlayerUpgrades = new List<IUpgrade>();
            Player_Upgrades_Missing = new List<string>();
            
            Plot_Upgrades_Cache = new List<Plot_Upgrades>();
            Plot_Upgrade_Data = new Dictionary<PlotID, Dictionary<string, Dictionary<string, byte[]>>>();

            foreach (HookUID hk in hooks) { SiscosHooks.unregister(hk); }

            hooks.Add(SiscosHooks.register(HOOK_ID.Game_Saved, onGameSaved));
            hooks.Add(SiscosHooks.register(HOOK_ID.Pre_Game_Loaded, onPreGameLoaded));
            hooks.Add(SiscosHooks.register(HOOK_ID.Post_Game_Loaded, onPostGameLoaded));
            hooks.Add(SiscosHooks.register(HOOK_ID.Spawn_Player_Upgrades_UI, onSpawn_PlayerUpgrades_Kiosk));
            hooks.Add(SiscosHooks.register(HOOK_ID.Spawn_Plot_Upgrades_UI, onSpawn_PlotUpgrades_Kiosk));
            hooks.Add(SiscosHooks.register(HOOK_ID.Plot_Load_Upgrades, onPlot_Loaded_Upgrades));
            hooks.Add(SiscosHooks.register(HOOK_ID.Level_Loaded, onLevelLoaded));
            hooks.Add(SiscosHooks.register(HOOK_ID.Demolished_Land_Plot, onPlot_Demolished));
        }

        #region Misc
        internal static void Register(IUpgrade upgrade)
        {
            if (!ALL.ContainsKey(upgrade.Type)) ALL.Add(upgrade.Type, new Dictionary<string, IUpgrade>());

            var dict = ALL[upgrade.Type];
            if (dict.ContainsKey(upgrade.ID))
            {
                SLog.Warn("[Upgrades] Blocked Attempt to register duplicate {0} upgrade instance for \"{1}\"", upgrade.Type, upgrade.ID);
                return;
            }
            //var old = ALL[upgrade.Type].FirstOrDefault(o => String.Compare(o.ID, upgrade.ID) == 0);
            //if (old != null) ALL[upgrade.Type].Remove(old);

            SLog.Debug("[Upgrades] Registered: {0}", upgrade.ID);
            ALL[upgrade.Type].Add(upgrade.ID, upgrade);
        }

        public static IUpgrade Get_Upgrade(Upgrade_Type type, string ID)
        {
            if (!ALL.ContainsKey(type)) return null;

            //return ALL[type].FirstOrDefault(o => (String.Compare(o.ID, ID) == 0));
            IUpgrade up = null;
            if (!ALL[type].TryGetValue(ID, out up)) return null;

            return up;
        }
        #endregion

        #region Upgrade Data Values
        private static void Setup_Upgrade_Data_Dict(PlotID pid, string upgradeName)
        {
            if (!Plot_Upgrade_Data.ContainsKey(pid)) Plot_Upgrade_Data.Add(pid, new Dictionary<string, Dictionary<string, byte[]>>());
            if (!Plot_Upgrade_Data[pid].ContainsKey(upgradeName)) Plot_Upgrade_Data[pid].Add(upgradeName, new Dictionary<string, byte[]>());
        }

        public static void Write_Upgrade_Data_Value(PlotID pid, PlotUpgrade upgrade, string key, byte[] value)
        {
            Write_Upgrade_Data_Value(pid, upgrade.ID, key, value);
        }

        public static void Write_Upgrade_Data_Value(PlotID pid, string upgradeName, string key, byte[] value)
        {
            Setup_Upgrade_Data_Dict(pid, upgradeName);
            if (!Plot_Upgrade_Data[pid][upgradeName].ContainsKey(key)) Plot_Upgrade_Data[pid][upgradeName].Add(key, value);
            else Plot_Upgrade_Data[pid][upgradeName][key] = value;
            
            //PLog.Info("SET => [{0}][{1}] [{2}]: ({3}){4}", pid, upgradeName, key, value.Length, Util.FormatByteArray(value));
        }

        public static byte[] Read_Upgrade_Data_Value(PlotID pid, string upgradeName, string key)
        {
            if (!Plot_Upgrade_Data.ContainsKey(pid)) return null;
            if (!Plot_Upgrade_Data[pid].ContainsKey(upgradeName)) return null;
            if (!Plot_Upgrade_Data[pid][upgradeName].ContainsKey(key)) return null;

            byte[] buf = null;
            if (!Plot_Upgrade_Data[pid][upgradeName].TryGetValue(key, out buf)) return null;

            return buf;
        }

        public static bool Has_Upgrade_Data_Value(PlotID pid, string upgradeName, string key)
        {
            if (!Plot_Upgrade_Data.ContainsKey(pid)) return false;
            if (!Plot_Upgrade_Data[pid].ContainsKey(upgradeName)) return false;
            if (!Plot_Upgrade_Data[pid][upgradeName].ContainsKey(key)) return false;

            return Plot_Upgrade_Data[pid][upgradeName].ContainsKey(key);
        }

        public static void Clear_Upgrade_Data(PlotID pid, string upgradeName)
        {
            if (!Plot_Upgrade_Data.ContainsKey(pid)) return;
            if (!Plot_Upgrade_Data[pid].ContainsKey(upgradeName)) return;

            Plot_Upgrade_Data[pid].Remove(upgradeName);
        }
        #endregion

        #region Purchasing
        public static bool TryPurchase(PersonalUpgradeUI kiosk, PlayerUpgrade upgrade)
        {
            if (Player.HasUpgrade(upgrade))
            {
                kiosk.PlayErrorCue();
                kiosk.Error("e.already_has_personal_upgrade");
            }
            else if (!Player.CanBuyUpgrade(upgrade))
            {
                kiosk.PlayErrorCue();
                kiosk.Error("e.ineligible_for_personal_upgrade");
            }
            else if (Player.Currency >= upgrade.Cost)
            {
                Sound.Play(SoundId.PURCHASED_PLAYER_UPGRADE);
                Player.SpendCurrency((int)upgrade.Cost, false);
                Player.GiveUpgrade(upgrade);
                kiosk.Close();
                return true;
            }
            else
            {
                kiosk.PlayErrorCue();
                kiosk.Error("e.insuf_coins");
            }
            return false;
        }

        public static bool TryPurchase(LandPlotUI kiosk, PlotUpgrade upgrade)
        {
#if !SR_VANILLA
            LandPlot plot = kiosk.Get_LandPlot();
            PlotUpgradeTracker tracker = plot.GetComponent<PlotUpgradeTracker>();

            if (tracker.HasUpgrade(upgrade))
            {
                kiosk.Error("e.already_has_upgrade");
            }
            else if (Player.Currency >= upgrade.Cost)
            {
                Sound.Play(SoundId.PURCHASED_PLOT_UPGRADE);
                Player.SpendCurrency(upgrade.Cost, false);
                tracker.Add(upgrade);
                kiosk.Close();
                return true;
            }
            else
            {
                Sound.Play(SoundId.ERROR);
                kiosk.Error("e.insuf_coins");
            }
#endif
            return false;
        }
#endregion
        
#region Event Handlers
        private static Sisco_Return onPostGameLoaded(ref object sender, ref object[] args, ref object return_value)
        {
            string gameName = (string)args[0];
            string plyUpgradesFile = String.Concat(gameName, ".pug");

            // LOAD PLAYER UPGRADES
            if (File.Exists(plyUpgradesFile))
            {
                string str = File.ReadAllText(plyUpgradesFile);
                string[] list = str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (list.Length <= 0) return null;

                foreach (string ID in list)
                {
                    string id = ID.ToLower();
                    PlayerUpgrade upgrade = Get_Upgrade(Upgrade_Type.PLAYER_UPGRADE, id) as PlayerUpgrade;
                    if (upgrade == null)
                    {
                        Player_Upgrades_Missing.Add(id);
                        SLog.Info("[Upgrades] Unable to find Upgrade, ID: {0}", id);
                    }
                    else
                    {
                        Player.GiveUpgrade(upgrade);
                    }
                }
            }
            return null;
        }

        private static Sisco_Return onPreGameLoaded(ref object sender, ref object[] args, ref object return_value)
        {
            string gameName = (string)args[0];
            string plotUpgradesFile = String.Concat(gameName, ".plu");
            Load_Plot_Upgrades(plotUpgradesFile);
            return null;
        }

        private static Sisco_Return onGameSaved(ref object sender, ref object[] args, ref object return_value)
        {
            string gameName = (string)args[0];
            string plyUpgradesFile = String.Concat(gameName, ".pug");
            string plotUpgradesFile = String.Concat(gameName, ".plu");

            SLog.Debug("Writing: {0}", plyUpgradesFile);
            SLog.Debug("Writing: {0}", plotUpgradesFile);

            // SAVE PLAYER UPGRADES
            // Always write to a temp file first when saving data
            string tmpFile = String.Concat(plyUpgradesFile, ".tmp");
            List<string> upgrades_list = new List<string>(Player_Upgrades_Missing);
            upgrades_list.AddRange( PlayerUpgrades.Select(o => o.ID).ToArray() );

            File.WriteAllText(tmpFile, String.Join("\n", upgrades_list.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()));
            File.Copy(tmpFile, plyUpgradesFile, true);
            File.Delete(tmpFile);

            // SAVE PLOT UPGRADES
            Save_Plot_Upgrades(plotUpgradesFile);
            return null;
        }

        private static Sisco_Return onPlot_Loaded_Upgrades(ref object sender, ref object[] args, ref object return_value)
        {
            LandPlot plot = (LandPlot)sender;
            PlotID id = new PlotID(plot);

            Plot_Upgrades cached = Plot_Upgrades_Cache.FirstOrDefault(o => o.ID.Equals(id));

            if (cached != null)
            {
                var track = plot.gameObject.AddComponent<PlotUpgradeTracker>();
                track.Load(plot, cached);

                Plot_Upgrades_Cache.Remove(cached);
            }

            return null;
        }

        private static Sisco_Return onPlot_Demolished(ref object sender, ref object[] args, ref object return_value)
        {
            LandPlot.Id ID = (LandPlot.Id)args[0];
            LandPlot plot = sender as LandPlot;
            PlotID id = new PlotID(plot);
            //SLog.Debug("Plot({0}){1} Demolished", ID, id);

            var track = plot.gameObject.GetComponent<PlotUpgradeTracker>();
            track.Remove_All();

            return null;
        }

        private static Sisco_Return onLevelLoaded(ref object sender, ref object[] args, ref object return_value)
        {
            // Each time we load the main menu we want to clear any and all upgrade related data to ensure nothing freaky happens when the player loads a new file.
            int lvl = (int)args[0];
            if(lvl == 0 || Levels.isSpecial())
            {
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((object o) =>
                {
                    SLog.Debug("[Upgrades] Flushing custom upgrades...");
                    Upgrades.setup = false;
                    Upgrades.Setup();

                    timer.Dispose();
                }, null, 0, System.Threading.Timeout.Infinite);
            }
            return null;
        }

        private static Sisco_Return onSpawn_PlayerUpgrades_Kiosk(ref object sender, ref object[] args, ref object return_value)
        {
            if(!ALL.ContainsKey(Upgrade_Type.PLAYER_UPGRADE)) return null;

            var kiosk = sender as PersonalUpgradeUI;
            GameObject panel = return_value as GameObject;
            var ui = panel.GetComponent<PurchaseUI>();
            
            foreach (PlayerUpgrade up in ALL[Upgrade_Type.PLAYER_UPGRADE].Values)
            {
                ui.AddButton(new PurchaseUI.Purchasable(up.Name, up.Sprite, up.PreviewSprite, up.Description, up.Cost, new PediaDirector.Id?(), new UnityAction(() => { up.Purchase(kiosk.gameObject); }), Player.CanBuyUpgrade(up)));
            }

            return null;
        }
        
        private static Sisco_Return onSpawn_PlotUpgrades_Kiosk(ref object sender, ref object[] args, ref object return_value)
        {
            LandPlot.Id kind = (LandPlot.Id)args[0];
#if !SR_VANILLA
            LandPlotUI kiosk = sender as LandPlotUI;
            LandPlot plot = kiosk.Get_LandPlot();
            PlotUpgradeTracker tracker = plot.GetComponent<PlotUpgradeTracker>();
            if (tracker == null) tracker = plot.gameObject.AddComponent<PlotUpgradeTracker>();
                

            GameObject panel = return_value as GameObject;
            var ui = panel.GetComponent<PurchaseUI>();

            foreach (PlotUpgrade up in ALL[Upgrade_Type.PLOT_UPGRADE].Values)
            {
                if (up.Kind != kind) continue;
                bool can_buy = up.CanBuy(tracker);
                ui.AddButton(new PurchaseUI.Purchasable(up.Name, up.Sprite, up.PreviewSprite, up.Description, up.Cost, new PediaDirector.Id?(), new UnityAction(() => { up.Purchase(kiosk.gameObject); }), can_buy));
            }
#endif
            return null;
        }
#endregion

#region Saving / Loading
        private static void Load_Plot_Upgrades(string fileName)
        {
            // LOAD PLOT UPGRADES
            if (File.Exists(fileName))
            {
                Stream strm = File.OpenRead(fileName);
                if (strm == null) throw new IOException("Cannot open file for reading: " + fileName);

                BinaryReader br = new BinaryReader(strm);
                int total = br.ReadInt32();
                for (int i = 0; i < total; i++)
                {
                    Plot_Upgrades up = Plot_Upgrades.Deserialize(br);
                    Plot_Upgrades_Cache.Add(up);
                }

                // Load the upgrade data cache
                int pidTotal = br.ReadInt32();
                for (int p = 0; p < pidTotal; p++)
                {
                    PlotID pid = PlotID.Deserialize(br);
                    int upgrTotal = br.ReadInt32();

                    for (int u = 0; u < upgrTotal; u++)
                    {
                        string upgrade = br.ReadString();
                        int kTotal = br.ReadInt32();
                        // Make sure we can add keys/values without issue.
                        Setup_Upgrade_Data_Dict(pid, upgrade);

                        for (int k = 0; k < kTotal; k++)
                        {
                            string key = br.ReadString();
                            int blen = br.ReadInt32();
                            byte[] value = br.ReadBytes(blen);

                            //PLog.Info("[{0}][{1}] [{2}]: ({3}){4}", pid, upgrade, key, blen, Util.FormatByteArray(value));

                            if (!Plot_Upgrade_Data[pid][upgrade].ContainsKey(key)) Plot_Upgrade_Data[pid][upgrade].Add(key, value);
                            else Plot_Upgrade_Data[pid][upgrade][key] = value;
                        }
                    }
                }

                strm.Close();
            }
        }

        private static void Save_Plot_Upgrades(string fileName)
        {
            string tempFile = String.Concat(fileName, ".tmp");

            Stream strm = File.OpenWrite(tempFile);
            if (strm == null) throw new IOException("Cannot open file for writing: " + fileName);

            BinaryWriter bw = new BinaryWriter(strm);
            bw.Write((int)PlotUpgradeTracker.allTrackers.Count);
            foreach (PlotUpgradeTracker tracker in PlotUpgradeTracker.allTrackers)
            {
                tracker.Serialize(bw);
            }

            bw.Write((int)Plot_Upgrade_Data.Count);
            // Write all of the plot's upgrades' set data values
            foreach (var plot_kvp in Plot_Upgrade_Data)
            {
                PlotID plot = plot_kvp.Key;
                plot.Serialize(bw);

                bw.Write((int)plot_kvp.Value.Count);
                foreach (var upgr_kvp in plot_kvp.Value)
                {
                    string upgrade = upgr_kvp.Key;
                    bw.Write(upgrade);

                    bw.Write((int)upgr_kvp.Value.Count);
                    foreach (var kvp in upgr_kvp.Value)
                    {
                        bw.Write((string)kvp.Key);
                        bw.Write((int)kvp.Value.Length);
                        bw.Write(kvp.Value);
                    }
                }
            }

            strm.Close();
            File.Copy(tempFile, fileName, true);
            File.Delete(tempFile);
        }
#endregion
    }

    /// <summary>
    /// Assists in accurately keeping track of LandPlots, being able to be compared to a LandPlot instance and consistently identify weather or not it matches that LandPlot.
    /// </summary>
    [Serializable]
    public class PlotID
    {
        private Vector3? pos = null;
        public bool Valid { get { return pos.HasValue; } }

        public PlotID() { }
        public PlotID(LandPlot plot) { pos = plot.gameObject.transform.position; }

        public void Serialize(BinaryWriter bw)
        {
            if(pos.HasValue)
            {
                bw.Write((float)pos.Value.x);
                bw.Write((float)pos.Value.y);
                bw.Write((float)pos.Value.z);
            }
            else
            {
                bw.Write((float)0);
                bw.Write((float)0);
                bw.Write((float)0);
            }
        }

        public static PlotID Deserialize(BinaryReader br)
        {
            float x, y, z;
            x = br.ReadSingle();
            y = br.ReadSingle();
            z = br.ReadSingle();

            return new PlotID() { pos = new Vector3(x, y, z) };
        }

        public bool Matches(LandPlot plot)
        {
            Vector3 plot_pos = plot.gameObject.transform.position;
            return this.Matches(plot_pos);
        }

        public bool Matches(Vector3 B)
        {
            return ((pos.Value - B).sqrMagnitude < 1f);
        }

        public override bool Equals(object obj)
        {
            PlotID B = obj as PlotID;
            return Matches(B.pos.Value);
        }

        public override int GetHashCode()
        {
            return pos.Value.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("<{0}, {1}, {2}>", (int)pos.Value.x, (int)pos.Value.y, (int)pos.Value.z);
        }
    }

    /// <summary>
    /// Assists with serialization of Plot Upgrades
    /// </summary>
    public class Plot_Upgrades
    {
        public PlotID ID = new PlotID();
        public List<string> upgrades = new List<string>();

        public Plot_Upgrades() { }

        public static void Serialize(BinaryWriter bw, PlotID id, List<string> list)
        {
            //PLog.Info("PlotUpgrades: {0} | Writing {1} plot upgrades", id, list.Count);
            id.Serialize(bw);
            bw.Write((int)list.Count);
            foreach(string str in list)
            {
                bw.Write(str);
            }
        }

        public static Plot_Upgrades Deserialize(BinaryReader br)
        {
            PlotID id = PlotID.Deserialize(br);
            List<string> list = new List<string>();

            int num = br.ReadInt32();//the number of upgrade strings this list has
            for(int i=0; i<num; i++)
                list.Add( br.ReadString() );

            return new Plot_Upgrades() { ID=id, upgrades=list };
        }
    }

    /// <summary>
    /// Assists in reading/writing saved values for plot a upgrade instances
    /// </summary>
    public class Plot_Upgrade_Data
    {
        private PlotID pid = null;
        private string upgradeName = null;

        public Plot_Upgrade_Data(PlotID id, string upgrName)
        {
            pid = id;
            upgradeName = upgrName;
        }

        public bool HasValue(string key)
        {
            return Upgrades.Has_Upgrade_Data_Value(pid, upgradeName, key);
        }

#region Setters
        public void Set_Bool(string key, bool v)
        {
            Upgrades.Write_Upgrade_Data_Value(pid, upgradeName, key, BitConverter.GetBytes(v));
        }

        public void Set_Int(string key, int v)
        {
            Upgrades.Write_Upgrade_Data_Value(pid, upgradeName, key, BitConverter.GetBytes(v));
        }

        public void Set_Float(string key, float v)
        {
            Upgrades.Write_Upgrade_Data_Value(pid, upgradeName, key, BitConverter.GetBytes(v));
        }

        public void Set_Double(string key, double v)
        {
            Upgrades.Write_Upgrade_Data_Value(pid, upgradeName, key, BitConverter.GetBytes(v));
        }

        public void Set_String(string key, string str)
        {
            Upgrades.Write_Upgrade_Data_Value(pid, upgradeName, key, UTF8Encoding.UTF8.GetBytes(str));
        }
#endregion

#region Getters
        public bool Get_Bool(string key)
        {
            return BitConverter.ToBoolean(Upgrades.Read_Upgrade_Data_Value(pid, upgradeName, key), 0);
        }

        public int Get_Int(string key)
        {
            return BitConverter.ToInt32(Upgrades.Read_Upgrade_Data_Value(pid, upgradeName, key), 0);
        }

        public float Get_Float(string key)
        {
            return BitConverter.ToSingle(Upgrades.Read_Upgrade_Data_Value(pid, upgradeName, key), 0);
        }

        public double Get_Double(string key)
        {
            return BitConverter.ToDouble(Upgrades.Read_Upgrade_Data_Value(pid, upgradeName, key), 0);
        }

        public string Get_String(string key)
        {
            byte[] buf = Upgrades.Read_Upgrade_Data_Value(pid, upgradeName, key);
            if (buf == null) return null;

            return UTF8Encoding.UTF8.GetString(buf);
        }
#endregion
    }
}
