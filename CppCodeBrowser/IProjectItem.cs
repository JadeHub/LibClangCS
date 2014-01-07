using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppCodeBrowser
{
    public enum ProjectItemType
    {
        SourceFile,
        HeaderFile
    }

    /// <summary>
    /// Represents either a source or header file.
    /// </summary>
    public interface IProjectItem
    {
        /// <summary>
        /// Type of this item.
        /// </summary>
        ProjectItemType Type { get; }

        /// <summary>
        /// File path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// TranslationUnit objects which referenced this file. 
        /// 
        /// For source files this will only contain the source file's TranslationUnit object. 
        /// For header files it will contain all TranslationUnit objects which included the header.
        /// </summary>
        IEnumerable<LibClang.TranslationUnit> TranslationUnits { get; }

        /// <summary>
        /// Diagnostic objects located in this file.
        /// </summary>
        IEnumerable<LibClang.Diagnostic> Diagnostics { get; }
    }

    public class SourceFile : IProjectItem
    {
        private LibClang.TranslationUnit _tu;

        internal SourceFile(string path, LibClang.TranslationUnit tu)
        {
            Path = path;
            _tu = tu;
        }

        #region Properties

        public ProjectItemType Type
        {
            get { return ProjectItemType.SourceFile; }
        }

        /// <summary>
        /// File path.
        /// </summary>
        public string Path
        {
            get;
            private set;
        }
        
        /// <summary>
        /// TranslationUnit objects which referenced this file. 
        /// 
        /// For source files this will only contain the source file's TranslationUnit object. 
        /// For header files it will contain all TranslationUnit objects which included the header.
        /// </summary>
        public IEnumerable<LibClang.TranslationUnit> TranslationUnits 
        {
            get { yield return _tu; } 
        }

        /// <summary>
        /// Diagnostic objects located in this file.
        /// </summary>
        public IEnumerable<LibClang.Diagnostic> Diagnostics 
        { 
            get 
            { 
                return _tu.Diagnostics.Where(d => d.Location.File.Name == Path); 
            }
        }

        #endregion
    }

    public class HeaderFile : IProjectItem
    {
        private HashSet<LibClang.TranslationUnit> _tus;

        public HeaderFile(string path)
        {
            Path = path;
            _tus = new HashSet<LibClang.TranslationUnit>();
        }

        #region Properties

        public ProjectItemType Type
        {
            get { return ProjectItemType.HeaderFile; }
        }

        /// <summary>
        /// File path.
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// TranslationUnit objects which referenced this file. 
        /// 
        /// For source files this will only contain the source file's TranslationUnit object. 
        /// For header files it will contain all TranslationUnit objects which included the header.
        /// </summary>
        public IEnumerable<LibClang.TranslationUnit> TranslationUnits
        {
            get { return _tus; }
        }

        /// <summary>
        /// Diagnostic objects located in this file.
        /// </summary>
        public IEnumerable<LibClang.Diagnostic> Diagnostics
        {
            get
            {
                foreach (LibClang.TranslationUnit tu in _tus)
                {
                    foreach (LibClang.Diagnostic diag in tu.Diagnostics.Where(d => System.IO.Path.GetFullPath(d.Location.File.Name) == Path))
                    {
                        yield return diag;
                    }
                }   
            }
        }

        #endregion
                
        #region Methods

        public void AddTranslationUnit(LibClang.TranslationUnit tu)
        {
            _tus.Add(tu);
        }

        #endregion
    }
}
