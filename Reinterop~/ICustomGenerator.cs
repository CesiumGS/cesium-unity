namespace Reinterop
{
    /// <summary>
    /// Allows for custom code generation for a particular type. Register
    /// with <see cref="CppGenerationContext.CustomGenerators"/>.
    /// </summary>
    internal interface ICustomGenerator
    {
        IEnumerable<TypeToGenerate> GetDependencies(CppGenerationContext context);

        GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated);
    }
}
