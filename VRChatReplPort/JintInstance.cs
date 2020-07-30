using System;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Runtime;
using MelonLoader;
using Console = System.Console;

namespace PulsarCRepl
{
    public class JintInstance
    {
        private Engine myengine = null;
        private string engineOut;

        public JintInstance()
        {
            myengine = new Engine(cfg => { cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies()); });
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
        }

        public string GetOutput()
        {
            return engineOut;
        }

        public void ExecuteCode(string code)
        {
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