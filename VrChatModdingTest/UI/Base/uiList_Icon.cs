using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// A <see cref="uiListItem"/> which puts focus on it's icon with text below it.
    /// </summary>
    class uiListIcon : uiListItem
    {
        public uiListIcon() : base()
        {
            title.TextSize = 12;
            title.local_style.wordWrap = true;
            title.Autosize_Method = AutosizeMethod.BLOCK;
            title.Autosize = true;
            title.TextAlign = TextAnchor.MiddleCenter;

            icon.Autosize = false;
            icon.SizeConstraint = uiSizeConstraint.WIDTH_MATCHES_HEIGHT;
        }
        
        public override void doLayout()
        {
            icon.alignTop();
            icon.CenterHorizontally();
            icon.FloodY(title.Area.size.y);

            title.CenterHorizontally();
            title.moveBelow(icon);
        }
    }
}
