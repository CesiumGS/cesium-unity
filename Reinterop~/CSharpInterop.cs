using Microsoft.CodeAnalysis;

namespace Reinterop
{
    // The C# mirror of CppInterop.cs - recipes that expand to the fixed interop patterns used
    // throughout the generated C# code, so those patterns are defined once instead of being
    // hand-copied at every call site.
    internal static class CSharpInterop
    {
        private const string ExceptionVariableName = "reinteropException";

        /// <summary>
        /// Builds the statements that declare the "reinteropException" out-parameter, call a native
        /// (C++) function passing it, check for an exception, and (optionally) return the converted
        /// result. The trailing "&amp;reinteropException" call argument is added automatically - callers
        /// should not include it in <paramref name="arguments"/>.
        /// </summary>
        /// <param name="functionName">The native function to call.</param>
        /// <param name="arguments">
        /// The call's own arguments (not including the exception out-parameter, which this method
        /// adds automatically).
        /// </param>
        /// <param name="resultType">
        /// If the call's own return value should be captured, the type to declare it as. Null if the call returns void.
        /// </param>
        /// <param name="returnExpression">
        /// If a return statement should be generated, the (already converted) expression to return.
        /// Null if the call returns void.
        /// </param>
        /// <param name="resultVariableName">The name to declare the captured result variable as.</param>
        public static IReadOnlyList<CSharpStatement> CallNativeFunction(
            CppGenerationContext context,
            CSharpExpression functionName,
            IReadOnlyList<CSharpExpression> arguments,
            CSharpType? resultType = null,
            CSharpExpression? returnExpression = null,
            string resultVariableName = "result")
        {
            List<CSharpStatement> statements = new()
            {
                new CSharpVariableDeclaration(
                    CSharpType.FromSymbol(context, context.Compilation.GetSpecialType(SpecialType.System_IntPtr)),
                    ExceptionVariableName,
                    new CSharpIdentifier("System.IntPtr.Zero"))
            };

            List<CSharpExpression> callArguments = new(arguments)
            {
                new CSharpUnary("&", new CSharpIdentifier(ExceptionVariableName))
            };

            CSharpExpression call = new CSharpCall(functionName, callArguments);

            statements.Add(resultType != null
                ? new CSharpVariableDeclaration(resultType, resultVariableName, call)
                : new CSharpExpressionStatement(call));

            statements.Add(new CSharpIf(
                new CSharpBinary("!=", new CSharpIdentifier(ExceptionVariableName), new CSharpIdentifier("System.IntPtr.Zero")),
                new CSharpStatement[] { new CSharpThrow(TranslatedManagedException(context)) }));

            if (returnExpression != null)
                statements.Add(new CSharpReturn(returnExpression));

            return statements;
        }

        private static CSharpExpression TranslatedManagedException(CppGenerationContext context)
        {
            return new CSharpCast(CSharpType.FromSymbol(context, context.Compilation.GetTypeByMetadataName("System.Exception")!),
                new CSharpCall(new CSharpIdentifier("Reinterop.ObjectHandleUtility.GetObjectAndFreeHandle"),
                    new CSharpExpression[] { new CSharpIdentifier(ExceptionVariableName) }));
        }
    }
}
