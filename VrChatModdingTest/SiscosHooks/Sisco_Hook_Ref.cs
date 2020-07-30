namespace SR_PluginLoader
{
    public class Sisco_Hook_Ref
    {
        public HOOK_ID evt = HOOK_ID.NONE;
        public Sisco_Hook_Delegate callback = null;

        public Sisco_Hook_Ref(HOOK_ID e, Sisco_Hook_Delegate cb)
        {
            this.evt = e;
            this.callback = cb;
        }
    }
}
