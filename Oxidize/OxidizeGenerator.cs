using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Oxidize;

[Generator]
public class OxidizeGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor RandomWarning = new DiagnosticDescriptor(id: "RANDOM001",
                                                                                              title: "Some random warning",
                                                                                              messageFormat: "Yep something went wrong '{0}'",
                                                                                              category: "SomeCategory",
                                                                                              DiagnosticSeverity.Warning,
                                                                                              isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(CSharpOxidizeAttribute.Generate);
        context.RegisterPostInitializationOutput(CSharpObjectHandle.Generate);

        // For each method in the Oxidize class, look at the types, methods, and properties it uses and create from
        // that a list of items to be generated (GenerationItems).
        IncrementalValuesProvider<IEnumerable<GenerationItem>> perMethodGenerationItems =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsOxidizeType,
                transform: GetOxidizeClass);

        // Consolidate the GenerationItems from the different methods into a single dictionary.
        IncrementalValueProvider<Dictionary<ITypeSymbol, GenerationItem>> generationItems =
            perMethodGenerationItems
                .Collect()
                .Select(CombineGenerationItems);

        // Process the generation items, for example, linking them together.
        IncrementalValuesProvider<GenerationItem> processedGenerationItems = generationItems.SelectMany(Process);

        var cppGenerator = context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider).Select((pair, _) => CreateCppGenerator(pair.Left, pair.Right));
        //var cppGenerator = context.CompilationProvider.Select(CreateCppGenerator);
        var withCppGenerator = processedGenerationItems.Combine(cppGenerator);

        // Generate C++ code.
        var typeDefinitions = withCppGenerator.Select((pair, _) => pair.Right.GenerateType(pair.Left));
        context.RegisterImplementationSourceOutput(typeDefinitions.Combine(cppGenerator), (context, pair) => pair.Right.WriteType(pair.Left));

        // Generate C++ initialization function
        var typesAndGenerator = typeDefinitions.Collect().Combine(cppGenerator);
        context.RegisterImplementationSourceOutput(typesAndGenerator, (context, pair) => pair.Right.WriteInitializeFunction(pair.Left));
        context.RegisterImplementationSourceOutput(typesAndGenerator, (context, pair) => CppObjectHandle.Generate(pair.Right.Options));

        //context.RegisterImplementationSourceOutput(typeDefinitions.Collect(), (context, )

        // Generate the initialization function after all the other C++ code is generated.
        //typeDefinitions.Coll
        //withCppGenerator.Collect().Select((pair, _) => pair.

        // Generate the required items
        context.RegisterSourceOutput(typesAndGenerator, (context, pair) => CSharpCodeGenerator.Generate(context, pair.Right.Options.Compilation, pair.Left));
        //context.RegisterImplementationSourceOutput(withCppGenerator, (context, pair) => pair.Right.Generate(context, pair.Left));
        //context.RegisterSourceOutput(processedGenerationItems, CppCodeGenerator.Generate);
    }

    private static Dictionary<ITypeSymbol, GenerationItem> CombineGenerationItems(ImmutableArray<IEnumerable<GenerationItem>> listOfItems, CancellationToken token)
    {
        Dictionary<ITypeSymbol, GenerationItem> result = new Dictionary<ITypeSymbol, GenerationItem>(SymbolEqualityComparer.Default);

        foreach (IEnumerable<GenerationItem> items in listOfItems)
        {
            foreach (GenerationItem item in items)
            {
                GenerationItem current;
                if (!result.TryGetValue(item.type, out current))
                {
                    current = new GenerationItem(item.type);
                    result.Add(item.type, current);
                }

                foreach (IMethodSymbol method in item.constructors)
                {
                    current.constructors.Add(method);
                }

                foreach (IMethodSymbol method in item.methods)
                {
                    current.methods.Add(method);
                }

                foreach (IPropertySymbol property in item.properties)
                {
                    current.properties.Add(property);
                }
            }
        }

        return result;
    }

    private static bool IsOxidizeType(SyntaxNode node, CancellationToken token)
    {
        var attributeNode = node as AttributeSyntax;
        if (attributeNode == null)
            return false;

        var name = attributeNode.Name;
        var simpleName = name as SimpleNameSyntax;
        if (simpleName != null)
            return simpleName.Identifier.Text == "Oxidize" || simpleName.Identifier.Text == "OxidizeAttribute";

        var qualifiedName = name as QualifiedNameSyntax;
        if (qualifiedName != null)
            return qualifiedName.Right.Identifier.Text == "Oxidize" || qualifiedName.Right.Identifier.Text == "OxidizeAttribute";

        return false;
    }

    private static IEnumerable<GenerationItem> GetOxidizeClass(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        SemanticModel semanticModel = ctx.SemanticModel;
        OxidizeWalker walker = new OxidizeWalker(semanticModel);

        var attributeSyntax = ctx.Node as AttributeSyntax;
        if (attributeSyntax == null)
            return Array.Empty<GenerationItem>();

        var classSyntax = attributeSyntax.Parent?.Parent as ClassDeclarationSyntax;
        if (classSyntax == null)
            return Array.Empty<GenerationItem>();

        foreach (MemberDeclarationSyntax memberSyntax in classSyntax.Members)
        {
            MethodDeclarationSyntax? methodSyntax = memberSyntax as MethodDeclarationSyntax;
            if (methodSyntax == null)
                continue;

            if (string.Equals(methodSyntax.Identifier.Text, "ExposeToCPP", StringComparison.InvariantCultureIgnoreCase))
                walker.Visit(methodSyntax);
        }

        return walker.GenerationItems.Values;
    }

    private string? GetName(NameSyntax? name)
    {
        if (name == null)
            return null;

        var simpleName = name as SimpleNameSyntax;
        if (simpleName != null)
            return simpleName.Identifier.Text;

        var qualifiedName = name as QualifiedNameSyntax;
        if (qualifiedName != null)
            return qualifiedName.Right.Identifier.Text;

        return null;
    }

    private static IEnumerable<GenerationItem> Process(Dictionary<ITypeSymbol, GenerationItem> items, CancellationToken token)
    {
        foreach (GenerationItem item in items.Values)
        {
            InheritanceChainer.Chain(item, items);
        }

        return items.Values;
    }

    private static CppCodeGenerator CreateCppGenerator(Compilation compilation, AnalyzerConfigOptionsProvider options)
    {
        CppGenerationContext cppContext = new CppGenerationContext(compilation);
        
        string? projectDir;
        if (!options.GlobalOptions.TryGetValue("build_property.projectdir", out projectDir))
            projectDir = "";

        string? cppHeaderPath;
        if (!options.GlobalOptions.TryGetValue("cpp_header_path", out cppHeaderPath))
            cppHeaderPath = "generated/include";

        string? cppSourcePath;
        if (!options.GlobalOptions.TryGetValue("cpp_source_path", out cppSourcePath))
            cppSourcePath = "generated/src";

        cppContext.OutputHeaderDirectory = Path.GetFullPath(Path.Combine(projectDir, cppHeaderPath));
        cppContext.OutputSourceDirectory = Path.GetFullPath(Path.Combine(projectDir, cppSourcePath));

        string? baseNamespace;
        if (!options.GlobalOptions.TryGetValue("base_namespace", out baseNamespace))
            baseNamespace = "";

        cppContext.BaseNamespace = baseNamespace;

        return new CppCodeGenerator(cppContext);
    }
}
