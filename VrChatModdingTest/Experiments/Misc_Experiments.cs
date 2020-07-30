using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    class Misc_Experiments
    {
        public static void Find_Common_Classes_For_Idents(HashSet<Identifiable.Id> ID_LIST)
        {
            Dictionary<string, int> TALLY = new Dictionary<string, int>();
            //Lookup the prefab for each id and grab a list of all classes it has on it

            foreach(Identifiable.Id id in ID_LIST)
            {
                var pref = Directors.lookupDirector.GetPrefab(id);
                GameObject inst = (GameObject)GameObject.Instantiate(pref, Vector3.zero, Quaternion.identity);
                
                MonoBehaviour[] class_list = inst.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var cInst in class_list.DistinctBy(o => o.name))
                {
                    if (!TALLY.ContainsKey(cInst.name)) TALLY.Add(cInst.name, 0);
                    TALLY[cInst.name]++;
                }

                GameObject.Destroy(inst);
            }

            SLog.Info("==== Results ====");
            foreach(KeyValuePair<string, int> kvp in TALLY.OrderByDescending(o => o.Value))
            {
                float pct = ((float)kvp.Value / ID_LIST.Count);
                SLog.Info("{0}: {1:P1}", kvp.Key, pct);
            }
            SLog.Info("=================");

        }


    }
}
