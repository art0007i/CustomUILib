# CustomUILib

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows modders to easily add custom inspector ui to components which don't have any by default.<br>
To use simply call the `CustomUILib.AddCustomInspector` function (I recommend using the generic version).<br>
For an example on how to use this just check example.cs
If a component already implements `ICustomInspector` don't use this mod with it, instead just patch that components BuildInspectorUI function.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [CustomUILib.dll](https://github.com/art0007i/CustomUILib/releases/latest/download/CustomUILib.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.