
## Generate Bindings

The Unity / C# types and methods to expose to C++ are defined in `CesiumForUnityNativeBindings/CesiumForUnityTypes.json`. To generate the C# and C++ code that connects the two languages, run:

```
./UnitNativeScripting/generate-bindings.bat
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

Build the C# code using `dotnet`:

```
dotnet build cesium-unity.sln
```

If the parent directory contains an `Assets` folder, this step will also copy the built assembly and its dependencies there so that they can be loaded by Unity.
