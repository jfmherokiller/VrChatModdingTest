using System;

namespace SR_PluginLoader
{
    public enum cSizeMode { NONE = 0, FLOOD_X, FLOOD_Y }
    public class ControlSizer
    {
        private cSizeMode Mode;
        private float Offset = 0f;

        public ControlSizer(float offset, cSizeMode mode)
        {
            this.Offset = offset;
            this.Mode = mode;
        }

        public void Apply(uiControl control)
        {
            switch (Mode)
            {
                case cSizeMode.FLOOD_X:
                    control.FloodX(Offset);
                    break;
                case cSizeMode.FLOOD_Y:
                    control.FloodY(Offset);
                    break;
            }
        }

        public bool Equals(float off, cSizeMode d)
        {
            return (Util.floatEq(this.Offset, off) && this.Mode == d);
        }

        public override string ToString()
        {
            return String.Format("["+nameof(ControlSizer)+"] MODE: {0}  OFFSET: {1} ", Enum.GetName(typeof(cSizeMode), Mode), Offset);
        }
    }

}
