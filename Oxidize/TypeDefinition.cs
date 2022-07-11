using System;
using System.Collections.Generic;
using System.Text;

namespace Oxidize
{
    internal class TypeDefinition
    {
        public List<string> declarations = new List<string>();
        public List<string> definitions = new List<string>();
        public List<string> headerIncludes = new List<string>();
        public List<string> cppIncludes = new List<string>();
    }
}
