# Monster Con TTS Accessibility Mod

Screen reader and accessibility helpers for the Windows Steam version of Monster Prom 4: Monster Con.

Created by Shaysters and Codex.

## Get the Game

Buy Monster Prom 4: Monster Con on Steam:

https://store.steampowered.com/app/2869860/Monster_Prom_4_Monster_Con/

## Install BepInEx

This mod uses BepInEx 5. The current Windows Steam build is x86, so use the x86 Windows package.

Download BepInEx here:

https://github.com/BepInEx/BepInEx/releases

Recommended file:

`BepInEx_win_x86_5.4.23.5.zip`

Do not use BepInEx 6 for this release.

Common game install paths:

`C:\Program Files (x86)\Steam\steamapps\common\Monster Prom 4 - Monster Con`

`C:\Program Files\Steam\steamapps\common\Monster Prom 4 - Monster Con`

If Steam is installed somewhere else, open your Steam library folder and then:

`steamapps\common\Monster Prom 4 - Monster Con`

To install BepInEx:

1. Open the folder that contains `MonsterCon.exe`.
2. Extract BepInEx into that folder.
3. Launch the game once, then close it.

## Install This Mod

Copy these files into:

`Monster Prom 4 - Monster Con\BepInEx\plugins`

Files:

`MonsterConAccessibilityMod.dll`

`ManualPages.en.txt`

`Tolk.dll`

`nvdaControllerClient32.dll`

Then start your screen reader and launch the game.

## Added Controls

Keyboard:

`R` repeats the last repeatable spoken text.

`T` reads the current player's stats.

Controller:

Left stick repeats the last repeatable spoken text.

Right stick reads the current player's stats.

Repeat is meant for dialogue/manual text, not menu or choice focus.

## Notes

The in-game manual text included with this mod was written by AI. It may not be completely accurate.

The schedule screen has limited accessibility. The mod reads a short current/next stop summary.

Pepper indicators on date choices are visual only and are not readable by TTS in this release.

Gallery image descriptions are not included.
