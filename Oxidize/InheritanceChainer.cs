using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class InheritanceChainer
    {
        /// <summary>
        /// Sets the baseClass and interfaces properties of a given GenerationItem from the appropriate
        /// items in the full list of GenerationItems.
        /// </summary>
        /// <param name="item">The item for which to set the baseClass and interfaces properties.</param>
        /// <param name="items">The full list of generation items.</param>
        public static void Chain(GenerationItem item, Dictionary<ITypeSymbol, GenerationItem> items)
        {
            // This item's immediate base class might not be generated. So walk up the inheritance chain
            // until we find one that is.
            ITypeSymbol? current = item.type.BaseType;
            while (current != null)
            {
                GenerationItem baseGenerationItem;
                if (items.TryGetValue(current, out baseGenerationItem))
                {
                    item.baseClass = baseGenerationItem;
                    break;
                }
                current = current.BaseType;
            }

            foreach (ITypeSymbol anInterface in item.type.AllInterfaces)
            {
                GenerationItem interfaceGenerationItem;
                if (items.TryGetValue(anInterface, out interfaceGenerationItem))
                {
                    item.interfaces.Add(interfaceGenerationItem);
                }
            }
        }
    }
}
