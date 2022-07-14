using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        var withCompilation = processedGenerationItems.Combine(context.CompilationProvider);

        // Generate the required items
        //context.RegisterSourceOutput(processedGenerationItems, CSharpCodeGenerator.Generate);
        context.RegisterImplementationSourceOutput(withCompilation, (context, pair) => Generate(context, pair.Right, pair.Left));
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

    private static void Generate(SourceProductionContext context, Compilation compilation, GenerationItem item)
    {
        CppGenerationContext cppContext = new CppGenerationContext(compilation);
        cppContext.BaseNamespace = "TestOxidize";

        CppCodeGenerator cpp = new CppCodeGenerator(cppContext);
        cpp.Generate(context, item);
    }
}
