using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SR_PluginLoader
{
    public abstract class SafeEnum
    {
        protected int id = -1;
        private static int _idx = 0;
        private string name = null;

        protected SafeEnum()
        {
            this.id = ++_idx;
        }

        protected SafeEnum(int i)
        {
            this.id = i;
            if (i >= _idx) _idx = (i + 1);
        }

        public bool Equals(SafeEnum obj)
        {
            return (this.id == obj.id);
        }

        public bool Equals(int i)
        {
            return (this.id == i);
        }

        static public explicit operator int(SafeEnum hook)
        {
            return hook.id;
        }
        /*
        static public implicit operator SafeEnum(int i)
        {
            return new SafeEnum(i);
        }
        */
        // ugh, oh god is this awful.
        // Update: not so awful now that it caches the name...
        public override string ToString()
        {
            if (this.name != null) return this.name;

            Type type = this.GetType();
            //PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != type) continue;
                var hk = (SafeEnum)field.GetValue(this);

                if (hk.id != this.id) continue;
                this.name = field.Name;
                break;
            }

            return this.name;
        }


    }
}
