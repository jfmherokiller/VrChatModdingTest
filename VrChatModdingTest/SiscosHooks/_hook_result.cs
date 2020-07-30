namespace SR_PluginLoader
{
    /// <summary>
    /// FOR INTERNAL USE ONLY, HOOKS SHOULD RETURN AN INSTANCE OF THE "Siscos_Return" CLASS!
    /// </summary>
    public class _hook_result
    {
        internal bool handled = false;
        public bool abort = false;
        //public bool has_custom_return;
        public object[] args = null;
        //public object return_value;

        public _hook_result()
        {
        }

        public _hook_result(object[] args)
        {
            this.args = args;
            //if (this.args == null) this.args = new object[0];
        }
    }
}
