namespace Reinterop
{
    // Recipes that expand to the fixed interop patterns used throughout the generated C++ code,
    // so those patterns are defined once instead of being hand-copied at every call site.
    internal static class CppInterop
    {
        private const string ExceptionVariableName = "reinteropException";

        /// <summary>
        /// Builds the statements that call a managed (C#) function pointer, check for an exception,
        /// and (optionally) return the converted result.
        /// </summary>
        /// <param name="functionPointerName">The name of the function pointer field to call.</param>
        /// <param name="callArguments">
        /// The already fully-converted call arguments, including the trailing "&amp;reinteropException".
        /// </param>
        /// <param name="resultTypeName">
        /// If the call produces a result that should be captured, the type to declare it as (typically
        /// "auto"). Null if the call returns void.
        /// </param>
        /// <param name="returnExpression">
        /// If a return statement should be generated, the (already converted) expression to return.
        /// </param>
        public static IReadOnlyList<CppStatement> CallManagedFunction(
            string functionPointerName,
            IEnumerable<string> callArguments,
            string? resultTypeName = null,
            string? returnExpression = null)
        {
            CppExpression call = new CppCall(
                new CppIdentifier(functionPointerName),
                callArguments.Select(argument => (CppExpression)new CppRaw(argument)).ToArray());

            List<CppStatement> statements = new()
            {
                new CppVariableDeclaration("void*", ExceptionVariableName, new CppRaw("nullptr")),
                resultTypeName != null
                    ? new CppVariableDeclaration(resultTypeName, "result", call)
                    : new CppExpressionStatement(call),
                new CppIf(
                    new CppBinary("!=", new CppIdentifier(ExceptionVariableName), new CppRaw("nullptr")),
                    new CppStatement[] { new CppThrow(TranslatedNativeException()) })
            };

            if (returnExpression != null)
                statements.Add(new CppReturn(new CppRaw(returnExpression)));

            return statements;
        }

        private static CppExpression TranslatedNativeException()
        {
            return new CppCall(new CppIdentifier("Reinterop::ReinteropNativeException"), new CppExpression[]
            {
                new CppCall(new CppIdentifier("::DotNet::System::Exception"), new CppExpression[]
                {
                    new CppCall(new CppIdentifier("::DotNet::Reinterop::ObjectHandle"), new CppExpression[]
                    {
                        new CppIdentifier(ExceptionVariableName)
                    })
                })
            });
        }
    }
}
