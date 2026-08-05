namespace Reinterop
{
    // Recipes that expand to the fixed interop patterns used throughout the generated C++ code,
    // so those patterns are defined once instead of being hand-copied at every call site.
    internal static class CppInterop
    {
        private const string ExceptionVariableName = "reinteropException";

        /// <summary>
        /// Builds the statements that call a managed (C#) function pointer, check for an exception,
        /// and (optionally) return the converted result. The trailing "reinteropException" out-parameter
        /// is added and checked automatically - callers should not include it in <paramref name="arguments"/>.
        /// </summary>
        /// <param name="functionPointer">The function pointer field to call.</param>
        /// <param name="arguments">
        /// The call's own arguments (not including the exception out-parameter, which this method adds
        /// automatically). Use <see cref="CppArgument.OutParameter"/> for any argument (e.g. a
        /// struct-return rewrite) that's produced via an out-parameter instead of a plain value.
        /// </param>
        /// <param name="resultTypeName">
        /// If the call's own return value should be captured, the type to declare it as (typically
        /// "auto"). Null if the call returns void, or if the result is instead produced via a
        /// <see cref="CppArgument.OutParameter"/> in <paramref name="arguments"/>.
        /// </param>
        /// <param name="returnExpression">
        /// If a return statement should be generated, the (already converted) expression to return.
        /// </param>
        /// <param name="resultVariableName">The name to declare the captured result variable as.</param>
        public static IReadOnlyList<CppStatement> CallManagedFunction(
            CppExpression functionPointer,
            IReadOnlyList<CppArgument> arguments,
            string? resultTypeName = null,
            CppExpression? returnExpression = null,
            string resultVariableName = "result")
        {
            List<CppStatement> statements = new()
            {
                new CppVariableDeclaration("void*", ExceptionVariableName, new CppRaw("nullptr"))
            };

            List<CppExpression> callArguments = new();
            foreach (CppArgument argument in arguments)
            {
                switch (argument)
                {
                    case CppOutParameterArgument outParameter:
                        statements.Add(new CppVariableDeclaration(outParameter.TypeName, outParameter.Name));
                        callArguments.Add(new CppUnary("&", new CppIdentifier(outParameter.Name)));
                        break;
                    case CppValueArgument value:
                        callArguments.Add(value.Expression);
                        break;
                }
            }
            callArguments.Add(new CppUnary("&", new CppIdentifier(ExceptionVariableName)));

            CppExpression call = new CppCall(functionPointer, callArguments);

            statements.Add(resultTypeName != null
                ? new CppVariableDeclaration(resultTypeName, resultVariableName, call)
                : new CppExpressionStatement(call));

            statements.Add(new CppIf(
                new CppBinary("!=", new CppIdentifier(ExceptionVariableName), new CppRaw("nullptr")),
                new CppStatement[] { new CppThrow(TranslatedNativeException()) }));

            if (returnExpression != null)
                statements.Add(new CppReturn(returnExpression));

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

        /// <summary>
        /// Wraps <paramref name="body"/> (the extern "C" function's own call implementation - the
        /// reverse direction of <see cref="CallManagedFunction"/>, i.e. C++ calling back into C#, or
        /// user C++ code implementing a partial method) in the standard three-way catch that
        /// translates any exception thrown by it into the "reinteropException" out-parameter:
        /// a thrown <c>ReinteropNativeException</c> is unwrapped back to its original managed
        /// exception, any other <c>std::exception</c> is wrapped in a new managed exception carrying
        /// its message, and anything else becomes a generic managed exception.
        /// </summary>
        /// <param name="onException">
        /// The statements to run after the out-parameter is set, in each of the three catch clauses -
        /// typically a single return statement (or none, for a void function).
        /// </param>
        public static IReadOnlyList<CppStatement> TranslateExceptionsToOutParameter(
            IReadOnlyList<CppStatement> body,
            IReadOnlyList<CppStatement> onException)
        {
            CppStatement setException(CppExpression value) =>
                new CppAssignment(new CppUnary("*", new CppIdentifier(ExceptionVariableName)), value);

            CppExpression reinteropNativeExceptionValue = new CppRaw(
                "::DotNet::Reinterop::ObjectHandle(e.GetDotNetException().GetHandle()).Release()");
            CppExpression stdExceptionValue = new CppRaw(
                "::DotNet::Reinterop::ReinteropException(::DotNet::System::String(e.what())).GetHandle().Release()");
            CppExpression unknownExceptionValue = new CppRaw(
                "::DotNet::Reinterop::ReinteropException(::DotNet::System::String(\"An unknown native exception occurred.\")).GetHandle().Release()");

            return new CppStatement[]
            {
                new CppTry(body, new[]
                {
                    new CppCatch("::DotNet::Reinterop::ReinteropNativeException", "e",
                        new[] { setException(reinteropNativeExceptionValue) }.Concat(onException).ToArray()),
                    new CppCatch("std::exception", "e",
                        new[] { setException(stdExceptionValue) }.Concat(onException).ToArray()),
                    new CppCatch(null, null,
                        new[] { setException(unknownExceptionValue) }.Concat(onException).ToArray())
                })
            };
        }
    }
}
