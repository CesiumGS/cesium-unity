
## Generate Bindings

The Unity / C# types and methods to expose to C++ are defined in `CesiumForUnityNativeBindings/CesiumForUnityTypes.json`. To generate the C# and C++ code that connects the two languages, run:

```
./UnityNativeScripting/generate-bindings.bat
```

The C# code is written to `CesiumForUnityNativeBindings/generated`. The C++ code is written to `CesiumForUnityNative/generated/src`.

## Build the C++ code

Build the C++ code using CMake:

```
cd CesiumForUnityNative
cmake -B build -S .
cmake --build build --target install
```

## Build the C# code

Build the C# code and publish it into your Unity project's Assets directory using `dotnet`. If `cesium-unity` is in a subdirectory of a Unity project, the command below is all you need:

```
dotnet publish CesiumForUnityNativeBindings -o ..\Assets
```

If your Unity project is in another location, change `..\Assets` to the full path to the project's Assets directory.
