namespace SR_PluginLoader
{
    public class Sisco_Return
    {
        /// <summary>
        ///  should the hooked function return immediately after calling it's hooks?
        /// </summary>
        public bool early_return = false;
        /// <summary>
        /// should we STOP calling other event handlers in the list (Use this if you need to prevent other plugins from interfering with yours)?
        /// </summary>
        public bool handled = false;


        public Sisco_Return(bool custom_return = false, object return_value = null, bool _abort = false, bool _handled = false)
        {
            this.early_return = _abort;
            this.handled = _handled;
        }

        public Sisco_Return(_hook_result res)
        {
            this.early_return = res.abort;
            this.handled = res.handled;
        }
    }

}
