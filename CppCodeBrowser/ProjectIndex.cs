using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppCodeBrowser
{
    public interface IProjectIndex
    {
        /// <summary>
        /// Can be header or source file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IFileIndex GetIndexForFile(string path);
    }

    public class ProjectIndex : IProjectIndex, IDisposable
    {
        private readonly Dictionary<string, IFileIndex> _pathToIndexMap;

        public ProjectIndex()
        {
            _pathToIndexMap = new Dictionary<string, IFileIndex>();
        }

        public void Dispose()
        {
            foreach (IFileIndex fi in _pathToIndexMap.Values)
            {
                fi.Dispose();
            }
            _pathToIndexMap.Clear();
        }

        public IFileIndex GetIndexForFile(string path)
        {
            IFileIndex result = null;
            return _pathToIndexMap.TryGetValue(path, out result) ? result : null;
        }

        public void AddFile(string path, LibClang.TranslationUnit tu)
        {
            _pathToIndexMap.Add(path, new FileIndex(tu));
        }
    }
}
