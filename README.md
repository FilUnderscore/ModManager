![Mod Manager Banner](https://i.imgur.com/bLquUKP.png)
[![latest version](https://img.shields.io/github/v/release/FilUnderscore/ModManager?include_prereleases)](https://github.com/FilUnderscore/ModManager/releases)
# Mod Manager
Manage your mods smarter by having an in-game tool to help manage all your Mod Settings and Updates through one interface, the Mods menu.

## Features
* **Mods List/Settings UI accessible from the main menu.** Enable/Disable and customize mod settings from mods that support the Mod Settings API.
* **Game mod list compatibility checking.** Ensures you don't load a save with the wrong set of mods.
* **Better error handling.** Shows a user-friendly dialog box with the option to copy/ignore the message instead of spamming the in-game console with no way to close it.
* **Mod version checking.** Get notified if any new mod updates are available, as well as get compatibility updates about old/new mod versions with the current version of the game installed.
* **Custom Mod Loader.** Load mods from multiple directories (as well as the new default %AppData%\7DaysToDie\Mods and the old 7DaysToDie\Mods program folder).

## Installation
**Note:** Do not download the whole repository and attempt to install the 000-ModManager folder from the dev branch, as it will not work due to no compiled DLL being present. Follow the installation instructions below.

Download and extract the latest release from the [Releases](https://github.com/FilUnderscore/ModManager/releases) page, then just drag and drop the 000-ModManager folder into the `Mods` folder in your 7 Days to Die install folder. 

Settings can be modified in the Mod Manager Settings Tab in the Mods menu.

### Client-only
Disable EAC so the game can load the Improved Hordes DLL.

## Mod Support
[SMXmenu support](https://github.com/FilUnderscore/ModManager/releases/download/1.0.2/ModManagerSMXmenuSupport.zip)

## Donations
You are already showing a lot of support just by enjoying the mod, but if you really appreciate my work and want to show extra support in the form of a donation, feel free to buy me a coffee on Ko-fi.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/R6R3N9KGV) 

## Mod Developers (API)
Mod Manager includes its own APIs to integrate with mods (C# currently, XPath/XML support coming soon).

**Currently including:**
* Mod Manifest API that allows your users to check for new mod versions within the in-game Mods menu
* Mod Settings API  to allow users to customize settings without needing to restart their game (and the UI is handled all by the Mod Manager).

The API is written as a wrapper that can be included in your project as a source file, which allows for optional Mod Manager support - meaning no errors will be thrown if the user chooses not to use the Mod Manager with your mod).

[More information about the API can be found here](https://github.com/FilUnderscore/ModManager/wiki/Mod-Integration)

## Screenshots
![Image showing Mod Menu showing Improved Hordes and Mod Manager loaded. Currently showing information about Mod Manager.](https://i.imgur.com/Xo4LabI.png)
![Image showing Mod Settings for Mod Manager shown in the Mod Menu.](https://i.imgur.com/KRLa8sf.png)
![Image showing Mod Settings for Improved Hordes shown in the Mod Menu.](https://i.imgur.com/gne9qWf.png)
![Image showing Sample error message showing the better error handling feature with Mod Manager.](https://i.imgur.com/RyOQCFj.png)
![Image showing Mods button on the 7 Days To Die Main Menu.](https://i.imgur.com/ldefUSU.png)

## [Terms of Use](https://github.com/FilUnderscore/ModManager/blob/dev/LICENSE)
