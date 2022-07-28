using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Oxidize
{
    internal class CodeGenerator
    {
        public readonly CppGenerationContext Options;

        public CodeGenerator(CppGenerationContext options)
        {
            this.Options = options;
        }

        public void WriteInitializeFunction(ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCppInit init = GeneratedCppInit.Merge(results.Select(result => result == null ? new GeneratedCppInit() : result.CppInit));

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, "initializeOxidize.cpp"), init.ToSourceFileString(), Encoding.UTF8);

            string headerPath = Options.OutputHeaderDirectory;
            if (this.Options.BaseNamespace != null)
                headerPath = Path.Combine(headerPath, this.Options.BaseNamespace);

            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, "initializeOxidize.h"), init.ToHeaderFileString(), Encoding.UTF8);
        }

        public GeneratedResult? GenerateType(GenerationItem item)
        {
            INamedTypeSymbol? named = item.type as INamedTypeSymbol;
            if (named == null || named.IsGenericType)
            {
                Console.WriteLine("Skipping generation for generic type (for now).");
                return null;
            }

            CppType itemType = CppType.FromCSharp(this.Options, item.type);

            // No need to generate code for primitives.
            if (itemType.Kind == CppTypeKind.Primitive)
                return null;

            GeneratedResult result = new GeneratedResult(itemType);

            Interop.GenerateForType(this.Options, item, result);
            CppHandleManagement.Generate(this.Options, item, result);
            Constructors.Generate(this.Options, item, result);
            Casts.Generate(this.Options, item, result);
            Fields.Generate(this.Options, item, result);

            // Generate properties and methods throughout the whole inheritance hierarchy.
            GenerationItem? current = item;
            while (current != null)
            {
                Properties.Generate(this.Options, item, current, result);
                Methods.Generate(this.Options, item, current, result);
                current = current.baseClass;
            }

            // If this class has partial methods that are meant to be implemented in C++,
            // generate the necessary bindings.
            if (item.implClassName != null)
            {
                // TODO: parse out namespaces? Require user to specify them separately?
                CppType implementationType = new CppType(CppTypeKind.Unknown, Array.Empty<string>(), item.implClassName, null, 0, item.implHeaderName);
                result.CppImplementationInvoker = new GeneratedCppImplementationInvoker(implementationType);
                result.CSharpPartialMethodDefinitions = new GeneratedCSharpPartialMethodDefinitions(CSharpType.FromSymbol(this.Options.Compilation, item.type));
                
                MethodsImplementedInCpp.Generate(this.Options, item, result);
                Console.WriteLine(result.CSharpPartialMethodDefinitions.ToSourceFileString());
            }

            return result;
        }

        public void WriteCppCode(GeneratedResult? result)
        {
            if (result == null)
                return;

            CppType type = result.CppDefinition.Type;
            string headerPath = Path.Combine(new string[] { Options.OutputHeaderDirectory }.Concat(type.Namespaces).ToArray());
            Directory.CreateDirectory(headerPath);
            File.WriteAllText(Path.Combine(headerPath, type.Name + ".h"), result.CppDeclaration.ToHeaderFileString(), Encoding.UTF8);

            Directory.CreateDirectory(Options.OutputSourceDirectory);
            File.WriteAllText(Path.Combine(Options.OutputSourceDirectory, type.Name + ".cpp"), result.CppDefinition.ToSourceFileString(), Encoding.UTF8);
        }

        public static void WriteCSharpCode(SourceProductionContext context, Compilation compilation, ImmutableArray<GeneratedResult?> results)
        {
            GeneratedCSharpInit combined = GeneratedCSharpInit.Merge(results.Select(result => result == null ? new GeneratedCSharpInit() : result.CSharpInit));
            Console.WriteLine(combined.ToSourceFileString());
            context.AddSource("OxidizeInitializer", combined.ToSourceFileString());
        }
    }
}
