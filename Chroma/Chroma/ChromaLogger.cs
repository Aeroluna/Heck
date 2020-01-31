using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace Chroma {

    public static class ChromaLogger {

        public enum Level {
            DEBUG = 1,
            INFO = 2,
            WARNING = 4,
            ERROR = 8,
            NEVER = 255
        }

        public static IPALogger logger;

        public static Level SoundLevel { get; set; } = Level.NEVER;

        public static void Log(Exception e, Level level = Level.ERROR, bool sound = true,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0) {
            Log(e.ToString(), level, sound, member, line);
        }

        public static void Log(Object obj, Level level = Level.DEBUG, bool sound = true,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0) {
            Log(obj.ToString(), level, sound, member, line);
        }

        public static void Log(string[] messages, Level level = Level.DEBUG, bool sound = true,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0) {
            foreach (String s in messages) Log(s, level, sound, member, line);
        }

        public static void Log(string message, Level level = Level.DEBUG, bool sound = true,
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0) {
            if (ChromaConfig.DebugMode) {
                logger.Log((IPALogger.Level)level, $"{member}({line}): {message}");
            } else {
                logger.Log((IPALogger.Level)level, $"{message}");
            }
            if (sound && level >= SoundLevel) AudioUtil.Instance.PlayErrorSound();
        }

    }

}
