# Monster Con TTS Accessibility Mod

Screen reader accessibility for the Windows Steam version of Monster Prom 4: Monster Con.

Created by Shaysters, Codex, and Claude.

## Links

Monster Prom 4: Monster Con: https://store.steampowered.com/app/2869860/Monster_Prom_4_Monster_Con/

BepInEx 5: https://github.com/BepInEx/BepInEx/releases

## Install with the installer (recommended)

The installer sets up everything for you, including BepInEx. It needs 64-bit Windows 10 or later and supports the Steam version of the game.

1. Download `MonsterConAccessibilityInstaller.exe` from the [latest release](https://github.com/shaysters/Monster-Con-Accessibility/releases/latest).
2. Close the game, then run the installer.
   Windows SmartScreen may warn that the app is unrecognized, because the installer is not code-signed. Choose "More info", then "Run anyway".
3. The installer finds the game through Steam. If it doesn't, use Browse to pick the folder that contains `MonsterCon.exe`.
4. Press Install. The installer adds BepInEx if it is missing, then the mod, and removes files left by older versions of the mod.
5. Start your screen reader, then launch the game.

Run the installer again at any time to update, repair, or uninstall the mod.

## Manual install

### Install BepInEx

This mod uses BepInEx 5. The current Windows Steam build is x86, so use the x86 Windows package.

Recommended file:

`BepInEx_win_x86_5.4.23.5.zip`

To install BepInEx:

1. Open the folder that contains `MonsterCon.exe`, such as `C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 4 - Monster Con`.
2. Extract BepInEx into that folder.
3. Launch the game once, then close it.

### Install This Mod

Copy the contents of `MonsterConAccessibility.zip` from the latest release into:

`Monster Prom 4 - Monster Con\BepInEx\plugins`

Files:

`MonsterConAccessibilityMod.dll`

`ManualPages.en.txt`

`prism.dll`

`Prism` folder (Prism license files)

Then start your screen reader and launch the game.

## Speech

The mod uses Prism for speech. If NVDA, JAWS, or another supported screen reader is running when the game starts, speech goes through it, and to your braille display if the screen reader supports it. If no screen reader is running, the mod uses built-in Windows speech. If you start your screen reader after the game, restart the game to switch over.

## Added Controls

Keyboard:

`R` repeats the last repeatable spoken text.

`T` reads the current player's stats.

`C` reads the online code again while you are in an online lobby. When you host a match, the code is also read automatically, digit by digit.

Controller:

Left stick repeats the last repeatable spoken text.

Right stick reads the current player's stats.

## Notes

The in-game manual text included with this mod was written by AI. It may not be completely accurate.

The schedule screen has limited accessibility. The mod reads a short current/next stop summary.

Pepper indicators on date choices are visual only and are not readable by TTS in this release.

Food court tables say who is sitting there and how many seats are free.

Gallery image descriptions are not included.

The release zip includes Prism (`prism.dll`), a third-party speech library. It is not owned by Shaysters, Codex, or Claude, and is licensed under the Mozilla Public License 2.0. Its license files are in the `Prism` folder.

The installer is built with [Loadstone](https://github.com/bradjrenshaw/loadstone) by Bradley Renshaw.

Speech uses [Prism](https://github.com/ethindp/prism) by Ethin Probst, licensed under the Mozilla Public License 2.0.

## Troubleshooting

- **No speech at all:** make sure `MonsterConAccessibilityMod.dll`, `prism.dll`, and the `Prism` folder are in `BepInEx\plugins`, and that BepInEx is installed. Running the installer again (Repair) fixes most problems.
- **Speech uses the Windows voice instead of your screen reader:** start your screen reader before the game, then restart the game.
- **Still stuck:** the BepInEx log is at `<Game Folder>\BepInEx\LogOutput.log`. It shows whether the mod loaded and which speech backend Prism chose.

## Building from source

For developers. You need the .NET SDK (version 6 or later) and the game installed with BepInEx, because the build uses the game's own files and BepInEx's files from the game folder.

Build:

```
dotnet build MonsterConAccessibilityMod.csproj -c Release
```

If the game is not in the default Steam folder, add `-p:GameDir="<game folder>"`. The built mod is in `bin\Release\net472`.

Build the release zip into `dist\MonsterConAccessibility.zip`:

```
powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
```

Add `-GameDir "<game folder>"` if the game is not in the default Steam folder.

To publish a release: update `changes-latest.md`, build the zip, create a draft GitHub release with the zip attached (`gh release create vX.Y.Z --draft --notes-file changes-latest.md dist\MonsterConAccessibility.zip`), then run the **Release** workflow from the Actions tab with the same tag. It builds the installer, writes `loadstone-release.json`, and publishes the release. The installer's package files are in `loadstone-packages/`, and its settings are in `installer/profile.json`.
