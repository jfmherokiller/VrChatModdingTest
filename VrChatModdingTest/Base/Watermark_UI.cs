
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Manages creating the necessary components to render the Plugin Loader's watermark on the UI
    /// </summary>
    internal static class PluginLoader_Watermark
    {
        #region Variables
        private static PluginLoader_Watermark_Component root = null;
        #endregion

        public static void Setup()
        {
            if (root != null) return;
            root = uiControl.Create<PluginLoader_Watermark_Component>();
            root.alignBottom();
            root.alignLeftSide();

            root.Autosize_Method = AutosizeMethod.GROW;
            root.isVisible = true;
        }
    }
    

    internal class PluginLoader_Watermark_Component : uiPanel
    {
        private uiText label = null;
        private uiIcon icon = null;

        public PluginLoader_Watermark_Component() : base()
        {
            Autosize_Method = AutosizeMethod.GROW;
            Name = "Watermark";
            //local_style.normal.background = null;
            Set_Padding(2);
            
            label = Create<uiText>(this);
            label.TextColor = new Color(1f, 1f, 1f, 0.6f);
            label.TextSize = 16;
            label.TextStyle = FontStyle.Bold;
            label.Text = Loader.TITLE;


            icon = Create<uiIcon>(this);
            icon.local_style.normal.background = null;
            icon.Image = TextureHelper.icon_logo;
            icon.Tint = new Color(1f,1f,1f, 0.6f);
            icon.Set_Size(36, 36);
        }

        public override void doLayout()
        {
            icon.alignLeftSide(3);

            label.moveRightOf(icon, 6);
            label.alignBottom();
        }
    }
}

