namespace Reinterop
{
    internal class GenerateTypeState
    {
        /// <summary>
        /// Holds a cache of generated methods for the current type being generated. This is used to avoid generating the same method multiple times,
        /// especially in cases where a method is generic and has multiple specializations. The key is the Name property of the CppInteropFunction.
        /// </summary>
        public Dictionary<string, CppInteropFunction> MethodCache;

        public GenerateTypeState()
        {
            this.MethodCache = new Dictionary<string, CppInteropFunction>();
        }
    }
}
