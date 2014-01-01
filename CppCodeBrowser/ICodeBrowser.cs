
namespace CppCodeBrowser
{
   /* public struct CodeLocation
    {
        public readonly string Path;
        public readonly int Offset;
    }*/

    public interface ICodeBrowser
    {
        ICodeLocation[] GetReferencesToItemAt(ICodeLocation location);
    }
}
