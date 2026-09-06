namespace Reinterop
{
    internal interface IGenerateCSharpBody
    {
        IEnumerable<CSharpStatement> GenerateBody(CppGenerationContext context, CSharpFunctionCallableFromCpp function);
    }
}
