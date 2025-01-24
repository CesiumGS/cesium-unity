using Reinterop;

namespace CesiumForUnity
{
    /// <summary>
    /// Internal helpers for testing Reinterop functionality.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::TestReinteropImpl", "TestReinteropImpl.h", true)]
    internal partial class TestReinterop
    {
        public partial bool CallThrowAnExceptionFromCppAndCatchIt();
        public partial bool CallThrowAnExceptionFromCppAndDontCatchIt();

        public static void ThrowAnException()
        {
            throw new System.Exception("Test Exception!");
        }
    }
}
