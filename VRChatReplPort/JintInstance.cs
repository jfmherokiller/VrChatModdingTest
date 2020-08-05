using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Jint.Runtime.References;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using Console = System.Console;
using Int32 = Il2CppSystem.Int32;
using Object = System.Object;
using Types = Jint.Runtime.Types;

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
                    //cfg.AddObjectConverter<Il2IntMyConverter>();
                    //cfg.AddObjectConverter<Il2floatMyConverter>();
                    cfg.SetWrapObjectHandler(WrapObjectHandler);
                });
        }

        private ObjectInstance WrapObjectHandler(Engine engine, object target)
        {
            ObjectWrapper instance;
            //if (target.ToString() == "Il2CppSystem.Type")
            //{
                //return new ObjectWrapper(engine, new WrapperClass((Il2CppSystem.Object)target));
            //    throw new Exception($"Error you are trying to access an il2cpp type which does not have support yet");
            //}
            var thetype = target.GetType();
            if(target is Vector3 myvec)
            {
                //instance = new ObjectWrapper(engine, target);
                instance = new ObjectWrapper(engine, new SVector3(myvec.x,myvec.y,myvec.z));
                //throw new Exception($"Error you are trying to access an il2cpp type which does not have support yet");
            }
            else if(target is Transform mytrans)
            {
                instance = new ObjectWrapper(engine, new STransform(mytrans));
            }
            else if (target is Quaternion myquad)
            {
                instance = new ObjectWrapper(engine,new SQuaderton(myquad));
            }
            else if(target is GameObject myGameObject)
            {
                instance = new ObjectWrapper(engine,new SGameObject(myGameObject));
            }
            else
            {
                instance = new ObjectWrapper(engine, target);
            }
            
            if (instance.IsArrayLike)
            {
                instance.SetPrototypeOf(engine.Array.PrototypeObject);
            }

            return instance;
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

    public class Il2IntMyConverter : IObjectConverter
    {
        public bool TryConvert(Engine engine, object value, out JsValue result)
        {
            var myval = (Il2CppSystem.Object) value;
            try
            {
                var myint = myval.Unbox<int>();
                result = new JsNumber(myint);
                return true;
            }
            catch (Exception e)
            {
                result = null;
                return false;
            }
        }
    }
    public class Il2floatMyConverter : IObjectConverter
    {
        public bool TryConvert(Engine engine, object value, out JsValue result)
        {
            var myval = (Il2CppSystem.Object) value;
            try
            {
                var myint = myval.Unbox<float>();
                result = new JsNumber(myint);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}