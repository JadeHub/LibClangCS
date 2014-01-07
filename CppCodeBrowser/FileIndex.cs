using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibClang;

namespace CppCodeBrowser
{
    internal interface IProjectFile
    {
        IEnumerable<Diagnostic> Diagnostics { get; }
    }

    /// <summary>
    /// Index of a source file.
    /// </summary>
    public interface ISourceFileIndex : IDisposable
    {
        LibClang.Cursor GetCursorAt(ICodeLocation location);
        IEnumerable<Diagnostic> Diagnostics { get; }
        TokenSet GetTokens(SourceRange range);
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
            LibClang.Cursor c = _tu.GetCursorAt(location.Path, location.Offset);
            if (c.Kind == CursorKind.NoDeclFound)
                return null;
            return c;
        }

        public IEnumerable<Diagnostic> Diagnostics 
        {
            get { return _tu.Diagnostics; }
        }

        public TokenSet GetTokens(SourceRange range)
        {
            TokenSet set = TokenSet.Create(_tu, range);
            return set;
        }
    }
}
