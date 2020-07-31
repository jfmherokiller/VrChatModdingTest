using System;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Jint.Runtime.References;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using Console = System.Console;

namespace PulsarCRepl
{
    public class JintInstance
    {
        private Engine myengine = null;
        private string engineOut;
        
        public JintInstance()
        {
            myengine = new Engine(
                cfg =>
                {
                    cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies());
                    cfg.CatchClrExceptions(exception =>
                    { 
                        MelonModLogger.Log(exception.Message);
                        return true;
                    });
                    cfg.SetWrapObjectHandler((engine, target) =>
                    {
                        if (target.GetType() == typeof(Il2CppSystem.Type))
                        {
                            return null;
                        }
                        var instance = new ObjectWrapper(engine,target);
                        if (instance.IsArrayLike)
                        {
                            instance.SetPrototypeOf(engine.Array.PrototypeObject);
                        }
                        return instance;
                    });
                });
        }

        public void SetupEnginePieces()
        {
            //setup print function
            myengine = myengine
                .SetValue("print", new Action<object>(Value => { engineOut += ("\n" + Value + "\n"); }));
            //add a load function
            myengine = myengine.SetValue("load", new Func<string, object>(
                path => myengine.Execute(File.ReadAllText(path))
                    .GetCompletionValue()));
            //setup Assembly-Csharp,Assembly-csharp-firstpass
            myengine = JintAdditons.AddGameSpecificClasses(myengine);
            myengine = myengine.Execute(
                "function testing() {return cs_VRCPlayer.field_Internal_Static_VRCPlayer_0.transform}");
        }

        public string GetOutput()
        {
            return engineOut;
        }

        public void ExecuteCode(string code)
        {
            IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
            try
            {
                var result = myengine.GetValue(myengine.Execute(code).GetCompletionValue());
                //filter out statements which dont return data
                if (result.Type == Types.None || result.Type == Types.Null || result.Type == Types.Undefined) return;
                
                var str = TypeConverter.ToString(myengine.Json.Stringify(myengine.Json,
                    Arguments.From(result, Undefined.Instance, "  ")));
                engineOut = $"=> {str}";
            }
            catch (JavaScriptException je)
            {
                engineOut = je.ToString();
            }
            catch (Exception e)
            {
                engineOut = e.Message;
            }
        }
    }
}