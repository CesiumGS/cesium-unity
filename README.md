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

Before you begin, check that the HintPaths in [CesiumForUnity.csproj](CesiumForUnity/CesiumForUnity.csproj) are correct for your platform and version of Unity. On Mac, for instance, the UnityEngine.dll HintPath is:
`/Applications/Unity/Hub/Editor/2021.3.2f1/Unity.app/Contents/Managed/UnityEngine.dll`

Building Cesium for Unity requires building both an Editor and a non-Editor (i.e. built game) configuration, and each configuration has both a C# and a C++ part. So there are a totally of four projects, and all must be built before you can run Cesium for Unity.

The C# code must be compiled first, because its compilation process generates some code that is needed by the C++ build. To build the C# code for the non-Editor configuration, run the following in the root `cesium-unity` directory:

```
dotnet publish CesiumForUnity -c Debug -p:Editor=False
```

Replace `Debug` with `Release` for a release build.

This will do the following:

* Compile the Cesium for Unity C# code.
* Generate (on the fly) some new C# code for interop with C++ and compile that in, too.
* Generate C++ header and source files for the C++ side of the interop.
* Copy the built DLLs and PDBs to the `Assets/NonEditor` directory.

To build the C++ code for the non-Editor configuration, run the following from the `cesium-unity` directory:

```
cmake -B build -S . -DEDITOR=false
cmake --build build --target install -j14
```

The `-j14` tells CMake to build using 14 threads. A higher or lower number may be more suitable for your system.

The CMake build will:

* Compile the DLL containing the C++ code.
* Copy the built DLLs and PDBs to the `Assets/NonEditor` directory.

Next, build the Editor configuration of both the C# and C++ code:

```
dotnet publish CesiumForUnity -c Debug -p:Editor=True
cmake -B build -S . -DEDITOR=true
cmake --build build --target install -j14
```

Unity requires that the binaries for the non-Editor configuration _exist_, otherwise Cesium for Unity won't work at all, even in the Editor. But once you've built it once, if you're working exclusively in the Editor, you can iterate by only building the Editor configuration. The non-Editor binaries must exist, but they need not be up-to-date unless you're planning to build a game to run outside the Editor.

## Running the Examples

You should now be able to open the cesium-unity-samples project in the Unity Editor and see Cesium datasets being streamed in.
