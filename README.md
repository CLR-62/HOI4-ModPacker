# ModPacker by CLR62 | 2026

**ModPacker** is a utility designed to pack mods for pirated copies of *Hearts of Iron IV* that do not have access to the Steam Workshop.

![ModPacker Screenshot](https://github.com/CLR-62/HOI4-ModPacker/blob/master/ModPacker/src/Assets/screenshot.png "ModPacker screenshot")

Program support both english and russian languages.

## How to pack?
1. In the "Mod folder path" section, select path to your mod folder(the folder that contains folders such as common, history, gfx, etc.)
2. Program will automatically suggest destination zip file name and .mod file name, but you can change it as you want
3. And if you preffer to change name of your mod(for example, i would like to add "Pirate" postfix for this version of the mod), you can select "Change to..." option in "Mod name changing" and write new name of mod below, or just leave selected option "Leave it as it is"
4. Then press "Pack", depending on your PC specs and mod sizes, it may take a few minutes to copy and pack mod into archive. Folder with the packed zip archive will open automatically, but if it is not, then the archive can be found in "Dist" folder in the program directory.

## How It Works
The program performs the following actions:

1. Takes the user-selected mod folder.
2. Creates a `.mod` file for it.
   > ⚠️ **Important:** The generated file has the "Read Only" attribute and uses a **relative path** to the mod folder (`mod\MOD_FOLDER`). This prevents the HOI4 launcher from automatically correcting or modifying the path.
3. Packages both the mod folder and the new `.mod` file into a ZIP archive.

## Installing the Mod

To get the mod working, the player needs to:

1. Extract the contents of the archive to the following folder: `Documents/Paradox Interactive/Hearts of Iron IV/mod/`
2. **Attention!** When extracting, make sure to select **"Extract here"** (or "Extract to current folder"). This ensures that both the mod folder and the `.mod` file are placed directly inside the `mod` directory, rather than in a subfolder.
