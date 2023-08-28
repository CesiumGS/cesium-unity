# Overview

This is a summary of the setup and workflows for developers who want to modify the Cesium for Unity plugin. If you just want to use Cesium for Unity in your own applications, see the main [README](../README.md).

## :computer: Building Cesium for Unity

### Prerequisites

* CMake v3.18 or later (the latest version is recommended)
* [.NET SDK v6.0 or later](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* If you're using Visual Studio, you need Visual Studio 2022.
* Unity 2021.3+ (the latest version of the Unity 2021.3 LTS release is recommended)
* On Windows, support for long file paths must be enabled, or you are likely to see build errors. See [Maximum Path Length Limitation](https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=registry#enable-long-paths-in-windows-10-version-1607-and-later).
* For best JPEG-decoding performance, you must have [nasm](https://www.nasm.us/) installed so that CMake can find it. Everything will work fine without it, just slower.

The built Cesium for Unity Assembly will run on much older versions of .NET, including the version of Mono included in Unity. However, these very recent versions are required for the C#<->C++ interop code generator (Reinterop).

To make sure things are set up correctly, open a command-prompt (PowerShell is a good choice on Windows) and run:

* `dotnet --version` and verify that it reports 6.0 or later
* `cmake --version` and verify that it reports 3.15 or later

### Setting up the development environment

Clone the [`cesium-unity-samples`](https://github.com/CesiumGS/cesium-unity-samples) (game) project anywhere you like:

```
git clone --recurse-submodules git@github.com:CesiumGS/cesium-unity-samples.git
```

Then, clone the `cesium-unity` (plugin) project to a folder named `com.cesium.unity` inside its `Packages` folder:

```
cd cesium-unity-samples/Packages
git clone --recurse-submodules git@github.com:CesiumGS/cesium-unity.git com.cesium.unity
```

Be sure to also clone the submodules. If you forgot the `--recurse-submodules` option when you cloned, run `git submodule update --init --recursive` inside the `com.cesium.unity` folder.

## Reinterop

Reinterop is a Roslyn (C# compiler) source generator that is automatically invoked by Unity while compiling the Cesium for Unity C# code, and generates C# <-> C++ interop layer.

To build Reinterop and publish it to Cesium for Unity's directory, run the following from the `Packages/com.cesium.unity` directory:

```
dotnet publish Reinterop~ -o .
```

This should be repeated if you modify Reinterop, or if you pull new changes that modify it.

For more details, see the [Reinterop README](../Reinterop~/README.md).

## Build for the Editor

To start the Cesium for Unity build process, open the `cesium-unity-samples` project in the Unity Editor. Unity will automatically compile the Cesium for Unity C# source code, invoking Reinterop along the way to generate the C# and C++ source code.

At this point, you can open the sample scene and you will see GameObjects with Cesium3DTileset and other Cesium behaviors attached to them. However, the Cesium functionality will not actually work yet. Instead, you'll see errors like this in the console:

```
DllNotFoundException: CesiumForUnityNative assembly:<unknown assembly> type:<unknown type> member:(null)
NotImplementedException: The native implementation is missing so OnValidate cannot be invoked.
```

This is because the C++ code has not yet been compiled. To compile the C++ code for use in the Editor, run:

```
cd cesium-unity-samples/Packages/com.cesium.unity/native~
cmake -B build -S . -DCMAKE_BUILD_TYPE=Debug
cmake --build build -j14 --target install --config Debug
```

The `-j14` tells CMake to build using 14 threads. A higher or lower number may be more suitable for your system.

To build a release build, use these commands instead:

```
cd cesium-unity-samples/Packages/com.cesium.unity/native~
cmake -B build -S . -DCMAKE_BUILD_TYPE=RelWithDebInfo
cmake --build build -j14 --target install --config RelWithDebInfo
```

Once this build/install completes, Cesium for Unity should work the next time Unity loads Cesium for Unity. You can get it to do so by either restarting the Editor, or by making a small change to any Cesium for Unity script (.cs) file in `Packages/com.cesium.unity/Runtime`.

## Building and Running Games

When you build and run a standalone game (i.e. with File -> Build Settings... or File -> Build and Run in the Unity Editor), Unity will automatically compile Cesium for Unity for the target platform. Then, by hooking into Unity build events, Cesium for Unity will build the corresponding native code for that platform by running CMake on the command-line. This can take a few minutes, and during that time Unity's progress bar will display a message stating the location of the build log file.

You can view build progress on Windows using the following PowerShell command:

```
cd cesium-unity-samples/Packages/com.cesium.unity
Get-Content -Path native~/build-Standalone/build.log -Wait
```

Replace `build-Standalone` with the name of the log file from the progress window.

Or on Linux or macOS:

```
cd cesium-unity-samples/Packages/com.cesium.unity
tail -f native~/build-Standalone/build.log
```

If the log indicates that CMake cannot be found, make sure it is installed and in your path. Restarting Unity to pick up path changes may help. If all else fails, change `"cmake"` in `CompileCesiumForUnityNative.cs` to the full path of your CMake executable.

## Running the Samples

The cesium-unity-samples project has several scenes that help you to quickly get running with Cesium for Unity. Go to `File -> Open Scene`, navigate to the `Scenes` directory, and select one of the sample scenes.

## Packaging Cesium for Unity

To create a release package of Cesium for Unity, suitable to be installed with the Unity Package Manager, do the following (adjust the Unity path for your system):

```
$ENV:UNITY="C:\Program Files\Unity\Hub\Editor\2021.3.13f1\Editor\Unity.exe"
mkdir -p c:\cesium\CesiumForUnityBuildProject\Packages
cd c:\cesium\CesiumForUnityBuildProject\Packages
git clone --recurse-submodules git@github.com:CesiumGS/cesium-unity.git com.cesium.unity
cd com.cesium.unity
dotnet publish Reinterop~ -o .
dotnet run --project Build~
```

On success, the built .tar.gz package is found in the root directory of the project (e.g. `c:\cesium\CesiumForUnityBuildProject`).

## Running Tests

In the Unity Editor, go to Window -> General -> Test Runner. Switch to the "Play Mode" tests and click "Run All".

You can also run the Cesium for Unity tests when the plugin is installed from the Package Manager. In this case, the tests won't show up in the Test Runner by default, though. To make them appear, edit the project's `Packages/manifest.json` file and add the following property to the JSON:

```
"testables": ["com.cesium.unity"]
```
