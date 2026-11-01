# Monster Con Accessibility

Monster Con Accessibility is a mod that makes Monster Prom 4: Monster Con, the multiplayer dating sim by Beautiful Glitch, playable with a screen reader. It reads menus, dialogue, choices, stats, the schedule, the in-game manual and food court tables, and reads the online code when you host a match.

It works with NVDA, JAWS and other Windows screen readers, and falls back to Windows' built-in voices if no screen reader is running. If your screen reader has a braille display connected, everything the mod reads goes to braille too.

- Join the Discord to chat, get help or report problems: <https://discord.gg/KNDXhztGGK>
- Support shaysters on Ko-fi: <https://ko-fi.com/shaysters>

Version 1.1.4. See [CHANGELOG.md](CHANGELOG.md) for what has changed.

## Requirements

- Monster Prom 4: Monster Con for Windows, from Steam or GOG:
  - Steam: <https://store.steampowered.com/app/2869860/Monster_Prom_4_Monster_Con/>
  - GOG: <https://www.gog.com/en/game/monster_prom_4_monster_con>
- A screen reader such as NVDA or JAWS (optional; without one, the mod uses Windows' built-in speech).

The mod has been tested with the Steam version. The GOG and DRM-free versions should work too, but they haven't been tested yet.

## Installing

1. Close the game if it's running.
2. Download **MonsterConAccessibilityInstaller.exe** from the [latest release](https://github.com/shaysters/Monster-Con-Accessibility/releases/latest). It needs 64-bit Windows 10 or later.
3. Run it. Windows SmartScreen may warn that the app is unrecognized, because the installer is not code-signed. Choose **More info**, then **Run anyway**.
4. The installer finds your Steam copy of the game by itself. For a GOG or DRM-free copy, or if it can't find the game, use the Browse button next to the game folder and choose the folder that contains `MonsterCon.exe`.
5. Choose **Install**. The installer adds BepInEx if it's missing, then the mod, and removes files left by older versions of the mod.
6. Start your screen reader, then start the game.

### Updating and uninstalling

Keep the installer. Run it again to update to the newest version, repair the mod, or uninstall it. Close the game first. The installer says on its first page when an update is available, and the mod tells you too, once the title menu has been read.

### Installing by hand

1. Download BepInEx 5 for 32-bit Windows, `BepInEx_win_x86_5.4.23.5.zip`, from the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases). The game is 32-bit, so the x64 package won't work.
2. Extract it into the game folder, the one that contains `MonsterCon.exe`. On a default Steam install that's:

   ```
   C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 4 - Monster Con
   ```

   For GOG or DRM-free copies, use the folder you installed the game to.
3. Start the game once so BepInEx can set itself up, then close it.
4. Download **MonsterConAccessibility.zip** from the [latest release](https://github.com/shaysters/Monster-Con-Accessibility/releases/latest) and extract everything in it into `BepInEx\plugins` in the game folder. That folder should then contain `MonsterConAccessibilityMod.dll`, `ManualPages.en.txt`, `prism.dll` and the `Prism` folder (Prism's license files).
5. Start your screen reader, then start the game.

To uninstall a manual install, delete the files and folder listed in step 4 from `BepInEx\plugins`. To remove BepInEx too, delete the `BepInEx` folder, `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and `changelog.txt` from the game folder.

## Keys

| Keyboard | Controller | What it does |
|---|---|---|
| r | Left stick click | Repeat the last thing the mod said. |
| t | Right stick click | Read the current player's stats. |
| c | None | In an online lobby, read the online code again. |

Notes:

- When you host an online match, the online code is read automatically, digit by digit.
- Food court tables say who is sitting there and how many seats are free.
- The schedule screen reads a short summary of the current and next stop.

## Known limitations

- The schedule screen only has the short summary above.
- Pepper markers on date choices are only shown as pictures, so the mod can't read them yet.
- Gallery pictures don't have descriptions.
- The in-game manual text included with the mod was written by AI and may not be completely accurate.

## Troubleshooting

BepInEx keeps a log in the game folder:

```
BepInEx\LogOutput.log
```

It shows whether the mod loaded and which speech output Prism chose. If something goes wrong, include the log when you report it.

- **No speech at all:** make sure `MonsterConAccessibilityMod.dll`, `prism.dll` and the `Prism` folder are in `BepInEx\plugins`, and that BepInEx is installed. Running the installer again and choosing Repair fixes most problems.
- **Windows speech talks instead of your screen reader:** start your screen reader before the game. If you start it after, restart the game.

To report a problem or suggest something, open an issue on the [GitHub page](https://github.com/shaysters/Monster-Con-Accessibility/issues).

## Credits

This mod was developed by [Shaysters](https://github.com/shaysters), with code written by Codex (OpenAI) and Claude (Anthropic).

The installer is built with [Loadstone](https://github.com/bradjrenshaw/loadstone) by [Bradley Renshaw](https://github.com/bradjrenshaw).

Speech is provided by [Prism](https://github.com/ethindp/prism) by Ethin Probst, under the MPL-2.0 license. It isn't owned by Shaysters, Codex or Claude. Its license files are in the `Prism` folder.

Monster Prom 4: Monster Con is made by Beautiful Glitch. This mod isn't affiliated with or endorsed by Beautiful Glitch.

## For developers

Build and release instructions are in [DEVELOPING.md](DEVELOPING.md).
