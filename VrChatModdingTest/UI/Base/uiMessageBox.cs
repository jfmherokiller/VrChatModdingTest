using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SR_PluginLoader
{
    public class uiMessageBox : uiDialogResult
    {
        public uiMessageBox() { }

        public static uiMessageBox New(string message, DialogResultDelegate callback)
        {
            var bx = Create<uiMessageBox>();
            bx.Text = message;
            bx.onResult += callback;
            return bx;
        }

        public static uiMessageBox New(string message, string title, DialogResultDelegate callback)
        {
            var bx = Create<uiMessageBox>();
            bx.Text = message;
            bx.Title = title;
            bx.onResult += callback;
            return bx;
        }
    }
}
