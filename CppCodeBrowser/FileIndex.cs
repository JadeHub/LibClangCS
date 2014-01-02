using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibClang;

namespace CppCodeBrowser
{
    /// <summary>
    /// Index of a source file.
    /// </summary>
    public interface ISourceFileIndex : IDisposable
    {
        LibClang.Cursor GetCursorAt(ICodeLocation location);
        bool IncludesHeader(string path);
    }

    public class SourceFileIndex : ISourceFileIndex, IDisposable
    {
        #region Data

        private readonly LibClang.TranslationUnit _tu;
        
        #endregion

        public SourceFileIndex(LibClang.TranslationUnit tu)
        {
            _tu = tu;
        }

        public void Dispose()
        {
            _tu.Dispose();
        }

        public LibClang.Cursor GetCursorAt(ICodeLocation location)
        {
            return _tu.GetCursorAt(location.Path, location.Offset);
        }

        public bool IncludesHeader(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            foreach (TranslationUnit.HeaderInfo header in _tu.HeaderFiles)
            {
                if (System.IO.Path.GetFullPath(header.Path) == fullPath)
                    return true;
            }
            return false;
        }
    }
}
