using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Each LandPlot get's an instance of this script attached to manage it's custom Plot upgrades.
    /// </summary>
    [Serializable]
    public class PlotUpgradeTracker : MonoBehaviour
    {
        /// <summary>
        /// The index within the global PlotList for the LandPlot which owns this instance.
        /// </summary>
        //private int idx = -1;
        private PlotID ID = new PlotID();
        private HashSet<PlotUpgrade> upgrades = new HashSet<PlotUpgrade>();
        /// <summary>
        /// All of the upgrades that we couldn't find and instance for but which we KNOW this tracker should have and serialize to file.
        /// </summary>
        private List<string> upgrades_missing = new List<string>();
        private HashSet<string> allUpgrades { get { return new HashSet<string>(new List<string>(upgrades_missing).Concat(this.upgrades.Select(o => o.ID)).ToList()); } }

        public static List<PlotUpgradeTracker> allTrackers = new List<PlotUpgradeTracker>();
        public LandPlot plot { get; private set; }


        #region Initialization
        private void Awake()
        {
            PlotUpgradeTracker.allTrackers.Add(this);
        }

        private void OnDestroy()
        {
            PlotUpgradeTracker.allTrackers.Remove(this);
        }


        private void Start()
        {
            // Determine our ID
            if (!ID.Valid)
            {
                for (int i = 0; i < LandPlot.allPlots.Count; i++)
                {
                    LandPlot plot = LandPlot.allPlots[i];
                    if (plot.gameObject == base.gameObject)
                    {
                        this.plot = plot;
                        this.ID = new PlotID(plot);
                        break;
                    }
                }
            }

            if(plot == null)
            {
                throw new ArgumentNullException("Cannot locate LandPlot to apply upgrades!");
            }

            // Apply all of our custom upgrades to the plot
            foreach (IUpgrade upgrade in upgrades)
            {
                upgrade.Apply(plot.gameObject);
            }
        }
        #endregion

        #region Public Manipulation
        public void Add(PlotUpgrade upgrade)
        {
            upgrades.Add(upgrade);
            upgrade.Apply(plot.gameObject);
        }

        public void Remove(PlotUpgrade upgrade)
        {
            upgrades.Remove(upgrade);
            upgrade.Remove(plot);
        }

        public bool HasUpgrade(PlotUpgrade upgrade)
        {
            return allUpgrades.Contains(upgrade.ID);
        }

        #endregion

        public void Remove_Upgrade(PlotUpgrade upgrade)
        {
            bool b = upgrades.Remove(upgrade);
            if (!b) SLog.Warn("Failed to remove custom plot upgrade from tracker. Upgrade: {0}", upgrade);
        }

        public void Remove_All()
        {
            List<PlotUpgrade> list = new List<PlotUpgrade>(upgrades);
            foreach(PlotUpgrade u in list)
            {
                try
                {
                    SLog.Info("Removing: {0}", u);
                    u.Remove(plot);
                }
                catch(Exception ex)// No surprises kthx
                {
                    SLog.Error(ex);
                }
            }
        }

        #region Serialization / Deserialization
        internal void Load(LandPlot pl, Plot_Upgrades up)
        {
            this.plot = pl;
            this.ID = new PlotID(plot);
            foreach(string upgradeName in up.upgrades)
            {
                PlotUpgrade upgrade = (PlotUpgrade)Upgrades.Get_Upgrade(Upgrade_Type.PLOT_UPGRADE, upgradeName);

                if (upgrade != null) this.upgrades.Add(upgrade);
                else upgrades_missing.Add(upgradeName);
            }
        }

        internal void Serialize(BinaryWriter bw)
        {
            Plot_Upgrades.Serialize(bw, ID, allUpgrades.ToList());
        }
        #endregion

    }
}
