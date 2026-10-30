using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MonsterConAccessibilityMod
{
    internal static class ManualPageDescriptions
    {
        private static Dictionary<string, string> _descriptions;

        internal static string Get(string key)
        {
            try
            {
                EnsureLoaded();
                string value;
                if (_descriptions.TryGetValue(key, out value))
                {
                    return value;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("ManualPageDescriptions.Get failed: " + ex);
            }
            return key == "cover" ? "Info manual cover." : "Info manual page " + key + ".";
        }

        private static void EnsureLoaded()
        {
            if (_descriptions != null)
            {
                return;
            }

            _descriptions = new Dictionary<string, string>();
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string path = Path.Combine(baseDir, "ManualPages.en.txt");
            if (!File.Exists(path))
            {
                return;
            }

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();
                if (key.Length > 0 && value.Length > 0)
                {
                    _descriptions[key] = value;
                }
            }
        }
    }
}
