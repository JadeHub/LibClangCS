using LibClang;
using System.Collections.Generic;

namespace CppCodeBrowser
{
    public class JumpToBrowser : ICodeBrowser
    {
        #region Data

        private IProjectIndex _index;

        #endregion

        #region Constructor

        public JumpToBrowser(IProjectIndex index)
        {
            _index = index;
        }

        #endregion

        #region ICodeBrowser

        public IEnumerable<ICodeLocation> BrowseFrom(ICodeLocation loc)
        {
            HashSet<ICodeLocation> results = new HashSet<ICodeLocation>();
            if (_index.IsSourceFile(loc.Path))
            {
                ISourceFileIndex fileIndex = _index.GetIndexForSourceFile(loc.Path);
                ICodeLocation result = JumpTo(fileIndex, loc);
                if (result != null)
                    results.Add(result);
            }
            else if (_index.IsHeaderFile(loc.Path))
            {
                foreach (ISourceFileIndex fileIndex in _index.GetIndexesForHeaderFile(loc.Path))
                {
                    ICodeLocation result = JumpTo(fileIndex, loc);
                    if (result != null)
                        results.Add(result);
                }                
            }
            return results;
        }

        #endregion

        #region Private Methods

        private ICodeLocation JumpTo(ISourceFileIndex fileIndex, ICodeLocation loc)
        {
            Cursor c = fileIndex.GetCursorAt(loc);
            return c == null ? null : JumpTo(c);
        }

        private ICodeLocation JumpTo(Cursor c)
        {
            LibClang.Cursor result = null;

            if (JumpToDeclaration(c))
            {
                //For constructor and method definitions we browse to the declaration
                result = FindDeclaration(c.SemanticParentCurosr, c.Usr);
            }
            else
            {
                if (c.CursorReferenced != null)
                    result = c.CursorReferenced;
                else if (c.Definition != null)
                    result = c.Definition;
                else
                    result = c.CanonicalCursor;
            }
            if (result == null)
                return null;
            return new CodeLocation(result.Location.File.Name, result.Location.Offset);
        }

        private static bool JumpToDeclaration(LibClang.Cursor c)
        {
            return ((c.Kind == CursorKind.Constructor ||
                        c.Kind == CursorKind.Destructor ||
                        c.Kind == CursorKind.CXXMethod ||
                        c.Kind == CursorKind.FunctionDecl)
                && c.IsDefinition);
        }

        private Cursor FindDeclaration(Cursor parent, string usr)
        {
            Cursor result = null;
            parent.VisitChildren(delegate(Cursor cursor, Cursor p)
            {
                if (cursor.Usr == usr && cursor.IsDefinition == false)
                {
                    result = cursor;
                    return Cursor.ChildVisitResult.Break;
                }
                return Cursor.ChildVisitResult.Continue;
            });
            return result;
        }

        #endregion
    }
}
