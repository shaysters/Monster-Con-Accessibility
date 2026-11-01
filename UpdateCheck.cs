using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterConAccessibilityMod
{
    // Once per game start, asks GitHub for the latest release and says so if it's newer than the
    // installed mod. Reads the release manifest the installer itself uses
    // (releases/latest/download/loadstone-release.json: a plain redirect, no API token or rate limit).
    // Nothing is sent except the request itself, and a failed check is only logged.
    //
    // Polled from the SceneController.Update patch, like the hotkeys: components on the BepInEx
    // manager object don't get updates in this game.
    internal static class UpdateCheck
    {
        private const string ManifestUrl = "https://github.com/shaysters/Monster-Con-Accessibility/releases/latest/download/loadstone-release.json";
        private static readonly Regex VersionField = new Regex("\"version\"\\s*:\\s*\"v?([0-9.]+)\"");


        private static UnityWebRequest _request;
        private static string _newerVersion;
        private static bool _started;
        private static bool _done;

        internal static void Tick()
        {
            if (_done || Plugin.Speech == null)
            {
                return;
            }

            if (_newerVersion != null)
            {
                if (!UpdateNotice.Ready(Plugin.Speech.SpeakCount, Time.realtimeSinceStartup - Plugin.Speech.LastSpeechTime))
                {
                    return;
                }

                _done = true;
                // Plugin.Speak remembers it, so R repeats it.
                Plugin.Speak("Monster Con Accessibility version " + _newerVersion + " is available. Run the installer to update.", false);
                return;
            }

            if (!_started)
            {
                _started = true;
                _request = UnityWebRequest.Get(ManifestUrl);
                _request.timeout = 15;
                _request.SendWebRequest();
                return;
            }

            if (_request == null || !_request.isDone)
            {
                return;
            }

            try
            {
                if (_request.result != UnityWebRequest.Result.Success)
                {
                    Plugin.Log?.LogInfo("Update check skipped: " + _request.error);
                    _done = true;
                    return;
                }

                Match m = VersionField.Match(_request.downloadHandler.text);
                Version latest = m.Success ? Parse(m.Groups[1].Value) : null;
                Version installed = Parse(Plugin.Version);
                Plugin.Log?.LogInfo("Update check: installed " + installed + ", latest " + latest);
                if (latest != null && installed != null && latest > installed)
                {
                    _newerVersion = m.Groups[1].Value;
                }
                else
                {
                    _done = true;
                }
            }
            finally
            {
                _request.Dispose();
                _request = null;
            }
        }

        private static Version Parse(string s)
        {
            try
            {
                return new Version(s);
            }
            catch
            {
                return null;
            }
        }
    }

    // When the update notice may be spoken: after the title menu's first line has been read
    // and the mod has then been quiet for a moment, so it neither cuts off the start-up speech
    // nor gets cut off by it. If the menu never speaks by itself, a longer silence will do.
    internal static class UpdateNotice
    {
        // Lines the mod speaks by itself before the title menu: nothing.
        private const int StartupLines = 0;
        private const float QuietSeconds = 2f;
        private const float FallbackQuietSeconds = 30f;

        internal static bool Ready(int spokenLines, float secondsQuiet)
        {
            if (secondsQuiet < QuietSeconds)
            {
                return false;
            }

            return spokenLines > StartupLines
                || (secondsQuiet >= FallbackQuietSeconds && Time.realtimeSinceStartup >= 30f);
        }
    }
}
