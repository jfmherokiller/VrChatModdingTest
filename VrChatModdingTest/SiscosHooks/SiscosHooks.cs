using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Hook functions may return either NULL or an instance of 'Sisco_Return'.
    /// </summary>
    /// <param name="sender">the triggering functions 'this' instance.</param>
    /// <param name="args">reference to the triggering functions args list.</param>
    /// <param name="return_value">reference to the value currently set to be returned by the function that fired this event.</param>
    /// <returns></returns>
    public delegate Sisco_Return Sisco_Hook_Delegate(ref object sender, ref object[] args, ref object return_value);

    /// <summary>
    /// yeah I named it after myself, wanna fight about it? Tough guy?!?
    /// </summary>
    public static class SiscosHooks
    {
        /// <summary>
        /// This is a table that tracks the number of hooks each event id has, this allows us the most efficient way to determine if an early abort on event firing is possible by limiting instructions at the head of the 'Call' function.
        /// </summary>
        private static int[] EventCounter = null;

        /// <summary>
        /// (FOR DEBUG PURPOSES)
        /// This is a list of hooks which when fired, we want to print a log messge for so we can verify they are working.
        /// </summary>
        public static HashSet<HOOK_ID> HOOKS_TO_ANNOUNCE = new HashSet<HOOK_ID>();
        /// <summary>
        /// This is the map of all registered events.
        /// It is ordered by HOOK_ID so firing off events is quick.
        /// </summary>
        private static Dictionary<HOOK_ID, Dictionary<HookUID, Sisco_Hook_Delegate>> Events = new Dictionary<HOOK_ID, Dictionary<HookUID, Sisco_Hook_Delegate>>();
        /// <summary>
        /// Maps Plugin/Assembly instances to a list of all their registered hooks (so we can unregister all of them at once without itterating through the entire global <see cref="Events"/> list).
        /// </summary>
        private static Dictionary<object, List<HookUID>> Tracker = new Dictionary<object, List<HookUID>>();
        /// <summary>
        /// Maps <see cref="Assembly"/> FullNames to their instances.
        /// </summary>
        private static Dictionary<string, object> assembly_registrars = new Dictionary<string, object>();

        internal static void Setup()
        {
            SLog.Info("[SiscosHooks] Initialized...");
            int max = HOOK_ID.Count;
            // foreach(var hook in HOOKS.HooksList) { max = Math.Max(max, hook.id); }
            
            EventCounter = new int[max];
            for(int i=0; i<EventCounter.Length; i++)
            {
                EventCounter[i] = 0;
            }

            #region Setup Event Extension Proxys
            register(HOOK_ID.Ext_Game_Saved, HookProxys.Ext_Game_Saved);
            register(HOOK_ID.Ext_Pre_Game_Loaded, HookProxys.Ext_Pre_Game_Loaded);
            register(HOOK_ID.Ext_Post_Game_Loaded, HookProxys.Ext_Post_Game_Loaded);
            register(HOOK_ID.Ext_Demolish_Plot, HookProxys.Ext_Demolish_Plot_Upgrade);
            register(HOOK_ID.Ext_Spawn_Plot_Upgrades_UI, HookProxys.Ext_Spawn_Plot_Upgrades_UI);
            register(HOOK_ID.Ext_Identifiable_Spawn, HookProxys.Ext_Identifiable_Spawn);
            register(HOOK_ID.Ext_Player_Death, HookProxys.Ext_Player_Death);
            register(HOOK_ID.Ext_LockOnDeath_Start, HookProxys.Ext_LockOnDeath_Start);
            register(HOOK_ID.Ext_LockOnDeath_End, HookProxys.Ext_LockOnDeath_End);
            #endregion

            #region Hook Prefab Instantiation Events
            Util.Inject_Into_Prefabs<Entity_Pref_Hook>(Ident.ALL_IDENTS);
            Util.Inject_Into_Prefabs<Plot_Pref_Hook>(Ident.ALL_PLOTS);
            Util.Inject_Into_Prefabs<Resource_Pref_Hook>(Ident.ALL_GARDEN_PATCHES);
            #endregion
            
        }

        public static _hook_result call(HOOK_ID hook, object sender, ref object returnValue, object[] args)
        {
            try
            {
#if DEBUG
                SLog.Info("[SiscosHooks] {0}({1})", hook, Get_Arg_String(args));
#endif

                _hook_result result = new _hook_result(args);
                Dictionary<HookUID, Sisco_Hook_Delegate> cb_list;
                bool r = Events.TryGetValue((HOOK_ID)hook, out cb_list);
                if (r == false) return new _hook_result();//no abort

                List<HookUID> TRASH = null;

                foreach (KeyValuePair<HookUID, Sisco_Hook_Delegate> kvp in cb_list)
                {
                    try
                    {
                        HookUID UID = kvp.Key;
                        UID.hasFired = true;

                        Sisco_Hook_Delegate act = kvp.Value;
                        if (act == null) continue;
                        Sisco_Return ret = act(ref sender, ref result.args, ref returnValue);
                        // If this hook is a singleuse hook then we need to put it in our trashbin so we know to unregister it at the end of this function.
                        if (UID.SingleUse)
                        {
                            if (TRASH == null) TRASH = new List<HookUID>();
                            TRASH.Add(UID);
                        }

                        if (ret != null)
                        {
                            if (ret.early_return) result.abort = true;
                            if (ret.handled == true)
                            {
                                result.handled = true;
                                break;//cancel all other events
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Log(hook, ex.Message);
                        Log(ex.StackTrace);
                    }
                }

                if(TRASH!=null)
                {
                    foreach(HookUID uid in TRASH)
                    {
                        try { unregister(uid); } catch (Exception ex) { Log(uid.hook, DebugHud.Format_Exception_Log(ex)); }
                    }
                }

                if(result == null)
                {
                    Log(hook, "Result became NULL somehow!");
                    return new _hook_result();// we MUST return something other then NULL or the whole game can screw up!
                }

                if(args != null && args.Length != result.args.Length)
                {
                    Log(hook, "The size of Result.args does not match the number of arguments recieved from the function!");
                }
                
                return result;
            }
            catch(Exception ex)
            {
                Log(hook, ex.Message);
                Log(ex.StackTrace);
                return new _hook_result();
            }

            return new _hook_result();//no abort
        }


        #region Internal
        internal static object Get_Assembly_Registrar(Assembly asy)
        {
            string key = asy.FullName;
            object value;
            if (!assembly_registrars.TryGetValue(key, out value))
            {
                value = new object();
                assembly_registrars.Add(key, value);
            }

            if (!SiscosHooks.Tracker.ContainsKey(value)) SiscosHooks.Tracker.Add(value, new List<HookUID>());

            return value;
        }

        #endregion

        #region REGISTRATION LOGIC
        
        //[Obsolete("Use register(Hook_ID, Callback, Registrar) or register(Hook_ID, Callback) instead!", true)]
        //public static bool register(object registrar, HOOK_ID hook, Sisco_Hook_Delegate cb) { return (register(hook, cb, null) != null); }
        
        /// <summary>
        /// Register your own function to be called whenever a specified event triggers.
        /// </summary>
        /// <param name="registrar">Identifier used for grouping many hooks into a category for efficient removal later.</param>
        /// <param name="hook">The event to hook.</param>
        /// <param name="cb">The function to call.</param>
        /// <returns>A <see cref="HookUID"/> for the newly registered hook or <c>null</c> upon failure.</returns>
        public static HookUID register(HOOK_ID hook, Sisco_Hook_Delegate cb, object registrar=null)
        {
            HookUID UID = null;
            if (registrar == null) registrar = Get_Assembly_Registrar( Assembly.GetCallingAssembly() );
            if(hook == null)
            {
                Log("Attempted to register for NULL event!");
                return null;
            }
            
            try
            {
                // create the callback list for this hook type if it doesn't exist.
                if (!SiscosHooks.Events.ContainsKey(hook)) SiscosHooks.Events[hook] = new Dictionary<HookUID, Sisco_Hook_Delegate>();
                UID = new HookUID(hook, registrar);
                SiscosHooks.Events[hook].Add(UID, cb);
                EventCounter[(int)hook] = SiscosHooks.Events[hook].Count;

                if (registrar != null)
                {
                    // create this registrar's hooks list if it doesn't exist.
                    if (!SiscosHooks.Tracker.ContainsKey(registrar)) return null;
                    //add this hook to their list.
                    SiscosHooks.Tracker[registrar].Add(UID);
                }
                return UID;
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            return null;
        }

        /// <summary>
        /// Registers a hook that will fire only once and then be removed.
        /// </summary>
        /// <param name="evt">The event to hook.</param>
        /// <param name="cb">The callback to fire for this event.</param>
        /// <returns></returns>
        public static HookUID Once(HOOK_ID evt, Sisco_Hook_Delegate cb)
        {
            HookUID hook = register(evt, cb);
            hook.SingleUse = true;
            return hook;
        }

        /// <summary>
        /// Allows registering single-use hook with a void callback.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static HookUID Once(HOOK_ID evt, Action cb)
        {
            return Once(evt, (ref object sender, ref object[] args, ref object retVal) => { cb(); return null; });
        }

        [Obsolete("Use unregister(HookUID) instead, All registered hooks are now assigned a unique HookUID value to identify them. Removing a hook based on it's assigned callback function can result in unexpected behaviour!")]
        /// <summary>
        /// Unhook a previous hook you installed.
        /// </summary>
        /// <param name="registrar">Unique identifier used for grouping many hooks into a category for efficient removal later.</param>
        /// <param name="hook">Id of the event to unhook.</param>
        /// <param name="cb">The function to call.</param>
        /// <returns>(BOOL) Whether the event was successfully unhooked.</returns>
        public static bool unregister(object registrar, HOOK_ID hook, Sisco_Hook_Delegate cb)
        {
            if (registrar == null) registrar = Get_Assembly_Registrar(Assembly.GetCallingAssembly());
            HookUID UID = null;
            try
            {
                bool hk_success = false;
                if (SiscosHooks.Events.ContainsKey(hook))
                {
                    // Since all hooks are given a unique id and mapped to it in the events list now, we need to find this delegates id and remove it that way.
                    UID = SiscosHooks.Events[hook].FirstOrDefault(kv => (kv.Value==cb)).Key;
                    if(UID != null) hk_success = SiscosHooks.Events[hook].Remove(UID);
                    if (!hk_success)
                    {
                        Log("Failed to remove hook from Events list: {0}", UID);
                        return false;
                    }
                    else
                    {
                        // Update the number of registered instances for this event.
                        EventCounter[(int)hook] = SiscosHooks.Events[hook].Count;
                    }
                }

                bool tr_success = false;
                if (SiscosHooks.Tracker.ContainsKey(registrar))
                {
                    //add this hook to their list.
                    tr_success = SiscosHooks.Tracker[registrar].Remove(UID);
                    if (!tr_success)
                    {
                        Log("Failed to remove {0} from tracker.", UID);
                    }
                }

                return (tr_success && hk_success);
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            return false;
        }
        
        /// <summary>
        /// Unregisters a previously installed hook.
        /// </summary>
        /// <returns>(BOOL) Whether the hook was successfully removed.</returns>
        public static bool unregister(HookUID id)
        {
            try
            {
                bool hk_success = false;
                if (SiscosHooks.Events.ContainsKey(id.hook))
                {
                    hk_success = SiscosHooks.Events[id.hook].Remove(id);
                    if (!hk_success)
                    {
                        Log("Failed to remove hook from hooks list. Sender({0})", id.registrar);
                        return false;
                    }
                    else
                    {
                        // Update the number of registered instances for this event.
                        EventCounter[(int)id.hook] = SiscosHooks.Events[id.hook].Count;
                    }
                }

                bool tr_success = false;
                if (SiscosHooks.Tracker.ContainsKey(id.registrar))
                {
                    //add this hook to their list.
                    tr_success = SiscosHooks.Tracker[id.registrar].Remove(id);
                    if (!tr_success)
                    {
                        Log("Failed to remove {0} from tracker.", id);
                    }
                }

                return (tr_success && hk_success);
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            return false;
        }


        /// <summary>
        /// Unhook ALL of the previous hooks you installed with a specified registrar object.
        /// </summary>
        /// <param name="registrar">Unique identifier used for grouping many hooks into a category for efficient removal later.</param>
        /// <param name="hook">Id of the event to unhook. Leave this blank to remove ALL hooked events.</param>
        /// <returns>(BOOL) Whether the events was successfully unhooked.</returns>
        public static bool unregister_all(object registrar)
        {
            if (registrar == null) registrar = Get_Assembly_Registrar(Assembly.GetCallingAssembly());

            try
            {
                // create this registrar's hooks list if it doesn't exist.
                List<HookUID> hooks_list;
                if (!SiscosHooks.Tracker.ContainsKey(registrar)) return false;
                SiscosHooks.Tracker.TryGetValue(registrar, out hooks_list);

                var trash = new List<HookUID>(hooks_list);
                foreach (HookUID uid in trash)
                {
                    bool b = unregister(uid);
                    if(!b)
                    {
                        Log("Failed to unregister {0}", uid);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }

            return false;
        }


        /// <summary>
        /// Unhook ALL of the previous hooks you installed with a specified registrar object.
        /// </summary>
        /// <param name="registrar">Unique identifier used for grouping many hooks into a category for efficient removal later.</param>
        /// <param name="hook">Id of the event to unhook. Leave this blank to remove ALL hooked events.</param>
        /// <returns>(BOOL) Whether the events was successfully unhooked.</returns>
        public static bool unregister_all(object registrar, HOOK_ID hook)
        {
            if (registrar == null) registrar = Get_Assembly_Registrar(Assembly.GetCallingAssembly());

            try
            {
                // create the callback list for this hook type if it doesn't exist.
                if (!SiscosHooks.Events.ContainsKey(hook)) SiscosHooks.Events[hook] = new Dictionary<HookUID, Sisco_Hook_Delegate>();

                // create this registrar's hooks list if it doesn't exist.
                List<HookUID> hooks_list;
                if (!SiscosHooks.Tracker.ContainsKey(registrar)) return false;
                SiscosHooks.Tracker.TryGetValue(registrar, out hooks_list);

                var trash = new List<HookUID>(hooks_list);
                foreach (HookUID UID in trash)
                {
                    if (hook != HOOK_ID.NONE && UID.hook != hook) continue;
                    bool b = unregister(UID);
                    if (!b)
                    {
                        Log("Failed to unregister {0}", UID);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            return false;
        }

        #endregion

        #region HELPERS
        public static string Get_Arg_String(object[] args)
        {
            if (args != null)
            {
                List<string> arglist = new List<string>();
                
                foreach (object arg in args)
                {
                    if(arg == null) arglist.Add( "null" );
                    else arglist.Add( arg.ToString() );
                }

                return String.Join(", ", arglist.ToArray());
            }

            return "NULL";
        }

        public static string SerializeObject<T>(this T toSerialize)
        {
            if (toSerialize == null) return "null";
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        #endregion

        #region LOGGING
        //These are just a bunch of functions to wrap around the logging system calls so the Event Hooks system can print customized log messages

        private const string LOG_TAG = "<b>SiscosHooks</b>  ";
        private static void Log(HOOK_ID hook, string format, params object[] args)
        {
            SLog.Info(String.Format("{0}<{1}> {2}", LOG_TAG, hook.ToString(), format), args);
        }

        private static void Log(string format, params object[] args)
        {
            SLog.Info(String.Format("{0} {1}", LOG_TAG, format), args);
        }

        private static void Log(Exception ex)
        {
            string str = Logging.Logger.Format_Exception(ex);
            SLog.Error("{0}(Exception) {1}", LOG_TAG, str);
        }

#endregion

    }

    #region PROXIES

    /// <summary>
    /// Here is where we keep any event hook extension proxys (Too keep things tidy!)
    /// An event hook extension proxy is a proxy function that extends or builds upon the information provided by a default hook coming from the generic hook system.
    /// This allows us to provide more intelligent and useful hooks to plugin makers!
    /// How does it work? Well to be honest I don't know, but I suspect magic...
    /// </summary>
    internal static class HookProxys
    {
        internal static LandPlot.Id Get_Plot_ID_From_Upgrades_UI_Class(object sender)
        {
            LandPlot.Id kind = LandPlot.Id.NONE;
            Type type = sender.GetType();
            if (type == typeof(GardenUI)) kind = LandPlot.Id.GARDEN;
            else if (type == typeof(CoopUI)) kind = LandPlot.Id.COOP;
            else if (type == typeof(CorralUI)) kind = LandPlot.Id.CORRAL;
            else if (type == typeof(PondUI)) kind = LandPlot.Id.POND;
            else if (type == typeof(SiloUI)) kind = LandPlot.Id.SILO;
            else if (type == typeof(IncineratorUI)) kind = LandPlot.Id.INCINERATOR;

            return kind;
        }

        internal static Sisco_Return Ext_Demolish_Plot_Upgrade(ref object sender, ref object[] args, ref object return_value)
        {
#if !SR_VANILLA
            LandPlot.Id kind = Get_Plot_ID_From_Upgrades_UI_Class(sender);
            LandPlotUI ui = sender as LandPlotUI;
            LandPlot plot = ui.Get_LandPlot();
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Demolished_Land_Plot, plot, ref return_value, new object[] { kind }));
#else
            return null;
#endif
        }

        internal static Sisco_Return Ext_Spawn_Plot_Upgrades_UI(ref object sender, ref object[] args, ref object return_value)
        {
            LandPlot.Id kind = Get_Plot_ID_From_Upgrades_UI_Class(sender);
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Spawn_Plot_Upgrades_UI, sender, ref return_value, new object[] { kind }));
        }

        internal static Sisco_Return Ext_Pre_Game_Loaded(ref object sender, ref object[] args, ref object return_value)
        {
            string saveFile = GameData.ToPath(args[0] as string);
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Pre_Game_Loaded, sender, ref return_value, new object[] { saveFile }));
        }

        internal static Sisco_Return Ext_Post_Game_Loaded(ref object sender, ref object[] args, ref object return_value)
        {
            string saveFile = GameData.ToPath(args[0] as string);
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Post_Game_Loaded, sender, ref return_value, new object[] { saveFile }));
        }

        internal static Sisco_Return Ext_Game_Saved(ref object sender, ref object[] args, ref object return_value)
        {
            string saveFile = GameData.ToPath((sender as GameData).gameName);
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Game_Saved, sender, ref return_value, new object[] { saveFile }));
        }

        internal static Sisco_Return Ext_Identifiable_Spawn(ref object sender, ref object[] args, ref object return_value)
        {
            Identifiable ident = sender as Identifiable;
            if (ident.id == Identifiable.Id.PLAYER)
            {
                SiscosHooks.call(HOOK_ID.Player_Spawn, ident.gameObject, ref return_value, args);
            }
            return null;
        }

        internal static bool is_player_dead = false;
        internal static Sisco_Return Ext_Player_Death(ref object sender, ref object[] args, ref object return_value)
        {
            is_player_dead = true;
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Player_Death, sender, ref return_value, args));
        }

        // The LockOnDeath class is used to lock player input for the game
        // This means that it is used both when the player "goes to sleep" and when they die
        // So we can use it to differentiate between the two
        internal static Sisco_Return Ext_LockOnDeath_Start(ref object sender, ref object[] args, ref object return_value)
        {
            return new Sisco_Return(SiscosHooks.call(HOOK_ID.Player_Sleep_Begin, sender, ref return_value, args));
        }

        internal static Sisco_Return Ext_LockOnDeath_End(ref object sender, ref object[] args, ref object return_value)
        {
            if (is_player_dead)
            {
                is_player_dead = false;
                return new Sisco_Return(SiscosHooks.call(HOOK_ID.Player_Spawn, sender, ref return_value, new object[] {}));
            }
            else
            {
                return new Sisco_Return(SiscosHooks.call(HOOK_ID.Player_Sleep_End, sender, ref return_value, args));
            }
        }


    }
#endregion
}
