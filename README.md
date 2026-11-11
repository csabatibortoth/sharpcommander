# Sharp Commander

A two-panel file manager for Windows, written in C# with Windows Forms, in the spirit of Total Commander.

It started as a school project. It now runs on .NET 10, works with both local folders and FTP servers, and has a Hungarian and an English user interface.

## Features

- **Two-panel browsing.** Each panel shows a local folder or an FTP server. Folders load in the background.
- **File operations.** Copy, move, rename, delete, new folder and search, between any two panels (local ↔ local, local ↔ FTP).
  - Moves within one drive are an instant rename instead of a copy and delete.
  - A progress window shows the current file and overall progress, and has a Cancel button.
  - When a target already exists you can answer Yes, No or All.
  - Copying a folder into itself is refused. A failed or cancelled copy removes its half-written target.
- **Search.** Finds names below the active panel's folder. `*` and `?` work as wildcards; plain text matches anywhere in the name.
- **Archives.** Compress files and folders into a `.zip`. Extract `.zip` and `.gz` files. Archives that try to write outside the target folder are refused.
- **FTP.** Uses [FluentFTP](https://github.com/robinrodricks/FluentFTP), with plain FTP, Explicit TLS or automatic encryption. Saved connections are kept without passwords.
- **Built-in text editor.** New, open, save, print, undo/redo and the clipboard commands. It keeps the file's encoding and line endings.
- **Windows clipboard.** Cut, Copy and Paste use the Windows clipboard, so they work together with Explorer.
- **Languages.** Hungarian and English. The UI follows the Windows language by default and can be switched live in the *Nyelv / Language* menu.

## Requirements

- Windows
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to build, or the .NET 10 Desktop Runtime to run

## Building and running

From the command line:

```
cd src
dotnet build sharpcommander.sln -c Release
dotnet run --project sharpcommander
```

Or open `src/sharpcommander.sln` in Visual Studio 2022 or newer and press F5.

The build output is in `src/sharpcommander/bin/Release/net10.0-windows/`.

## Settings

Preferences are stored in `%AppData%\Sharp Commander\settings.json`:

- the selected UI language (`null` means "follow Windows")
- saved FTP connections, most recently used first. **Passwords are never stored.**

Delete the file to reset everything to defaults.

## Project layout

```
src/sharpcommander/
├── Program.cs               entry point
├── mainForm.cs              main window: menus, toolbars, the two panels
├── FileHandlerForm.cs       one file panel
├── FileSystems/             IFileSystem with local and FTP implementations
├── Operations/              copy / move / delete / compress / extract logic
├── Localization/            Strings.resx (Hungarian), Strings.en.resx (English), Localizer
├── BuiltInTextEditor.cs     text editor
├── FtpConnectDialog.cs      FTP connection dialog
├── ProcessForm.cs           progress window
├── AppSettings.cs           settings.json handling
└── Resources/, icons/       images and icons
```

## Adding a language

1. Copy `Localization/Strings.en.resx` to `Localization/Strings.<culture>.resx` (for example `Strings.de.resx`) and translate the values.
2. Add the culture to `Localizer.SupportedLanguages` in `Localization/Localizer.cs`.

Control texts are matched by name (`FormClass.controlName`, with `Common.*` as a shared fallback), so the designer files don't need to change.

## Known limitations

- If one file fails, the whole operation stops with a message; there is no "skip and continue".
- Opening a file on an FTP server edits a temporary local copy; changes are not uploaded back.
- With *Automatic* FTP encryption, the app falls back to plain FTP if the server offers no TLS, which sends the password unencrypted. Choose *Explicit TLS* to prevent this.
- The update check is disabled.

## Authors

- **Csaba Tóth** - [csabatibortoth](https://github.com/csabatibortoth)

Project page: <https://kernelpanic.xyz/index.php/projects/sharp-commander/>

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
