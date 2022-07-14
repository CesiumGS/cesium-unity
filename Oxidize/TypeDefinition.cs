using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class TypeDefinition
    {
        /// <summary>
        /// The type's public declarations in the header .h file.
        /// </summary>
        public List<string> declarations = new List<string>();

        /// <summary>
        /// The type's definitions in the implementation .cpp file.
        /// </summary>
        public List<string> definitions = new List<string>();

        /// <summary>
        /// #includes required in the header .h file.
        /// </summary>
        public HashSet<string> headerIncludes = new HashSet<string>();

        /// <summary>
        /// #include required in the implementation .cpp file.
        /// </summary>
        public HashSet<string> cppIncludes = new HashSet<string>();

        /// <summary>
        /// Forward declarations to include the header .h file.
        /// </summary>
        public HashSet<string> forwardDeclarations = new HashSet<string>();

        /// <summary>
        /// The type's private declarations in the header .h file.
        /// </summary>
        public List<string> privateDeclarations = new List<string>();
    }
}
