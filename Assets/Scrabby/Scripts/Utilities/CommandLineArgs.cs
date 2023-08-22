using System.Collections.Generic;
using UnityEngine;

namespace Scrabby.Utilities
{
    public static class CommandLineArgs
    {
        private static Dictionary<string, string> _args;

        private static void Load()
        {
            _args = new Dictionary<string, string>();
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    _args.Add(args[i][2..], args[i + 1]);
                }
            }
        }
        
        public static string Get(string key)
        {
            if (_args == null)
            {
                Load();
            }

            if (Application.isEditor)
            {
                return key switch
                {
                    "auth" => "c876edb7-3f57-467e-956d-4911a9eb0403",
                    "challenge" => "2934b74f-5f2d-4641-a708-dd33ac43fc18",
                    "server_token" => "crabrave",
                    _ => ""
                };
            }

            return _args != null && _args.TryGetValue(key, out var arg) ? arg : "";
        }
    }
}