# Reinterop

Reinterop generates C# and C++ code to allow a .NET library to be seamlessly used from C++ code.

**Note: Reinterop currently requires the latest preview version of Visual Studio 2022, because it uses C# 11 / .NET 7. However, the generated code can be used almost anywhere, including in Unity's version of Mono.**

To use it, create a C# project and add the following to its .csproj file, adjusting the path to Reinterop as necessary.

```xml
  <ItemGroup>
      <ProjectReference Include="..\Reinterop\Reinterop.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>
```

This configures Reinterop as a Roslyn Analyzer (actually an Incremental Source Generator), which means that Reinterop will run as part of the C# compiler (Roslyn) while it is compiling your project, giving it the power to inspect the code being compiled and to inject additional code for compilation.

Next, add a new source file to your project that looks like this:

```csharp
using Reinterop;
using System;

namespace YourNamespace
{
    [Reinterop]
    internal class ConfigureReinterop
    {
        public void ExposeToCPP()
        {
            Console.WriteLine("hi");
        }
    }
}
```

The name of the namespace and class shown here are not important, but the `[Reinterop]` attribute and the `ExposeToCPP` method name are. The attribute tells Reinterop to look in this class for a method with that name.

The `ExposeToCPP` method acts like a bit of show-and-tell. It's never actually _executed_, but Reinterop makes every .NET type, method, property, etc. that is used in the method accessible from C++ by generating the necessary wrappers and interop code. The C# interop code is generated on the fly during compilation and automatically included in the build. The C++ code is written to a subdirectory called `generated` underneath the project directory. You can configure this by adding a file named `.globalconfig` to your project:

```conf
is_global = true

# The output path for generated C++ files.
# If this is relative, it is relative to the project directory.
cpp_output_path = ../CesiumForUnityNative/generated

# The namespace with which to prefix all C# namespaces. For example, if this
# property is set to "DotNet", then anything in the "System" namespaces in C#
# will be found in the "DotNet::System" namespace in C++.
base_namespace = DotNet

# The name of the DLL or SO containing the C++ code.
native_library_name = CesiumForUnityNative
```

You can then call into .NET from C++ by writing code like this:

```cpp
#include <DotNet/System/Console.h>
#include <DotNet/System/String.h>

void start() {
  DotNet::System::Console::WriteLine(DotNet::System::String("Hello World!"));
}
```

Build the generated C++ code into a shared library and copy it alongside the .NET assembly built by your C# project. Add C# code to call the generated method `Reinterop.ReinteropInitializer.Initialize()` at startup. This will initialize the interop layer and then call the `start` function in the C++ code. From there your C++ code can call back into .NET as it sees fit.

# Debugging

Debugging the source generator itself is possible, but not obvious. This page explain how it's done:

See https://github.com/JoanComasFdz/dotnet-how-to-debug-source-generator-vs2022

If you change the generator, be sure to compile it and then restart Visual Studio. Otherwise, every time you change your project, Visual Studio Intellisense will invoke the old, cached version of Reinterop and probably clobber the changes you were hoping to see.

# Debugging in Unity

The above won't work if Roslyn needs to be run by Unity. Instead, add this code to the `Initialize` method of `RoslynSourceGenerator.cs`:

```
if (!System.Diagnostics.Debugger.IsAttached)
{
    System.Diagnostics.Debugger.Launch();
}
```

Then, when Unity runs Roslyn and Roslyn runs Reinterop, Visual Studio will pop up offering to attach to the process. Let it do so, and you should be able to step and set breakpoints in the code generator.

# Temporarily disabling the code generator

It is sometimes useful to temporarily disable the code generator so that you can modify the generated code to try things out. This is easy to do. First, find the generated code. It may be in your project's `obj\Debug\netstandard2.1\generated\Reinterop\Reinterop.RoslynIncrementalGenerator` directory, or similar. If you can't find it, add the following to a `<PropertyGroup>` in your csproj (adjusting the path appropriately for your system):

```
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>C:\Dev\cesium-unity-samples\Assets\CesiumForUnity\generatedcsharp~</CompilerGeneratedFilesOutputPath>
```

In Unity, the above is a hassle. Instead, add a line to `csc.rsp`:

```
-generatedfilesout:"C:\place\to\write\files"
```

Once you have the generated code, open your project's .csproj and comment-out the section that adds the Reinterop project as an Analyzer:

```
  <!-- <ItemGroup>
    <ProjectReference Include="..\Reinterop\Reinterop.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup> -->
```

It may also look like this:

```
  <!-- <ItemGroup>
    <Analyzer Include="C:\some\path\Reinterop.dll" />
  </ItemGroup> -->
```

With that change, your project will no longer compile because the generated code is missing. To fix that, copy the generated code that you located previously into a folder called `generated` in your project's top-level directory. Your project should now build again.

To revert back to on-the-fly generation, delete the `generated` folder and uncomment the lines in the .csproj.

# Extending ExposeToCppSyntaxWalker

If you want to discover more elements in `ConfigureReinterop` so that you can generate code for them, you will probably need to know what syntax elements you're looking for, and it's sometimes not very obvious. But if you install the ".NET Compiler Platform SDK" in Visual Studio, you can go to View -> Other Windows -> Syntax Visualizer. Then find the code you want to capture in `ConfigureReinterop.cs` (or wherever), click it, and look at the elements in the Syntax Visualizer window. If the window is broken, make a trivial modification to the code (e.g. add a space and then remove it) and the the tree should appear.

To install the ".NET Compiler Platform SDK", just go to Add/Remove Programs, modify your Visual Studio installation, and find it under Individual Components.
