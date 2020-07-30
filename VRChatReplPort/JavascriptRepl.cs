using System;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Runtime;
using MelonLoader;
using Console = System.Console;

namespace PulsarCRepl
{
    class JintBits
    {
        public static void RunMe()
        {
            var engine = new Engine(cfg => { cfg.AllowClr(); });

            engine
                .SetValue("print", new Action<object>(Value => { MelonModLogger.Log(Value.ToString()); }))
                .SetValue("load", new Func<string, object>(
                    path => engine.Execute(File.ReadAllText(path))
                        .GetCompletionValue()));
            engine = JintAdditons.AddGameSpecificClasses(engine);

            MelonModLogger.Log("Type 'exit' to leave, " +
                               "'print()' to write on the console, " +
                               "'load()' to load scripts.");
            MelonModLogger.Log("");
            
            while (true)
            {
                MelonModLogger.Log("jint> ");
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    return;
                }

                try
                {
                    var result = engine.GetValue(engine.Execute(input).GetCompletionValue());
                    if (result.Type != Types.None && result.Type != Types.Null && result.Type != Types.Undefined)
                    {
                        var str = TypeConverter.ToString(engine.Json.Stringify(engine.Json,
                            Arguments.From(result, Undefined.Instance, "  ")));
                        MelonModLogger.Log("=> {0}", str);
                    }
                }
                catch (JavaScriptException je)
                {
                    MelonModLogger.Log(je.ToString());
                }
                catch (Exception e)
                {
                    MelonModLogger.Log(e.Message);
                }
            }
        }
    }
}