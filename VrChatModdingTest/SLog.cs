using System;
using MelonLoader;

namespace VrChatModdingTest
{
    public class SLog
    {
        public static void Info(string cannotResolveFile, string file)
        {
            MelonModLogger.Log(cannotResolveFile);
            MelonModLogger.Log(file);
            
        }

        public static void Info(string cannotResolveFile)
        {
            MelonModLogger.Log(cannotResolveFile);
        }

        public static void Error(string exception)
        {
            MelonModLogger.Log(exception);
        }

        public static void Error(Exception exception)
        {
            MelonModLogger.Log(exception.Message);
        }
    }
}