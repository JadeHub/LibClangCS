﻿
namespace CppCodeBrowser
{
    public interface ICodeLocation
    {
        string Path { get; }
        int Offset { get; }        
    }

    public struct CodeLocation : ICodeLocation
    {
        public CodeLocation(LibClang.SourceLocation loc) : this()
        {
            Path = loc.File.Name;
            Offset = loc.Offset;            
        }

        public CodeLocation(string path, int offset) : this()
        {
            Path = path;
            Offset = offset;            
        }

        public string Path
        {
            get;
            private set;
        }

        public int Offset
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Path, Offset);
        }

        public override bool Equals(object obj)
        {
            if (obj is CodeLocation)
            {
                CodeLocation rhs = (CodeLocation)obj;
                return rhs.Path == Path && rhs.Offset == Offset;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
