using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class Events
    {
        public static void Generate(CppGenerationContext context, GenerateTypeState state, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IEventSymbol evt in currentItem.Events)
            {
                GenerateSingleEvent(context, state, mainItem, result, evt);
            }
        }

        private static void GenerateSingleEvent(CppGenerationContext context, GenerateTypeState state, TypeToGenerate mainItem, GeneratedResult result, IEventSymbol evt)
        {
            if (evt.AddMethod == null || evt.RemoveMethod == null)
                return;

            Methods.GenerateSingleMethod(context, state, mainItem, result, evt.AddMethod);
            Methods.GenerateSingleMethod(context, state, mainItem, result, evt.RemoveMethod);
        }
    }
}
