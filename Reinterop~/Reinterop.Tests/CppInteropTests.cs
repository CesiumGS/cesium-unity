using NUnit.Framework;

namespace Reinterop.Tests
{
    /// <summary>
    /// Characterization tests for CppInterop's exception translation recipe.
    /// </summary>
    public class CppInteropTests
    {
        [Test]
        public void TranslateExceptionsToOutParameter_VoidReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CppStatement> body = CppInterop.TranslateExceptionsToOutParameter(
                new CppStatement[] { new CppRawStatement("DoTheThing();") },
                Array.Empty<CppStatement>());

            string expected = string.Join(Environment.NewLine, new[]
            {
                "try {",
                "    DoTheThing();",
                "} catch (::DotNet::Reinterop::ReinteropNativeException& e) {",
                "    *reinteropException = ::DotNet::Reinterop::ObjectHandle(e.GetDotNetException().GetHandle()).Release();",
                "} catch (std::exception& e) {",
                "    *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String(e.what())).GetHandle().Release();",
                "} catch (...) {",
                "    *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String(\"An unknown native exception occurred.\")).GetHandle().Release();",
                "}"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }

        [Test]
        public void TranslateExceptionsToOutParameter_ValueReturn_MatchesOriginalTemplate()
        {
            IReadOnlyList<CppStatement> body = CppInterop.TranslateExceptionsToOutParameter(
                new CppStatement[] { new CppRawStatement("auto result = DoTheThing();"), new CppRawStatement("return result;") },
                new CppStatement[] { new CppRawStatement("return nullptr;") });

            string expected = string.Join(Environment.NewLine, new[]
            {
                "try {",
                "    auto result = DoTheThing();",
                "    return result;",
                "} catch (::DotNet::Reinterop::ReinteropNativeException& e) {",
                "    *reinteropException = ::DotNet::Reinterop::ObjectHandle(e.GetDotNetException().GetHandle()).Release();",
                "    return nullptr;",
                "} catch (std::exception& e) {",
                "    *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String(e.what())).GetHandle().Release();",
                "    return nullptr;",
                "} catch (...) {",
                "    *reinteropException = ::DotNet::Reinterop::ReinteropException(::DotNet::System::String(\"An unknown native exception occurred.\")).GetHandle().Release();",
                "    return nullptr;",
                "}"
            });

            Assert.That(CppPrinter.Print(body), Is.EqualTo(expected));
        }
    }
}
