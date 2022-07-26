namespace Oxidize
{
    internal class CppTypeReference
    {
        public CppTypeReference(CppType type, bool requiresCompleteDefinition = false)
        {
            this.Type = type;

            if (!this.Type.CanBeForwardDeclared)
                RequiresCompleteDefinition = true;
            else
                RequiresCompleteDefinition = requiresCompleteDefinition;
        }

        public CppType Type;
        public bool RequiresCompleteDefinition = false;
    }
}
