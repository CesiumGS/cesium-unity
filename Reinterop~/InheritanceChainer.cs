using Microsoft.CodeAnalysis;

namespace Reinterop
{
    internal class InheritanceChainer
    {
        /// <summary>
        /// Sets the baseClass and interfaces properties of a given GenerationItem from the appropriate
        /// items in the full list of GenerationItems.
        /// </summary>
        /// <param name="item">The item for which to set the baseClass and interfaces properties.</param>
        /// <param name="items">The full list of generation items.</param>
        public static void Chain(TypeToGenerate item, Dictionary<ITypeSymbol, TypeToGenerate> items)
        {
            // This item's immediate base class might not be generated. So walk up the inheritance chain
            // until we find one that is.
            ITypeSymbol? current = item.Type.BaseType;
            while (current != null)
            {
                TypeToGenerate baseGenerationItem;
                if (items.TryGetValue(current, out baseGenerationItem))
                {
                    item.BaseClass = baseGenerationItem;
                    break;
                }
                current = current.BaseType;
            }

            foreach (ITypeSymbol anInterface in item.Type.AllInterfaces)
            {
                TypeToGenerate interfaceGenerationItem;
                if (items.TryGetValue(anInterface, out interfaceGenerationItem))
                {
                    item.Interfaces.Add(interfaceGenerationItem);
                }
            }
        }
    }
}
