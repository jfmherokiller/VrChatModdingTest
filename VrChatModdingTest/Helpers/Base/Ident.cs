using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    public static class Ident
    {
        private static Dictionary<Identifiable.Id, GameObject> prefabDict = null;
        public static GameObject GetPrefab(Identifiable.Id ID)
        {
            if(prefabDict == null)
            {
                prefabDict = new Dictionary<Identifiable.Id, GameObject>();
                foreach(GameObject prefab in Directors.lookupDirector.identifiablePrefabs)
                {
                    if (prefab == null) continue;
                    Identifiable ident = prefab.GetComponent<Identifiable>();
                    if (ident == null) continue;
                    if (prefabDict.ContainsKey(ident.id)) continue;

                    prefabDict.Add(ident.id, prefab);
                }
            }

            GameObject obj;
            if (!prefabDict.TryGetValue(ID, out obj)) return null;

            return obj;
        }

        private static HashSet<Identifiable.Id> _all_idents = null;
        public static HashSet<Identifiable.Id> ALL_IDENTS
        {
            get
            {
                if (_all_idents == null)
                {
                    _all_idents = new HashSet<Identifiable.Id>();
                    foreach(Identifiable.Id id in Enum.GetValues(typeof(Identifiable.Id)))
                    {
                        switch(id)
                        {
                            case Identifiable.Id.NONE:
                                break;
                            default:
                                _all_idents.Add(id);
                                break;
                        }
                    }
                }
                return _all_idents;
            }
        }

        private static HashSet<LandPlot.Id> _all_plots = null;
        public static HashSet<LandPlot.Id> ALL_PLOTS
        {
            get
            {
                if (_all_plots == null)
                {
                    _all_plots = new HashSet<LandPlot.Id>();
                    foreach (LandPlot.Id id in Enum.GetValues(typeof(LandPlot.Id)))
                    {
                        switch (id)
                        {
                            case LandPlot.Id.NONE:
                                break;
                            default:
                                _all_plots.Add(id);
                                break;
                        }
                    }
                }
                return _all_plots;
            }
        }

        private static HashSet<SpawnResource.Id> _all_garden_patches = null;
        public static HashSet<SpawnResource.Id> ALL_GARDEN_PATCHES
        {
            get
            {
                if (_all_garden_patches == null)
                {
                    _all_garden_patches = new HashSet<SpawnResource.Id>();
                    foreach (SpawnResource.Id id in Enum.GetValues(typeof(SpawnResource.Id)))
                    {
                        switch (id)
                        {
                            case SpawnResource.Id.NONE:
                                break;
                            default:
                                _all_garden_patches.Add(id);
                                break;
                        }
                    }
                }
                return _all_garden_patches;
            }
        }


        private static HashSet<Identifiable.Id> _all_slimes = null;
        public static HashSet<Identifiable.Id> ALL_SLIMES
        {
            get
            {
                if (_all_slimes == null) _all_slimes = Util.Combine_Ident_Lists(new HashSet<Identifiable.Id>[] { Identifiable.SLIME_CLASS, Identifiable.LARGO_CLASS, Identifiable.GORDO_CLASS });
                return _all_slimes;
            }
        }

        private static HashSet<Identifiable.Id> _all_animals = null;
        public static HashSet<Identifiable.Id> ALL_ANIMALS
        {
            get
            {
                if (_all_animals == null) _all_animals = Util.Combine_Ident_Lists(new HashSet<Identifiable.Id>[] { Identifiable.CHICK_CLASS, Identifiable.MEAT_CLASS });
                return _all_animals;
            }
        }

        private static Dictionary<Identifiable.Id, bool> _is_rad_cache = new Dictionary<Identifiable.Id, bool>();
        public static bool IsRadType(Identifiable.Id id)
        {
            if (!_is_rad_cache.ContainsKey(id))
            {
                var pref = Ident.GetPrefab(id);
                // If the prefab comes with a RadSource script then it's a radiation slime...
                GameObject inst = (GameObject)GameObject.Instantiate(pref, Vector3.zero, Quaternion.identity);

                var comp_list = inst.GetComponentsInChildren<Component>(true);
                bool found = false;
                foreach(var comp in comp_list)
                {
                    if(String.Compare(comp.name, "Radiation(Clone)") ==0)
                    {
                        found = true;
                        break;
                    }
                }
                _is_rad_cache.Add(id, found);
                GameObject.Destroy(inst);
            }

            return _is_rad_cache[id];
        }

        public static bool IsVeggie(Identifiable.Id id) { return Identifiable.VEGGIE_CLASS.Contains(id); }
        public static bool IsFruit(Identifiable.Id id) { return Identifiable.FRUIT_CLASS.Contains(id); }
    }
}
