using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace SR_PluginLoader
{
    public enum Upgrade_Type
    {
        INVALID = 0,
        PLAYER_UPGRADE,
        PLOT_UPGRADE
    }

    public interface IUpgrade
    {
        Upgrade_Type Type { get; }
        /// <summary>
        /// How many credits this upgrade costs
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// The icon that will represent this upgrade in the PurchaseUI
        /// </summary>
        Texture2D Icon { get; }
        Sprite Sprite { get; }

        /// <summary>
        /// The title of this upgrade
        /// </summary>
        string Name { get; }
        string Description { get; }
        /// <summary>
        /// A unique identifier string for the upgrade
        /// </summary>
        string ID { get; }

        bool Apply(GameObject obj);
        bool Purchase(GameObject obj);
    }

    public abstract class UpgradeBase : IUpgrade
    {
        private Plugin Parent;
        /// <summary>
        /// List of other upgrades that must be obtained before this one may.
        /// </summary>
        protected List<IUpgrade> prereqs = new List<IUpgrade>();
        public virtual Upgrade_Type Type { get { return Upgrade_Type.INVALID; } }
        /// <summary>
        /// How many credits this upgrade costs
        /// </summary>
        public int Cost { get; private set; }

        /// <summary>
        /// The function callback that is fired when this upgrade needs to apply itself.
        /// The callback will be pased a <c>GameObject</c> as it's parameter, this is the GameObject it should be applied to.
        /// </summary>
        protected Action<GameObject> ApplyFunction { get; private set; }

        /// <summary>
        /// The icon that will represent this upgrade in the PurchaseUI
        /// </summary>
        public Texture2D Icon { get; private set; }
        /// <summary>
        /// The preview image shown in the PurchaseUI.
        /// </summary>
        public Texture2D Preview { get; private set; }
        public Sprite Sprite { get { if (Icon == null) { return null; } return Sprite.Create(Icon, new Rect(0, 0, Icon.width, Icon.height), new Vector2(0.5f, 0.5f)); } }
        public Sprite PreviewSprite { get { if (Preview == null) { return null; } return Sprite.Create(Preview, new Rect(0, 0, Preview.width, Preview.height), new Vector2(0.5f, 0.5f)); } }

        /// <summary>
        /// The title of this upgrade
        /// </summary>
        public string Name { get; private set; }
        public string Description { get; private set; }

        /// <summary>
        /// A unique identifier string for the upgrade
        /// </summary>
        public string ID { get { return _id; } protected set { _id = value.ToLower(); } }
        private string _id = null;

        public abstract bool Purchase(GameObject obj);
        public Func<bool> can_buy_func = null;
        
        
        public UpgradeBase(Plugin parent, string id, int cost, string name, string desc, Action<GameObject> function, Texture2D icon, Texture2D preview)
        {
            Parent = parent;
            if (Parent == null) ID = String.Concat("unknown.", id).ToLower();
            else ID = String.Concat(Parent.data.NAME, ".", Parent.data.AUTHOR, ".", id).ToLower();

            Cost = cost;
            Name = name;
            Description = desc;
            Icon = icon;
            Preview = preview;
            ApplyFunction = function;

            if (Icon == null && Parent != null) Icon = Parent.icon;
            if (Icon == null)
            {// If all else fails we will just make the icon transparent.
                Preview = new Texture2D(1, 1);
                Preview.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
                Preview.Apply();
            }

            if (Preview == null)
            {// If all else fails we will just make the icon transparent.
                Preview = new Texture2D(1, 1);
                Preview.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
                Preview.Apply();
            }
        }

        public bool Apply(GameObject obj)
        {
            if (ApplyFunction == null) return false;

            ApplyFunction(obj);
            return true;
        }

        /// <summary>
        /// Adds a another upgrade as a prerequisite for this one.
        /// The player will have to obtain the other upgrade before this one is available.
        /// </summary>
        /// <param name="upgrade"></param>
        public void Requires(IUpgrade upgrade)
        {
            prereqs.Add(upgrade);
        }
    }

    /// <summary>
    /// An upgrade that modifies the players stats
    /// </summary>
    public class PlayerUpgrade : UpgradeBase
    {
        public override Upgrade_Type Type { get { return Upgrade_Type.PLAYER_UPGRADE; } }
        public override bool Purchase(GameObject sender) { return Upgrades.TryPurchase(sender.GetComponent<PersonalUpgradeUI>(), this); }
        public bool CanBuy(GameObject obj) { if (!PrereqsMet(obj)) { return false; } if (Player.HasUpgrade(this)) { return false; } if (can_buy_func != null) { return can_buy_func(); } return true; }

        /// <summary>
        /// Returns <c>true</c> if the player has this upgrade.
        /// </summary>
        public bool IsBought { get { return Player.HasUpgrade(this); } }
        
        protected bool PrereqsMet(GameObject obj)
        {
            foreach (PlayerUpgrade up in prereqs)
            {
                if (!up.IsBought) return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">A reference to your plugin, also used to uniquely identify the upgrade in save files</param>
        /// <param name="id">A string that will be used to uniquely identify this upgrade in save files</param>
        /// <param name="cost">How many credits the upgrade costs</param>
        /// <param name="name">The title text for the upgrade when shown in the upgrade kiosk</param>
        /// <param name="desc">The description text for the upgrade when shown in the upgrade kiosk</param>
        /// <param name="function">Function that applies the upgrade's effects.</param>
        /// <param name="icon">An icon used to represent the plugin when shown in the upgrade kiosk</param>
        public PlayerUpgrade(Plugin parent, string id, int cost, string name, string desc, Action<GameObject> function, Texture2D icon=null) : base(parent, id, cost, name, desc, function, icon, null)
        {
            Upgrades.Register(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">A reference to your plugin, also used to uniquely identify the upgrade in save files</param>
        /// <param name="id">A string that will be used to uniquely identify this upgrade in save files</param>
        /// <param name="cost">How many credits the upgrade costs</param>
        /// <param name="name">The title text for the upgrade when shown in the upgrade kiosk</param>
        /// <param name="desc">The description text for the upgrade when shown in the upgrade kiosk</param>
        /// <param name="function">Function that applies the upgrade's effects.</param>
        /// <param name="icon">An icon used to represent the plugin when shown in the upgrade kiosk</param>
        /// <param name="preview">The preview image to show in the upgrade kiosk</param>
        public PlayerUpgrade(Plugin parent, string id, int cost, string name, string desc, Action<GameObject> function, Texture2D icon, Texture2D preview) : base(parent, id, cost, name, desc, function, icon, preview)
        {
            Upgrades.Register(this);
        }
    }

    /// <summary>
    /// An upgrade that alters a land plot
    /// </summary>
    public class PlotUpgrade : UpgradeBase
    {
        public override Upgrade_Type Type { get { return Upgrade_Type.PLOT_UPGRADE; } }
        public override bool Purchase(GameObject sender) { return Upgrades.TryPurchase(sender.GetComponent<LandPlotUI>(), this); }
        public LandPlot.Id Kind { get; protected set; }
        public bool CanBuy(PlotUpgradeTracker plot) { if (!PrereqsMet(plot)) { return false; } if (plot.HasUpgrade(this)) { return false; } if (can_buy_func != null) { return can_buy_func(); } return true; }
        protected Action<LandPlot> removal_function;
        /// <summary>
        /// Returns <c>true</c> if the plot has this upgrade.
        /// </summary>
        public bool IsBought(PlotUpgradeTracker obj)
        {
            return false;
        }

        protected bool PrereqsMet(PlotUpgradeTracker obj)
        {
            foreach (PlotUpgrade up in prereqs)
            {
                if (!up.IsBought(obj)) return false;
            }

            return true;
        }

        /// <summary>
        /// Removes the upgrades effects from the LandPlot
        /// </summary>
        public void Remove(LandPlot plot) { Remove(false, plot); }

        /// <summary>
        /// Removes the upgrades effects from the LandPlot
        /// </summary>
        public void Remove(bool ignore_tracker, LandPlot plot)
        {
            PlotID pID = new PlotID(plot);
            // Clear any save data this upgrade might have set for this plot.
            Upgrades.Clear_Upgrade_Data(pID, this.ID);
            if (!ignore_tracker)
            {
                // Remove the upgrade from this plot's tracker.
                PlotUpgradeTracker tracker = plot.GetComponent<PlotUpgradeTracker>();
                if (tracker == null) SLog.Warn("Failed to remove upgrade from plot {0}. Cannot find PlotUpgradeTracker!", pID);
                else tracker.Remove_Upgrade(this);
            }
            // Call the upgrades cleanup logic.
            removal_function?.Invoke(plot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">A reference to your plugin, also used to uniquely identify the upgrade in save files</param>
        /// <param name="kind">What kind of plot item this upgrade is for; Coop, Silo, Corral, Pond, Garden, etc.</param>
        /// <param name="id">A string that will be used to uniquely identify this upgrade in save files</param>
        /// <param name="cost">How many credits the upgrade costs</param>
        /// <param name="name">The title text for the upgrade when shown in the kiosk</param>
        /// <param name="desc">The description text for the upgrade when shown in the kiosk</param>
        /// <param name="function">Function that applies the upgrade's effects.</param>
        /// <param name="remove_func">Function to remove the upgrade's effects.</param>
        /// <param name="icon">An icon used to represent the upgrade when shown in the kiosk</param>
        public PlotUpgrade(Plugin parent, LandPlot.Id kind, string id, int cost, string name, string desc, Action<GameObject> function, Action<LandPlot> remove_func, Texture2D icon = null) : base(parent, id, cost, name, desc, function, icon, null)
        {
            Kind = kind;
            removal_function = remove_func;
            Upgrades.Register(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">A reference to your plugin, also used to uniquely identify the upgrade in save files</param>
        /// <param name="kind">What kind of plot item this upgrade is for; Coop, Silo, Corral, Pond, Garden, etc.</param>
        /// <param name="id">A string that will be used to uniquely identify this upgrade in save files</param>
        /// <param name="cost">How many credits the upgrade costs</param>
        /// <param name="name">The title text for the upgrade when shown in the kiosk</param>
        /// <param name="desc">The description text for the upgrade when shown in the kiosk</param>
        /// <param name="function">Function that applies the upgrade's effects.</param>
        /// <param name="remove_func">Function to remove the upgrade's effects.</param>
        /// <param name="icon">An icon used to represent the upgrade when shown in the kiosk</param>
        /// <param name="preview">The preview image to show in the upgrade kiosk</param>
        public PlotUpgrade(Plugin parent, LandPlot.Id kind, string id, int cost, string name, string desc, Action<GameObject> function, Action<LandPlot> remove_func, Texture2D icon, Texture2D preview) : base(parent, id, cost, name, desc, function, icon, preview)
        {
            Kind = kind;
            removal_function = remove_func;
            Upgrades.Register(this);
        }
    }
}
