using Logging;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace SR_PluginLoader
{
    public static class MainMenu
    {
        #region Internal Variables

        internal static GameObject root = null;
        internal static MainMenuUI Instance = null;
        #endregion

        #region Private Variables
        private static Active_State_Tracker State = null;// Tracks the visibility state of the MenuUI
        private static Color blue_clr = new Color32(55, 149, 237, 255);//Blue color from our old Killstreaks.tf TTT server
        private static Color clr_gold_light = new Color32(250, 194, 73, 255);
        private static Color clr_gold = new Color32(206, 124, 37, 255);
        private static Color clr_brown = new Color32(40, 12, 0, 255);
        #endregion

        #region State Tracking Variables
        private static bool _setup = false;// Have we run setup yet?
        private static bool _visible = false;// Should the main menu be visible?
        /// <summary>
        /// Is the MainMenu able to be interacted with right now?
        /// </summary>
        public static bool isReady { get { return (_setup && Game.atMainMenu); } }
        /// <summary>
        /// Should the MainMenu be rendering?
        /// </summary>
        public static bool Visible { get { return _visible; } }
        #endregion
        
        #region Setup Logic

        internal static void Setup()// This is the initial setup function, it should only be called a single time, and only by the loader itself.
        {
            if (_setup) throw new NotSupportedException("Blocked attempt to call MainMenu.Setup() a second time.");
            _setup = true;//now we have run setup!

            SiscosHooks.register(HOOK_ID.Level_Loaded, MainMenu.onLevelLoaded);

            State = new Active_State_Tracker("MAIN_MENU");
            // Create our handler game object
            root = new GameObject("PluginLoader_MainMenu_Handler");
            GameObject.DontDestroyOnLoad(root);
            var Handler = root.AddComponent<LevelLoaded_Handler_Script>();
        }

        private static Sisco_Return onLevelLoaded(ref object sender, ref object[] args, ref object return_value)
        {
            bool is_main_menu = (String.Compare(Levels.MAIN_MENU, Application.loadedLevelName) == 0);
            if (!is_main_menu) return null;
            
            TrySpawn_PluginPanel();
            TrySpawn_PluginStore();

            // We need to delay hooking into the main menu until we are sure all of the buttons have been styled and initialized by the game's default logic.
            // Otherwise the buttons are invisible and people catch on fire and it's just a big legal ordeal really...
            new GameObject().AddComponent<ActionDelayer>().SelfDestruct().onStart += () => { Hook_MainMenu(); };
            return null;
        }

        internal static void TrySpawn_PluginPanel() { if(PluginManager.Instance == null) PluginManager.Instance = uiControl.Create<PluginManager>(); }

        internal static void TrySpawn_PluginStore() { if(PluginStore.Instance == null) PluginStore.Instance = uiControl.Create<PluginStore>(); }

        private static void Hook_MainMenu()
        {
            if (Instance != null) return;// If the instance isn't null then we will have already added all of our buttons and stuff
            Instance = UnityEngine.Object.FindObjectOfType<MainMenuUI>();
            if (Instance == null) throw new Exception("Cannot find MainMenuUI!");
            if (Instance.gameObject == null) throw new Exception("MainMenuUI component has no GameObject!");

            //Output_Hierarchy();

            var tracker = Instance.gameObject.AddComponent<MainMenuUI_Tracking_Script>();// Attach our tracking script to it.
            if (tracker == null) throw new Exception("Cannot attach MainMenu tracking script!");
            // Reset the MainMenu visibility state because we JUST found it so theres no way we have it hidden yet.
            State.Reset();

            //Add_Button("Manage Plugins", "PluginsMenu", new UnityAction(Show_PluginManager));
            //Add_Button("Manage Plugins", "PluginsMenu", new UnityAction(Show_PluginManager));
            //Add_Button("Manage Plugins", "PluginsMenu", new UnityAction(Show_PluginManager));
            //Add_Button("Manage Plugins", "PluginsMenu", new UnityAction(Show_PluginManager));
            Add_Button("Plugins", "Plugins", new UnityAction(Show_PluginManager), clr_gold_light, clr_gold, clr_brown);
            
            object retVal = new object();
            SiscosHooks.call(HOOK_ID.MainMenu_Loaded, null, ref retVal, new object[] {});
        }

        private static void Show_PluginManager() { PluginManager.Instance.Show(); }

        private static void Show_PluginStore() { PluginStore.Instance.Show(); }
        #endregion

        #region Public interaction functions
        public static void Show() { if (Instance != null) { Instance.gameObject.SetActive(State.Activate()); } }

        public static void Hide() { if(Instance != null) { Instance.gameObject.SetActive(State.Deactivate()); } }

        public static void Add_Button(string text, string name, UnityAction onclick_handler, Color? color=null, Color? hl_clr=null, Color? text_clr=null)
        {
            if (Instance != null)
            {
                Transform MenuPanel = Instance.transform.FindChild("StandardModePanel");
                Logging.Logger.Assert(MenuPanel != null, "", "MenuPanel is NULL!");

                Transform Status = Instance.transform.FindChild("Status");
                RectTransform statusTrans = (Status as RectTransform);

                VerticalLayoutGroup layout = MenuPanel.GetComponent<VerticalLayoutGroup>();
                Logging.Logger.Assert(layout != null, "", "Cannot find VerticalLayoutGroup component in MenuPanel");
                // Locate the last button in the menu
                Transform lastBtnTrans = MenuPanel.transform.GetChild(MenuPanel.childCount - 1);
                //Transform lastBtnTrans = MenuPanel.transform.FindChild("QuitButton");
                Logging.Logger.Assert(lastBtnTrans != null, "", "Cannot find last button in MenuPanel");
                GameObject lastBtn = lastBtnTrans.gameObject;
                RectTransform lastBtnSize = lastBtnTrans.GetComponent<RectTransform>();
                Logging.Logger.Assert(lastBtnSize != null, "", "Cannot find RectTransform in reference button");

                // Create a copy of the button so we can alter it to do our bidding. (We want to make a copy so we don't need to re-apply all the same styling and whatnot, thus ensuring it won't break as easily in the future)
                GameObject newButton = UnityEngine.Object.Instantiate<GameObject>(lastBtnTrans.gameObject);
                Logging.Logger.Assert(newButton!= null, "", "Failed to clone last button in MenuPanel!");
                newButton.transform.SetParent(MenuPanel);

                // We need to make the Menu panel taller now so it properly fits the buttons
                var menuTrans = (MenuPanel as RectTransform);
                // Since the height of the menu buttons is relative to the height of the menu itself we want to increase the menus height by the size of a single button + the VerticalLayoutGroup's padding value + the set spacing value
                float btnH = (menuTrans.sizeDelta.y / (MenuPanel.transform.childCount - 1)) * 0.5f;
                float yInc = (btnH + layout.padding.vertical + layout.spacing);
                //MenuPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(menuTrans.sizeDelta.x, menuTrans.sizeDelta.y + yInc);
                menuTrans.anchorMin = new Vector2(menuTrans.anchorMin.x, 1f);
                menuTrans.anchorMax = new Vector2(menuTrans.anchorMax.x, 1f);

                menuTrans.sizeDelta = new Vector2(menuTrans.sizeDelta.x, menuTrans.sizeDelta.y + yInc);
                menuTrans.anchoredPosition = new Vector2(menuTrans.anchoredPosition.x, ((Screen.height * -1.0f) + (menuTrans.sizeDelta.y * 0.5f)) + ((Screen.height * 0.5f) + statusTrans.anchoredPosition.y) + (statusTrans.sizeDelta.y * 0.5f)+2f);

                /*
                // Move ALL GUI elements on the MainMenuUI down so they don't overlap the now expanded panel
                for (int i=1; i<Instance.transform.childCount; i++)
                {
                    Transform e = Instance.transform.GetChild(i);
                    Vector2 sz = ((RectTransform)lastBtnTrans).sizeDelta;
                    RectTransform rt = (RectTransform)e;
                    
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - yInc);
                }
                */

                // Set the plugins button text.
                newButton.name = name;
                newButton.GetComponentInChildren<Text>().text = text;
                
                //Setup our buttons click logic
                Button btn = newButton.GetComponent<Button>();
                
                btn.onClick.RemoveAllListeners();
                btn.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
                btn.onClick.AddListener(onclick_handler);

                var styler = newButton.AddComponent<ButtonStyler_Overrider>();
                if (color.HasValue) styler.normalColor = color.Value;
                if (hl_clr.HasValue) { styler.highlightedColor = hl_clr.Value; styler.pressedColor = hl_clr.Value; }
                if (text_clr.HasValue) styler.normalText = text_clr.Value;
            }
        }
        #endregion

        #region Debugging Functions

        /// <summary>
        /// Prints the MainMenu root object's child hierarchy
        /// </summary>
        public static void Output_Hierarchy()
        {
            SLog.Info("Main Menu Hierarchy:\n{0}", Util.Get_Unity_GameObject_Hierarchy_String(Instance.gameObject));
        }
        #endregion
    }

    /// <summary>
    /// Helper script to handle the Level_Loaded event
    /// </summary>
    internal class LevelLoaded_Handler_Script : MonoBehaviour
    {
        private void Start()
        {
            Level_Loaded();// Since by the time our plugin loader is setup the main menu level has already loaded, We need to fire the level_loaded event manually.
        }

        private void OnLevelWasLoaded(int level_idx) { Level_Loaded(level_idx); }

        internal void Level_Loaded(int level_idx = 0)
        {
            object retVal = new object();
            SiscosHooks.call(HOOK_ID.Level_Loaded, null, ref retVal, new object[] { level_idx });
        }
    }

    /// <summary>
    /// Tracks the MainMenuUI script instance, unsets it within our MainMenu class when it dies.
    /// </summary>
    internal class MainMenuUI_Tracking_Script : MonoBehaviour
    {
        private void OnDestroy()
        {
            MainMenu.Instance = null;
        }
    }
}
