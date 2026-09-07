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

            GenerateSingleAccessor(context, mainItem, result, evt, evt.AddMethod, isAdd: true);
            GenerateSingleAccessor(context, mainItem, result, evt, evt.RemoveMethod, isAdd: false);
        }

        private static void GenerateSingleAccessor(CppGenerationContext context, TypeToGenerate item, GeneratedResult result, IEventSymbol evt, IMethodSymbol method, bool isAdd)
        {
            CSharpFunctionCallableFromCpp interop = new CSharpFunctionCallableFromCpp(context, item.Type)
                .Name(method.Name)
                .ReturnType(method.ReturnType)
                .Parameters(method.Parameters)
                .Static(method.IsStatic)
                .Body(new CSharpBodyAddRemoveEventDelegate(evt, isAdd));

            result.InteropFunctions2.Add(interop);
        }
    }
}
