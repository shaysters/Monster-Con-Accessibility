# Monster Con Accessibility: developer notes

A BepInEx 5 plugin (32-bit, .NET Framework 4.7.2) for Monster Prom 4: Monster Con. Speech goes through Prism, loaded from `prism.dll` next to the plugin.

## Building

You need the .NET SDK (version 6 or later), and the game installed with BepInEx, because the build uses the game's own files and BepInEx's files from the game folder.

```
dotnet build MonsterConAccessibilityMod.csproj -c Release
```

If the game isn't in the default Steam folder, add `-p:GameDir="<game folder>"`. The built mod is in `bin\Release\net472`.

## Packaging and releasing

Build the release zip into `dist\MonsterConAccessibility.zip`:

```
powershell -ExecutionPolicy Bypass -File scripts\package-release.ps1
```

Add `-GameDir "<game folder>"` if the game isn't in the default Steam folder.

To publish a release:

1. Update the version in `Plugin.cs`, `changes-latest.md` and `CHANGELOG.md`.
2. Build the zip.
3. Create a draft GitHub release with the zip attached:

   ```
   gh release create vX.Y.Z --draft --notes-file changes-latest.md dist\MonsterConAccessibility.zip
   ```

4. Run the **Release** workflow from the Actions tab with the same tag. It builds the installer, writes `loadstone-release.json`, and publishes the release.

The installer's package files are in `loadstone-packages`, and its settings are in `installer\profile.json`. The installer reads the package files straight from this repository, so changes to them reach players a few minutes after they're pushed.
