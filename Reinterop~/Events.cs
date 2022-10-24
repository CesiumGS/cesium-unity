using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class Events
    {
        public static void Generate(CppGenerationContext context, TypeToGenerate mainItem, TypeToGenerate currentItem, GeneratedResult result)
        {
            foreach (IEventSymbol evt in currentItem.Events)
            {
                GenerateSingleEvent(context, mainItem, result, evt);
            }
        }

        private static void GenerateSingleEvent(CppGenerationContext context, TypeToGenerate mainItem, GeneratedResult result, IEventSymbol evt)
        {
            if (evt.AddMethod == null || evt.RemoveMethod == null)
                return;

            Methods.GenerateSingleMethod(context, mainItem, result, evt.AddMethod);
            Methods.GenerateSingleMethod(context, mainItem, result, evt.RemoveMethod);
        }
    }
}
