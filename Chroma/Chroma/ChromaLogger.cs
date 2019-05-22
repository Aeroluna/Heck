using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma {

    public static class ChromaLogger {

        public enum Level {
            DEBUG = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3,
            NEVER = 4
        }

        private static string filePath = null;
        public static Level PrintLevel { get; set; } = Level.INFO;
        public static Level LogLevel { get; set; } = Level.WARNING;
        public static Level SoundLevel { get; set; } = Level.NEVER;

        internal static void Init() {
            if (filePath != null) return;

            //TODO customize levels

            filePath = Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma/log.txt";
            if (!File.Exists(filePath)) {
                using (var stream = File.Create(filePath)) { }
            }

            using (StreamWriter w = new StreamWriter(filePath, false)) {
                w.WriteLine("Logger initialized...");
            }
        }

        public static void Log(Exception e, Level level = Level.ERROR, bool sound = true) {
            Log(e.ToString(), level, sound);
        }

        public static void Log(Object obj, Level level = Level.DEBUG, bool sound = true) {
            Log(obj.ToString(), level, sound);
        }

        public static void Log(string[] messages, Level level = Level.DEBUG, bool sound = true) {
            foreach (String s in messages) Log(s, level, sound);
        }

        public static void Log(string message, Level level = Level.DEBUG, bool sound = true) {
            if (level >= PrintLevel || ChromaConfig.DebugMode) Console.WriteLine("[Chroma] " + message);
            if (sound && level >= SoundLevel) AudioUtil.Instance.PlayErrorSound();
            WriteLog(message, level);
        }

        private static void WriteLog(String s, Level level = Level.DEBUG) {
            if (level < LogLevel || ChromaConfig.DebugMode) return;
            using (StreamWriter w = new StreamWriter(filePath, true)) {
                w.WriteLine(s);
            }
        }

    }

}
