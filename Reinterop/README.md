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

Build the generated C++ code into a shared library and copy it alongside the .NET assembly built by your C# project. Add C# code to call the generated method `Reinterop.ReinteropInitializer.Initialize()` at startup. This will initialize the interop layer and then call the `start` function in the C++ code. From there your C++ code can call back into .NET as it fees fit.

# Debugging

Debugging the source generator itself is possible, but not obvious. This page explain how it's done:

See https://github.com/JoanComasFdz/dotnet-how-to-debug-source-generator-vs2022

If you change the generator, be sure to compile it and then restart Visual Studio. Otherwise, every time you change your project, Visual Studio Intellisense will invoke the old, cached version of Reinterop and probably clobber the changes you were hoping to see.
