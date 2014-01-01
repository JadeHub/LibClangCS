using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppCodeBrowser
{
    /// <summary>
    /// Index of a source file.
    /// </summary>
    public interface IFileIndex : IDisposable
    {
        LibClang.Cursor GetCursor(ICodeLocation location);
    }

    public class FileIndex : IFileIndex, IDisposable
    {
        #region Data

        private readonly LibClang.TranslationUnit _tu;

        #endregion

        public FileIndex(LibClang.TranslationUnit tu)
        {
            _tu = tu;
        }

        public void Dispose()
        {
            _tu.Dispose();
        }

        public LibClang.Cursor GetCursor(ICodeLocation location)
        {
            return _tu.GetCursorAt(location.Path, location.Offset);
        }
    }
}
