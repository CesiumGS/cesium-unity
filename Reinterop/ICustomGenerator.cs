namespace Oxidize
{
    /// <summary>
    /// Allows for custom code generation for a particular type. Register
    /// with <see cref="CppGenerationContext.CustomGenerators"/>.
    /// </summary>
    internal interface ICustomGenerator
    {
        GeneratedResult? Generate(CppGenerationContext context, TypeToGenerate type, GeneratedResult? generated);
    }
}
