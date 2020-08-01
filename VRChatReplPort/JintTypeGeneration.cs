using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jint;
using Jint.Runtime.Interop;

namespace PulsarCRepl
{
    public class JintTypeGeneration
    {
        public static Engine GenerateJintRefrences(Engine myengine, Dictionary<string, string> finalLibs)
        {
            var TypeNames = new Dictionary<string, Type>();
            //generate Jint References
            foreach (var PrefixAndLibraryName in finalLibs)
            {
                //filtered to types which arent special (like getset) and are classes and only include top classes
                var gameAssemblyClasses = Assembly.Load(PrefixAndLibraryName.Value).GetTypes()
                    .Where(item => item.IsClass && !item.IsSpecialName && !item.IsNested);
                //generate typename dict
                foreach (var gameAssemblyClass in gameAssemblyClasses)
                {
                    var typename = PrefixAndLibraryName.Key + gameAssemblyClass.Name;
                    //Handle duplicates, there might be a better way but i didn't want something too complex
                    if (TypeNames.ContainsKey(typename))
                    {
                        var mykey = PrefixAndLibraryName.Key;
                        mykey = $"z{mykey}";
                        typename = mykey + gameAssemblyClass.Name;
                    }

                    try
                    {
                        TypeNames.Add(typename, gameAssemblyClass);
                    }
                    catch (Exception)
                    {
                        var mykey = typename;
                        mykey = $"z{mykey}";
                        typename = mykey;
                        TypeNames.Add(typename, gameAssemblyClass);
                    }
                }

                //add types to jint instance
                foreach (var name in TypeNames)
                {
                    myengine = myengine.SetValue(name.Key, TypeReference.CreateTypeReference(myengine, name.Value));
                }
            }

            return GenerateClassViewFunction(myengine, TypeNames);
        }

        public static Engine GenerateClassViewFunction(Engine myengine, Dictionary<string, Type> typeNames)
        {
            var ClassesCombined = "\"" + string.Join(",", typeNames.Keys).Replace(",", "\",\"") + "\"";
            var ClassGetterFunct = $"function getClasses() {{return [{ClassesCombined}]}}";
            return myengine.Execute(ClassGetterFunct);
        }

        public static void GenerateTypeObject(string name, Type classval)
        {
            
        }
    }
}