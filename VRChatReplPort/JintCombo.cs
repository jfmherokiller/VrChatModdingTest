using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jint;
using Jint.Runtime.Interop;
using MelonLoader;

namespace PulsarCRepl
{
    class JintAdditons
    {
        //Prefix to use in javascript, Assembly File Name
        private static readonly Dictionary<string, string> GameDllNames = new Dictionary<string, string>
        {
            {"cs_", "Assembly-CSharp"},
            {"cs1_", "Assembly-CSharp-firstpass"}
        };

        public static Engine AddGameSpecificClasses(Engine myengine, Dictionary<string, string> yourLibraries = null)
        {
            //copy the static readonly dict
            var finalLibs = new Dictionary<string, string>(GameDllNames);
            //if we have libraries copy them to the finallibs dict
            yourLibraries?.ToList().ForEach(PrefixAndLibraryName =>
            {
                var prefix = PrefixAndLibraryName.Key;
                var LibraryName = PrefixAndLibraryName.Value;
                var mykey = PrefixAndLibraryName.Key;
                //Handle duplicates, there might be a better way but i didn't want something too complex
                if (finalLibs.ContainsKey(prefix))
                {
                    mykey = $"z{mykey}";
                }

                finalLibs.Add(mykey, LibraryName);
            });
            myengine = JintTypeGeneration.GenerateJintRefrences(myengine, finalLibs);

            return myengine;
        }
    }
}