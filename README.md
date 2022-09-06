## Prerequisites

* CMake v3.15 or later
* [.NET SDK v7.0.100-preview.7 or later](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* If you're using Visual Studio, you need Visual Studio 2022 v17.2 or later. The original release of Visual Studio 2022 is too old, so make sure yours has been updated.
* Unity 2021.3.2f1 (newer versions are likely to work)

The build Cesium for Unity assembly will run on much older versions of .NET, including the version of Mono included in Unity. However, these very recent versions are required for the C#<->C++ interop code generator (Reinterop).

To make sure things are set up correctly, open a command-prompt (PowerShell is a good choice on Windows) and run:

* `dotnet --version` and verify that it reports 7.0 or later
* `cmake --version` and verify that it reports 3.15 or later

## Setting up the development environment

Clone the `cesium-unity-samples` (game) project and `cesium-unity` (plugin) project anywhere you like:

```
git clone --recurse-submodules https://github.com/CesiumGS/cesium-unity-samples.git
git clone --recurse-submodules https://github.com/CesiumGS/cesium-unity.git
```

Be sure to also clone the submodules. If you forgot the `--recurse-submodules` option when you cloned, run `git submodule update --init --recursive`.

Create a directory junction (symbol link) from the game project's Assets directory into the plugin's Assets directory. On Windows 11, this should just work. On Windows 10, you may need to enable "Developer Mode" and/or use an Administrator command prompt. If you've put the two repos side-by-side as shown above, simply run the following in PowerShell:

```
cd cesium-unity
New-Item -ItemType Junction -Path "..\cesium-unity-samples\Assets\CesiumForUnity" -Target ".\Assets"
```

To setup the symbolic link in Mac or Linux:

```
cd cesium-unity-samples/Assets/
ln -s ../../cesium-unity/Assets CesiumForUnity
```

Unity only loads assets found in the game's Assets folder. By using a symlink, we keep the plugin's assets in the plugin's repo, making them much easier to manage with source control.

## Building

 In order to build successfully, the HintPaths in [CesiumForUnity.csproj](CesiumForUnity/CesiumForUnity.csproj) must be correct. On Mac, for instance, the UnityEngine.dll HintPath is:
`/Applications/Unity/Hub/Editor/2021.3.2f1/Unity.app/Contents/Managed/UnityEngine.dll`

The build consists of both C# and C++ code. The C# code must be compiled first, because its compilation process generates some code that is needed by the C++ build. To build the C# code, run the following in the root `cesium-unity` directory:

```
dotnet publish CesiumForUnity -c Debug
```

Replace `Debug` with `Release` for a release build.

This will do the following:

* Compile the Cesium for Unity C# code.
* Generate (on the fly) some new C# code for interop with C++ and compile that in, too.
* Generate C++ header and source files for the C++ side of the interop.
* Copy the built DLLs and PDBs to the `Assets` directory.

To build the C++ code, run the following from the `cesium-unity` directory:

```
cmake -B build -S .
cmake --build build --target install
```

(or just use `CMake: Configure` and `CMake: Install` from Visual Studio Code)

The CMake build will:

* Compile the DLL containing the C++ code.
* Copy the built DLLs and PDBs to the `Assets` directory.

## Running the Examples

You should now be able to open the cesium-unity-samples project in the Unity Editor and see Cesium datasets being streamed in.
