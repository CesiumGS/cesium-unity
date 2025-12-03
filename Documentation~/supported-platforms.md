# Supported Platforms {#supported-platforms}

Cesium for Unity makes use of native code - not just C# code - which means that it must be compiled for each platform on which it runs. The Cesium for Unity release packages include pre-built binaries for a number of platforms.

The following platforms are currently supported:

- [Windows](#windows)
- [macOS](#macos)
- [Android / Meta Quest](#android)
- [iOS](#ios)
- [Universal Windows Platform](#uwp)
- [Web](#web)
- [Others?](#others)

## Windows {#windows}

Cesium for Unity supports Windows as both a Unity Editor and Player platform. See the [Unity Windows](https://docs.unity3d.com/Manual/Windows.html) documentation for general information about develping Unity applications for the Windows platform.

Cesium for Unity requires an x86-64 processor on Windows; ARM64 is not currently supported. Most versions of Windows that can run Unity should be able to run Cesium for Unity as well.

## macOS {#macos}

Cesium for Unity supports macOS as both a Unity Editor and Player platform. See the [Unity macOS](https://docs.unity3d.com/Manual/AppleMac.html) documentation for general information about develping Unity applications for the macOS platform.

Cesium for Unity runs natively on both Intel x86-64 and Applie Silicon (M1, M2, M3, M4, M5, etc.) processors, and on macOS 10.15+.

## Android / Meta Quest {#android}

Cesium for Unity supports Android and Meta Quest as Unity Player platforms. See the [Unity Android](https://docs.unity3d.com/Manual/android.html) documentation for general information about develping Unity applications for the Android platform.

Cesium for Unity supports both Intel x86-64 and ARM64 processors. Most Android devices use ARM64, but the x86-64 support is important to support the Magic Leap 2 device. 32-bit Android devices with ARMv7 processors are _not_ supported.

The Android Player Settings must be configured as follows:

| *Setting* | *Value* |
|-----------|---------|
| Scripting Backend | `IL2CPP` |
| Target Architectures | `ARM64` _or_ `x86-64`, _not_ `ARMv7` |
| Internet Access | `Require` |

## iOS {#ios}

Cesium for Unity supports iOS as a Unity Player platform. See the [Unity iOS](https://docs.unity3d.com/Manual/iphone.html) documentation for general information about developing Unity applications for the iOS platform.

## Universal Windows Platform {#uwp}

Cesium for Unity supports Universal Windows Platform (UWP) as a Unity Player platform. This is most useful when targetting HoloLens devices, but it can also be used to build desktop applications.

Both the "Intel 64-bit" and "ARM 64-bit" architectures are supported. 32-bit platforms are _not_ supported.

Be sure that the `InternetClient` capability is enabled in the Player Settings.

## Web {#web}

Cesium for Unity supports the Web as a Unity Player platform. Both WebGL and WebGPU are supported. Unity 6 or later is required. See the [Unity Web](https://docs.unity3d.com/Manual/webgl.html) documentation for general information about developing Unity applications for the Web.

You _must_ turn on "Enable Native C/C++ Multithreading" in the Player Settings. If you don't, you will see errors like this at build time:

> wasm-ld: error: Library/PackageCache/com.cesium.unity@57778A6EB39E/Plugins/WebGL/libCesiumForUnityNative-Runtime.a(CesiumCreditSystem.cpp.o): undefined symbol: __wasm_lpad_context
wasm-ld: error: Library/PackageCache/com.cesium.unity@57778A6EB39E/Plugins/WebGL/libCesiumForUnityNative-Runtime.a(CesiumCreditSystem.cpp.o): undefined symbol: _Unwind_CallPersonality

When using the "Build and Run" button in the Unity Editor, Unity starts a simple web server to host your web application, and this works great for testing. To host the built application on your own web server, however, you need to ensure that the proper HTTP response headers are set. As explained in the [Unity documentation](https://docs.unity3d.com/Manual/webgl-technical-overview.html#multithreading-support), these required response headers are:

* `Cross-Origin-Opener-Policy: same-origin`
* `Cross-Origin-Embedder-Policy: require-corp`
* `Cross-Origin-Resource-Policy: cross-origin`

Failure to set these headers will result in a popup when the application loads saying, "Your web browser does not support multithreading."

Your custom web server must also be configured to serve files with the extension `.br` with the following response header:

* `Content-Encoding: br`

If this header is missing, the web page will display an error explaining that this needs to be done. If this is difficult to arrange, you can avoid the need by enabling the `Decompression Fallback` option in the Player Settings. However, this will increase startup time.

## Others? {#others}

Cesium for Unity's native code is quite portable, so building it for other platforms should be possible. However, it is _not_ possible to target other platforms when using the released Cesium for Unity packages. Instead, you will need to build it from the source code on [GitHub](https://github.com/CesiumGS/cesium-unity).

The basic steps are as follows:

1. Make sure you're able to build and run Cesium for Unity for the Unity Editor on your development machine by following the [Developer Setup Instructions](#developer-setup). If you're trying to run the Editor on a previously-unsupported platform, you will need to adapt these steps for your platform.
2. Add the new platform to the list of `SupportedPlatforms` near the top of [Build~\Package.cs](https://github.com/CesiumGS/cesium-unity/blob/main/Build~/Package.cs). This is the name that is used to refer to the platform on the command-line.
3. Add new code to the `Run` method in that same file to launch Unity and build it for the new platform. Copy an existing platform and modify it as appropriate.
4. Add a new function for your platform to [Editor/BuildCesiumForUnity.cs](https://github.com/CesiumGS/cesium-unity/blob/main/Editor/BuildCesiumForUnity.cs), again copying an existing platform as appropriate.
5. The hardest part: Modify [Editor/CompileCesiumForUnityNative.cs](https://github.com/CesiumGS/cesium-unity/blob/main/Editor/CompileCesiumForUnityNative.cs) to teach it how to compile the native code for your platform. Again, it's usually easiest to start with a similar platform, copy it, and modify as appropriate.
6. Modify the two `ConfigureReinterop.cs` files, adding a new `#if`-protected `CppOutputPath` for your platform. Here the `UNITY_*` symbols must match the names Unity uses for the platform.

If all that works flawlessly (good luck!) you will be able to build a player for your new platform from the Editor UI, and it will build the native code to go with it. You can also build from the command-line by running something like:

```
dotnet run --project Build~ package --platform MyNewPlatform
```

Where `MyNewPlatform` is the name used in step (2).