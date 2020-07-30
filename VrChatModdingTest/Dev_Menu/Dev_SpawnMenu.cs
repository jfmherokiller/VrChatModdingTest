using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public enum SpawnCategory
    {
        NONE = 0,
        SLIMES,
        PLORTS,
        ANIMALS,
        FRUITS,
        VEGETABLES
    }

    public class Dev_SpawnMenu : uiPanel
    {
        uiListView cList = null;
        uiTabPanel catPanel = null;


        public Dev_SpawnMenu()
        {
            Autosize = true;
            Autosize_Method = AutosizeMethod.FILL;
            onLayout += Layout;

            // This list will contain different categories of spawnable items
            cList = uiControl.Create<uiListView>(this);
            cList.Set_Margin(0, 4, 0, 0);
            cList.Set_Width(130f);

            catPanel = uiControl.Create<uiTabPanel>(this);

            foreach (SpawnCategory cty in Enum.GetValues(typeof(SpawnCategory)))
            {
                if (cty == SpawnCategory.NONE) continue;
                string catStr = Enum.GetName(typeof(SpawnCategory), cty);
                var catBtn = uiControl.Create<uiListItem>(String.Concat("category_", catStr.ToLower()), cList);
                catBtn.Title = catStr.ToLower().CapitalizeFirst();
                catBtn.Description = null;

                uiTab cTab = null;
                switch (cty)
                {
                    case SpawnCategory.SLIMES:
                        cTab = Create_Spawn_Category_Menu(catPanel, cty, Ident.ALL_SLIMES);
                        break;
                    case SpawnCategory.PLORTS:
                        cTab = Create_Spawn_Category_Menu(catPanel, cty, Identifiable.PLORT_CLASS);
                        break;
                    case SpawnCategory.ANIMALS:
                        cTab = Create_Spawn_Category_Menu(catPanel, cty, Ident.ALL_ANIMALS);
                        break;
                    case SpawnCategory.FRUITS:
                        cTab = Create_Spawn_Category_Menu(catPanel, cty, Identifiable.FRUIT_CLASS);
                        break;
                    case SpawnCategory.VEGETABLES:
                        cTab = Create_Spawn_Category_Menu(catPanel, cty, Identifiable.VEGGIE_CLASS);
                        break;

                    default:
                        SLog.Info("Unhandled Spawn menu category: {0}", catStr);
                        break;
                }

                catBtn.onSelected += (uiControl c) =>
                {
                    Sound.Play(SoundId.BTN_CLICK);
                    cTab.Select();
                };

            }
        }

        private new void Layout(uiPanel p)
        {
            base.doLayout();

            cList.alignTop();
            cList.alignLeftSide();
            cList.FloodY();

            catPanel.moveRightOf(cList);
            catPanel.FloodXY();

        }
                
        private void Dev_Spawn_Item(Identifiable.Id ID)
        {
            if (Game.atMainMenu)
            {
                Sound.Play(SoundId.ERROR);
                SLog.Info("Failed to spawn item: {0}, We are at the main menu.", ID);
                return;
            }

            RaycastHit? ray = Player.Raycast();
            if (!ray.HasValue)
            {
                Sound.Play(SoundId.ERROR);
                SLog.Info("Failed to spawn item: {0}, Unable to perform raycast from player's view. Perhaps the ray distance is too far.", ID);
            }

            if (Util.TrySpawn(ID, ray.Value) == null)
            {
                Sound.Play(SoundId.ERROR);
                SLog.Info("Failed to spawn item: {0}, An unknown error occured", ID);
            }
            else Sound.Play(SoundId.BTN_CLICK);
        }

        private uiTab Create_Spawn_Category_Menu(uiTabPanel container, SpawnCategory cty, HashSet<Identifiable.Id> ITEMS)
        {
            // This list will just have a bunch of pictures of slimes that can be spawned
            string catStr = Enum.GetName(typeof(SpawnCategory), cty);
            float ICON_SIZE = 100f;
            uiTab tab = container.Add_Tab(catStr);
            uiListView list = uiControl.Create<uiListView>(catStr, tab);
            list.Layout = new Layout_IconList(ICON_SIZE);
            list.Autosize_Method = AutosizeMethod.FILL;
            list.Autosize = true;


            foreach (Identifiable.Id ID in ITEMS)
            {
                Sprite sprite = null;
                try
                {
                    sprite = Directors.lookupDirector.GetIcon(ID);
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    SLog.Debug(ex);
                }

                if (sprite == null) continue;// Exclude anything without an icon out of respect for the devs, we will just assume things without an icon aren't in the game just yet I suppose...

                var itm = uiControl.Create<uiListIcon>(list);
                if (sprite != null) itm.Icon = sprite.texture;
                else itm.Icon = TextureHelper.icon_unknown;

                itm.Title = Language.Translate(ID);
                itm.Set_Size(ICON_SIZE, ICON_SIZE);
                itm.onClicked += (uiControl c) => { Dev_Spawn_Item(ID); };
                itm.Selectable = false;
            }

            return tab;
        }
    }
}
