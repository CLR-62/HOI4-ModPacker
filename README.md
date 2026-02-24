# ModPacker by CLR62 | 2026

**ModPacker** is a utility designed to pack mods for pirated copies of *Hearts of Iron IV* that do not have access to the Steam Workshop.

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
