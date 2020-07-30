using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SR_PluginLoader
{
    /*
    THIS IS FOR INTERNAL USE BY THE LIBRARY ONLY
    I WILL NOT BE DOCUMENTING THIS AREA
    */
    public enum debug_positioning
    {
        Instruction = 0,
        OpCode,
        Branch_Start,
        Branch_Exit,
        Cond_Branch_Start,
        Cond_Branch_Exit,
        Field_Ref,
        Method_Ref
    }

    [DebuggerDisplay("{hook.name} @ {name}")]
    public class Hook_Dbg_Data
    {
        public HOOK_ID hook = HOOK_ID.NONE;
        public HOOK_ID ext = HOOK_ID.NONE;
        public int id { get { return (int)hook; } set { this.hook = (HOOK_ID)value; } }
        public string name = null;
        /// <summary>
        /// Means the hook cannot cancel the event.
        /// </summary>
        public bool is_post = false;
        public debug_positioning method = debug_positioning.Instruction;
        public debug_positioning relative_method = debug_positioning.Instruction;
        public int pos = 0;
        public int relative = 0;
        public string arg = null;

        public Hook_Dbg_Data()
        {
        }
    }


    /// <summary>
    /// Contains a list of all hooks and their method translations for debugging purposes
    /// </summary>
    public static class HOOKS
    {
        public static Hook_Dbg_Data[] HooksList = new Hook_Dbg_Data[] {

            new Hook_Dbg_Data() { hook = HOOK_ID.VacPak_Consume, name = "WeaponVacuum.ConsumeVacItem" },
            new Hook_Dbg_Data() { hook = HOOK_ID.VacPak_Can_Capture, name = "Vacuumable.canCapture" },
            new Hook_Dbg_Data() { hook = HOOK_ID.VacPak_Capture, name = "Vacuumable.capture" },
            new Hook_Dbg_Data() { hook = HOOK_ID.VacPak_Think, name = "WeaponVacuum.Update" },

            new Hook_Dbg_Data() { hook = HOOK_ID.Game_Saved, name = "GameData.Write", pos = 0, ext = HOOK_ID.Ext_Game_Saved },
            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Game_Loaded, name = "GameData.Load", pos = 0, ext = HOOK_ID.Ext_Pre_Game_Loaded },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Game_Loaded, name = "GameData.Load", pos = -1, ext = HOOK_ID.Ext_Post_Game_Loaded },

            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Entity_Spawn, name = "DirectedActorSpawner.Spawn" },
            new Hook_Dbg_Data() { hook = HOOK_ID.EntitySpawner_Init, name = "DirectedActorSpawner.Start" },

            new Hook_Dbg_Data() { hook = HOOK_ID.Player_CanBuyUpgrade, name = "PlayerState.CanGetUpgrade", pos=-1 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_ApplyUpgrade, name="PlayerState.ApplyUpgrade", pos=-1 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_Damaged, name = "PlayerState.Damage" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_LoseEnergy, name = "PlayerState.SpendEnergy" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_SetEnergy, name = "PlayerState.SetEnergy" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_MoneyChanged, name = "PlayerState.AddCurrency" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_AddRads, name = "PlayerState.AddRads" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_Spawn, name = "Identifiable.Awake", pos=-1, ext = HOOK_ID.Ext_Identifiable_Spawn },
            new Hook_Dbg_Data() { hook = HOOK_ID.Player_Death, name = "PlayerDeathHandler.OnDeath", pos=-1, ext = HOOK_ID.Ext_Player_Death },

            new Hook_Dbg_Data() { hook = HOOK_ID.Ext_LockOnDeath_Start, name = "LockOnDeath.LockUntil", pos=-1 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Ext_LockOnDeath_End, name = "LockOnDeath.Update", pos=1, is_post=true, method = debug_positioning.Cond_Branch_Exit, relative = -1 },

            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Region_Spawn_Cycle, name = "CellDirector.Update", pos = 4, method = debug_positioning.Cond_Branch_Start },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Region_Spawn_Cycle, name = "CellDirector.Update", pos = 4, method = debug_positioning.Cond_Branch_Exit, relative = -1 },


            new Hook_Dbg_Data() { hook = HOOK_ID.Plot_Load_Upgrades, name = "LandPlot.SetUpgrades", pos=-1 },

            #region Upgrade UI Spawns
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Player_Upgrades_UI, name = "PersonalUpgradeUI.CreatePurchaseUI", pos=-1 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "EmptyPlotUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "GardenUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "CorralUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "CoopUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "PondUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "SiloUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            new Hook_Dbg_Data() { hook = HOOK_ID.Spawn_Plot_Upgrades_UI, name = "IncineratorUI.CreatePurchaseUI", pos=-1, ext=HOOK_ID.Ext_Spawn_Plot_Upgrades_UI },
            #endregion

            #region Upgrade UI Demolish
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "GardenUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "CorralUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "CoopUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "PondUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "SiloUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            new Hook_Dbg_Data() { hook = HOOK_ID.Demolished_Land_Plot, name = "IncineratorUI.Demolish", pos=1, relative = -1, method = debug_positioning.Cond_Branch_Exit, ext=HOOK_ID.Ext_Demolish_Plot },
            #endregion


            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Economy_Init, name = "EconomyDirector.InitForLevel", pos=0 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Economy_Init, name = "EconomyDirector.InitForLevel", pos=-1 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Economy_Updated, name = "EconomyDirector.Update", pos=-1, is_post=true, method = debug_positioning.Cond_Branch_Exit, relative=-1 },

            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Silo_Input, name = "SiloCatcher.OnTriggerEnter", pos=-2, method = debug_positioning.Cond_Branch_Start },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Silo_Input, name = "SiloCatcher.OnTriggerEnter", pos=-2, is_post=true, method = debug_positioning.Cond_Branch_Exit, relative=-1 },

            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Silo_Output, name = "SiloCatcher.OnTriggerStay", pos=-2, method = debug_positioning.Cond_Branch_Start },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Silo_Output, name = "SiloCatcher.OnTriggerStay", pos=-2, is_post=true, method = debug_positioning.Cond_Branch_Exit, relative=-1 },

            new Hook_Dbg_Data() { hook = HOOK_ID.ResourcePatch_Init, name = "SpawnResource.Start", pos=-1 },

            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Garden_Init, name = "GardenCatcher.Awake", pos=0 },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Garden_Init, name = "GardenCatcher.Awake", pos=-1 },
            
            //new Hook_Dbg_Data() { hook = HOOK_ID.Garden_Got_Input, name = "GardenCatcher.OnTriggerEnter" },
            new Hook_Dbg_Data() { hook = HOOK_ID.Garden_Got_Input, name = "GardenCatcher.OnTriggerEnter", pos=1, method = debug_positioning.Cond_Branch_Exit },
            new Hook_Dbg_Data() { hook = HOOK_ID.Pre_Garden_Set_Type, name = "GardenCatcher.OnTriggerEnter", pos=-2, method = debug_positioning.Cond_Branch_Start },
            new Hook_Dbg_Data() { hook = HOOK_ID.Post_Garden_Set_Type, name = "GardenCatcher.OnTriggerEnter", pos=-2, is_post=true, method = debug_positioning.Cond_Branch_Exit, relative=-1 },
        };
    }
}
