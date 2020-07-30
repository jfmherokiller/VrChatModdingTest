using System;

namespace SR_PluginLoader
{

    public enum cPosDir { NONE = 0, ABOVE, BELOW, LEFT, RIGHT, TOP_OF, BOTTOM_OF, LEFT_SIDE_OF, RIGHT_SIDE_OF, CENTER_X, CENTER_Y, SIT_ABOVE, SIT_BELOW, SIT_LEFT_OF, SIT_RIGHT_OF }
    
    public class ControlPositioner : IDisposable
    {
        private cPosDir dir;
        private float offset = 0f;
        private uiControl target;
        public ControlPositioner(uiControl c, float offset, cPosDir dir)
        {
            this.target = c;
            this.offset = offset;
            this.dir = dir;
        }

        public void Dispose()
        {

        }

        public void Apply(uiControl control)
        {
            switch (dir)
            {
                case cPosDir.ABOVE:
                    control.moveAbove(target, offset);
                    break;
                case cPosDir.BELOW:
                    control.moveBelow(target, offset);
                    break;
                case cPosDir.LEFT:
                    control.moveLeftOf(target, offset);
                    break;
                case cPosDir.RIGHT:
                    control.moveRightOf(target, offset);
                    break;
                case cPosDir.TOP_OF:
                    control.alignTop(offset);
                    break;
                case cPosDir.BOTTOM_OF:
                    control.alignBottom(offset);
                    break;
                case cPosDir.LEFT_SIDE_OF:
                    control.alignLeftSide(offset);
                    break;
                case cPosDir.RIGHT_SIDE_OF:
                    control.alignRightSide(offset);
                    break;
                case cPosDir.CENTER_X:
                    control.CenterHorizontally();
                    break;
                case cPosDir.CENTER_Y:
                    control.CenterVertically();
                    break;
                case cPosDir.SIT_ABOVE:
                    control.sitAbove(target, offset);
                    break;
                case cPosDir.SIT_BELOW:
                    control.sitBelow(target, offset);
                    break;
                case cPosDir.SIT_LEFT_OF:
                    control.sitLeftOf(target, offset);
                    break;
                case cPosDir.SIT_RIGHT_OF:
                    control.sitRightOf(target, offset);
                    break;
            }
        }

        public bool Equals(uiControl targ, float off, cPosDir d)
        {
            return (this.target == targ && Util.floatEq(this.offset, off) && this.dir == d);
        }

        public override string ToString()
        {
            return String.Format("[ControlPositioner] DIR: {0}  OFFSET: {1}  TARGET: {2}", Enum.GetName(typeof(cPosDir), dir), offset, target);
        }
    }

}
