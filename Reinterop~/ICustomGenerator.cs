namespace Reinterop
{
    /// <summary>
    /// Allows for custom code generation for a particular type. Register
    /// with <see cref="ReinteropGenerationContext.CustomGenerators"/>.
    /// </summary>
    internal interface ICustomGenerator
    {
        IEnumerable<TypeToGenerate> GetDependencies(ReinteropGenerationContext context);

        GeneratedResult? Generate(ReinteropGenerationContext context, TypeToGenerate type, GeneratedResult? generated);
    }
}
