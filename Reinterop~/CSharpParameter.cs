namespace Reinterop
{
    internal class CSharpParameter
    {
        public readonly CSharpType Type;
        public readonly string Name;

        public CSharpParameter(CSharpType type, string name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}
