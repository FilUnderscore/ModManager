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

Download and extract the latest release from the [Releases](https://github.com/FilUnderscore/ModManager/releases) page, then just drag and drop the 000-ModManager folder into the `Mods` folder in your 7 Days to Die install folder. Settings can be modified in the Mod Manager Settings Tab in the Mods menu.

### Client-only
Disable EAC so the game can load the Improved Hordes DLL.
