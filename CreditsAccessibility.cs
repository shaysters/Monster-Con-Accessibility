using Game;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class CreditsAccessibility
    {
        private static bool _announcedCreditsVideo;

        internal static void Tick()
        {
            try
            {
                CreditsView view = Object.FindObjectOfType<CreditsView>();
                if (view == null)
                {
                    _announcedCreditsVideo = false;
                    return;
                }

                object player = view.GetType().GetField("creditsVideoPlayer").GetValue(view);
                if (!IsVideoPlaying(player))
                {
                    return;
                }

                if (_announcedCreditsVideo)
                {
                    return;
                }

                _announcedCreditsVideo = true;
                Plugin.SpeakOnce("credits:video-start", "Credits screen. Credits are now playing.", true, 2f);
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning("CreditsAccessibility.Tick failed: " + ex);
            }
        }

        private static bool IsVideoPlaying(object player)
        {
            if (player == null)
            {
                return false;
            }

            System.Reflection.PropertyInfo property = player.GetType().GetProperty("isPlaying");
            object value = property == null ? null : property.GetValue(player, null);
            return value is bool && (bool)value;
        }
    }
}
