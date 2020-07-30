using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    internal enum PrefabType
    {
        NONE=0,
        ENTITY,
        PLOT,
        RESOURCE,
    }

    class Entity_Pref_Hook : Prefab_Hook
    {
        void Awake() { Kind = PrefabType.ENTITY; }
        //protected override void Start() { Kind = PrefabType.ENTITY; base.Start(); }
        //protected override void OnDestroy() { Kind = PrefabType.ENTITY; base.OnDestroy(); }
    }

    class Plot_Pref_Hook : Prefab_Hook
    {
        void Awake() { Kind = PrefabType.PLOT; }
        //protected override void Start() { Kind = PrefabType.PLOT; base.Start(); }
        //protected override void OnDestroy() { Kind = PrefabType.PLOT; base.OnDestroy(); }
    }
    
    class Resource_Pref_Hook : Prefab_Hook
    {
        void Awake() { Kind = PrefabType.RESOURCE; }
        //protected override void Start() { Kind = PrefabType.RESOURCE; base.Start(); }
        //protected override void OnDestroy() { Kind = PrefabType.RESOURCE; base.OnDestroy(); }
    }


    class Prefab_Hook : MonoBehaviour
    {
        public PrefabType Kind = PrefabType.NONE;
        /// <summary>
        /// Start occurs after all other behaviour scripts for th eobject have initiated and can now be interacted with.
        /// </summary>
        protected void Start()
        {
            switch (Kind)
            {
                case PrefabType.ENTITY:
                    Handle_Entity_Spawn();
                    break;
                case PrefabType.PLOT:
                    Handle_Land_Plot_Spawn();
                    break;
                case PrefabType.RESOURCE:
                    Handle_Garden_Patch_Spawn();
                    break;
                case PrefabType.NONE:
                    break;
                default:
                    throw new ArgumentException(String.Format("Unhandled PrefabType: {0}", this.Kind));
            }
        }

        protected void OnDestroy()
        {
            switch (Kind)
            {
                case PrefabType.ENTITY:
                    Handle_Entity_Destroyed();
                    break;
                case PrefabType.PLOT:
                    Handle_Land_Plot_Destroyed();
                    break;
                case PrefabType.RESOURCE:
                    Handle_Garden_Patch_Destroyed();
                    break;
                case PrefabType.NONE:
                    break;
                default:
                    throw new ArgumentException(String.Format("Unhandled PrefabType: {0}", this.Kind));
            }

        }

        #region Spawn Handlers

        private void Handle_Entity_Spawn()
        {
            Identifiable ident = base.gameObject.GetComponent<Identifiable>();
            Identifiable.Id ID = ident ? ident.id : Identifiable.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Spawned_Entity, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsSlime(ID) || Identifiable.IsLargo(ID) || Identifiable.IsGordo(ID))
                SiscosHooks.call(HOOK_ID.Spawned_Slime, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsAnimal(ID))
                SiscosHooks.call(HOOK_ID.Spawned_Animal, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsFood(ID))
                SiscosHooks.call(HOOK_ID.Spawned_Food, base.gameObject, ref return_value, new object[] { ID });

        }

        private void Handle_Land_Plot_Spawn()
        {
            LandPlot plot = base.gameObject.GetComponent<LandPlot>();
            LandPlot.Id ID = plot ? plot.id : LandPlot.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Spawned_Land_Plot, plot, ref return_value, new object[] { ID });
        }

        private void Handle_Garden_Patch_Spawn()
        {
            SpawnResource plot = base.gameObject.GetComponent<SpawnResource>();
            SpawnResource.Id ID = plot ? plot.id : SpawnResource.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Spawned_Garden_Patch, base.gameObject, ref return_value, new object[] { ID });
        }
        #endregion
        
        #region Destroy Handlers

        private void Handle_Entity_Destroyed()
        {
            Identifiable ident = base.gameObject.GetComponent<Identifiable>();
            Identifiable.Id ID = ident ? ident.id : Identifiable.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Destroyed_Entity, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsSlime(ID) || Identifiable.IsLargo(ID) || Identifiable.IsGordo(ID))
                SiscosHooks.call(HOOK_ID.Destroyed_Slime, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsAnimal(ID))
                SiscosHooks.call(HOOK_ID.Destroyed_Animal, base.gameObject, ref return_value, new object[] { ID });

            if (Identifiable.IsFood(ID))
                SiscosHooks.call(HOOK_ID.Destroyed_Food, base.gameObject, ref return_value, new object[] { ID });

        }

        private void Handle_Land_Plot_Destroyed()
        {
            LandPlot plot = base.gameObject.GetComponent<LandPlot>();
            LandPlot.Id ID = plot ? plot.id : LandPlot.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Destroyed_Land_Plot, plot, ref return_value, new object[] { ID });
        }

        private void Handle_Garden_Patch_Destroyed()
        {
            SpawnResource plot = base.gameObject.GetComponent<SpawnResource>();
            SpawnResource.Id ID = plot ? plot.id : SpawnResource.Id.NONE;

            object return_value = new object();
            SiscosHooks.call(HOOK_ID.Destroyed_Garden_Patch, base.gameObject, ref return_value, new object[] { ID });
        }
        #endregion
    }
}
