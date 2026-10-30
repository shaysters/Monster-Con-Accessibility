using System.Runtime.InteropServices;
using UnityEngine;

namespace MonsterConAccessibilityMod
{
    internal static class HotkeyAccessibility
    {
        private const int VK_R = 0x52;
        private const int VK_T = 0x54;
        private const int VK_C = 0x43;
        private const ushort XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
        private const ushort XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;

        private static bool _rawRepeatDown;
        private static bool _rawStatsDown;
        private static bool _rawOnlineCodeDown;

        internal static void Tick(string source)
        {
            try
            {
                if (!Plugin.IsGameWindowFocused())
                {
                    _rawRepeatDown = false;
                    _rawStatsDown = false;
                    _rawOnlineCodeDown = false;
                    return;
                }

                if (IsAnyKeyDown(
                    KeyCode.R,
                    KeyCode.JoystickButton8,
                    KeyCode.Joystick1Button8,
                    KeyCode.Joystick2Button8,
                    KeyCode.Joystick3Button8,
                    KeyCode.Joystick4Button8) ||
                    IsRawPressedOnce(ref _rawRepeatDown, VK_R, XINPUT_GAMEPAD_LEFT_THUMB))
                {
                    Plugin.RepeatLastSpoken();
                }

                if (IsAnyKeyDown(
                    KeyCode.T,
                    KeyCode.JoystickButton9,
                    KeyCode.Joystick1Button9,
                    KeyCode.Joystick2Button9,
                    KeyCode.Joystick3Button9,
                    KeyCode.Joystick4Button9) ||
                    IsRawPressedOnce(ref _rawStatsDown, VK_T, XINPUT_GAMEPAD_RIGHT_THUMB))
                {
                    Plugin.SpeakCurrentStats();
                }

                if (IsAnyKeyDown(KeyCode.C) ||
                    IsRawPressedOnce(ref _rawOnlineCodeDown, VK_C, 0))
                {
                    OnlineCodeAccessibility.SpeakCurrentCode();
                }

                OnlineCodeAccessibility.Tick();
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning(source + " HotkeyAccessibility.Tick failed: " + ex);
            }
        }

        private static bool IsAnyKeyDown(params KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                KeyCode key = keys[i];
                if (Input.GetKeyDown(key) || UICamera.GetKeyDown(key))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRawPressedOnce(ref bool wasDown, int virtualKey, ushort xinputButton)
        {
            bool down = IsVirtualKeyDown(virtualKey) || IsXInputButtonDown(xinputButton);
            bool pressed = down && !wasDown;
            wasDown = down;
            return pressed;
        }

        private static bool IsVirtualKeyDown(int virtualKey)
        {
            try
            {
                return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsXInputButtonDown(ushort button)
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    XINPUT_STATE state;
                    if (XInputGetState(i, out state) != 0)
                    {
                        continue;
                    }

                    if ((state.Gamepad.wButtons & button) != 0)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern uint XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }
    }
}
