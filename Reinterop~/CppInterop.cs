namespace Reinterop
{
    // Recipes that expand to the fixed interop patterns used throughout the generated C++ code,
    // so those patterns are defined once instead of being hand-copied at every call site.
    internal static class CppInterop
    {
        private const string ExceptionVariableName = "reinteropException";

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
        /// reverse direction of the managed-function call pattern, i.e. C++ calling back into C#, or
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
