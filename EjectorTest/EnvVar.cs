using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace EjectorTest
{
    public static class EnvVar
    {
        private static Mutex _mutex = new Mutex();
        private static Dictionary<string, string> _envDict;

        private static void init()
        {
            _mutex.WaitOne();
            if (_envDict != null) return;
            _envDict = new Dictionary<string, string>();
            
            var envRaw = File.ReadAllLines(TestContext.Parameters.Get("ENV_FILE"));
            foreach (var envLineRaw in envRaw)
            {
                var trimmedLine = envLineRaw.Trim();
                if (trimmedLine.IndexOf('#') == 0 || trimmedLine.Length == 0) continue;
                var splitterIndex = trimmedLine.IndexOf('=');
                if (splitterIndex == -1) throw new Exception($"Error parsing line: {envLineRaw}");
                try
                {
                    _envDict.Add(trimmedLine[0..splitterIndex].ToLowerInvariant(), trimmedLine[(splitterIndex + 1)..trimmedLine.Length]);
                }
                catch
                {
                    throw new Exception($"Error parsing line: {envLineRaw}");
                }
            }
            
            _mutex.ReleaseMutex();
        }

        public static string GetEnv(string name)
        {
            if (_envDict == null) init();
            name = name.ToLowerInvariant();
            if (!_envDict.TryGetValue(name, out var value))
            {
                throw new Exception($"Env \"{name}\" not set");
            }

            return value;
        }

        public static string GetInternalEnv(string name)
        {
            if (_envDict == null) init();
            name = name.ToLowerInvariant();
            if (!_envDict.TryGetValue(name, out var value))
                return null;
            return value;
        }

        public static void SetEnv(string name, string value)
        {
            if (_envDict == null) init();
            name = name.ToLowerInvariant();
            _envDict.Add(name, value);
        }
    }
}