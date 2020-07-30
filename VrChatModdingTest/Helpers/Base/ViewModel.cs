using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SR_PluginLoader
{
    /// <summary>
    /// Provides helper functions to Enable / Disable the ViewModel
    /// </summary>
    public static class ViewModel
    {
        private static GameObject _root_cached = null;
        public static GameObject Root { get { if (_root_cached == null) { vp_Weapon weap = GameObject.FindObjectsOfType<vp_Weapon>().FirstOrDefault(); if (weap != null) { _root_cached = weap.gameObject; } } return _root_cached; } }
        public static bool isActive { get { if (Root != null) { return Root.activeSelf; } throw new ArgumentNullException("Cannot locate HUD Root!"); } }


        public static void Toggle()
        {
            if (Root == null) throw new ArgumentNullException("Cannot locate ViewModel Root!");
            Root.SetActive(!Root.activeSelf);
        }

        public static void Toggle(bool en)
        {
            if (Root == null) throw new ArgumentNullException("Cannot locate ViewModel Root!");
            Root.SetActive(en);
        }

        public static void Enable()
        {
            if (Root == null) throw new ArgumentNullException("Cannot locate ViewModel Root!");
            Root.SetActive(true);
        }

        public static void Disable()
        {
            if (Root == null) throw new ArgumentNullException("Cannot locate ViewModel Root!");
            Root.SetActive(false);
        }
    }
}
