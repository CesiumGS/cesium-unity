using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class TypeDefinition
    {
        /// <summary>
        /// The type's declarations in the header .h file.
        /// </summary>
        public List<string> declarations = new List<string>();

        /// <summary>
        /// The type's definitions in the implementation .cpp file.
        /// </summary>
        public List<string> definitions = new List<string>();

        /// <summary>
        /// #includes required in the header .h file.
        /// </summary>
        public List<string> headerIncludes = new List<string>();

        /// <summary>
        /// #include required in the implementation .cpp file.
        /// </summary>
        public List<string> cppIncludes = new List<string>();

        /// <summary>
        /// Forward declarations to include the header .h file.
        /// </summary>
        public List<string> forwardDeclarations = new List<string>();
    }
}
