using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppCodeBrowser
{
    public interface IIndexBuilder : IDisposable
    {
        void IndexFile(string path, string[] compilerArgs);
        IProjectIndex Index { get; }
    }

    /// <summary>
    /// This is a simple synchronous implementation.
    /// </summary>
    public class IndexBuilder : IIndexBuilder
    {
        private readonly ProjectIndex _index;
        private readonly LibClang.Index _libClangIndex;

        public IndexBuilder()
        {
            _index = new ProjectIndex();
            _libClangIndex = new LibClang.Index(false, true);
        }

        public void Dispose()
        {
            _index.Dispose();
        }

        public void IndexFile(string path, string[] compilerArgs)
        {
            LibClang.TranslationUnit tu = new LibClang.TranslationUnit(_libClangIndex, path);

            if (tu.Parse(compilerArgs) == false)
            {
                tu.Dispose();
                return;
            }
            _index.AddFile(path, tu);

            foreach (string header in tu.HeaderFiles)
            {

            }
        }

        public IProjectIndex Index { get { return _index; } }
    }
}
