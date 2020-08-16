# KCollisionLibrary

## Projects
- KclImporter (A executable program used for importing and exporting collision models)
- KclLibraryGUI (A library for gui handling importing and configuring material attributes)
= KclRender (A 3D view displaying collision models and hit detection of octree boundings)
- KclLibrary (The main collision library for handling the file binaries, hit detection, etc)

## Features
- Supports all versions. GCN, WII, DS, 3DS Wii U, and Switch
- Supports multiple model subdivisions for V2 KCL (WiiU/Switch) allowing for high poly collision models.
- Can easily load and save KCL binaries, change endianness, version, etc.
- Supports mapping material attributes by .obj files (COL_## for material name, ## is hex value).
- KCL importer GUI which can load various game material and collision presets and export .obj files.

## Planned
- Functional collision handling for in tool editors (currently somewhat functional but needs improvements).

## Credits

Library by Syroot & KillXGaming.

Thanks to Syroot/Ray Koopa for the [original library base](https://gitlab.com/Syroot/NintenTools/MarioKart8/-/tree/master/src/Syroot.NintenTools.MarioKart8/Collisions).
