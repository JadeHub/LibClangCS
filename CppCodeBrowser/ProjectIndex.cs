using System;
using System.Linq;
using System.Collections.Generic;
using LibClang;

namespace CppCodeBrowser
{
    public interface IProjectIndex
    {
        /// <summary>
        /// Can be header or source file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        ISourceFileIndex GetIndexForSourceFile(string path);
        IEnumerable<ISourceFileIndex> GetIndexesForHeaderFile(string path);

        bool IsSourceFile(string path);
        bool IsHeaderFile(string path);
    }

    public class ProjectIndex : IProjectIndex, IDisposable
    {
        /// <summary>
        /// Source file path mapped to IFileIndex.
        /// </summary>
        private readonly Dictionary<string, ISourceFileIndex> _pathToIndexMap;

        /// <summary>
        /// Header file path mapped to set of IFileIndexes from which it was included.
        /// </summary>
        private readonly Dictionary<string, HashSet<ISourceFileIndex>> _headerToSourceMap;
     

        public ProjectIndex()
        {
            _pathToIndexMap = new Dictionary<string, ISourceFileIndex>();
            _headerToSourceMap = new Dictionary<string, HashSet<ISourceFileIndex>>();
        }

        public void Dispose()
        {
            foreach (ISourceFileIndex fi in _pathToIndexMap.Values)
            {
                fi.Dispose();
            }
            _pathToIndexMap.Clear();
        }

        public ISourceFileIndex GetIndexForSourceFile(string path)
        {
            ISourceFileIndex result;
            return _pathToIndexMap.TryGetValue(path, out result) ? result : null;
        }
                        
        public IEnumerable<ISourceFileIndex> GetIndexesForHeaderFile(string path)
        {            
            HashSet<ISourceFileIndex> result;
            return _headerToSourceMap.TryGetValue(System.IO.Path.GetFullPath(path), out result) ? 
                result : Enumerable.Empty<ISourceFileIndex>();
        }
        
        public void AddFile(string path, LibClang.TranslationUnit tu)
        {
            ISourceFileIndex index = new SourceFileIndex(tu);
            _pathToIndexMap.Add(path, index);

            foreach (TranslationUnit.HeaderInfo header in tu.HeaderFiles)
            {
                RecordHeader(index, header);
            }
        }

        public bool IsHeaderFile(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return _headerToSourceMap.ContainsKey(fullPath);
        }

        public bool IsSourceFile(string path)
        {
            return _pathToIndexMap.ContainsKey(path);
        }

        private void RecordHeader(ISourceFileIndex sourceIndex, TranslationUnit.HeaderInfo header)
        {
            string path = System.IO.Path.GetFullPath(header.Path);

            HashSet<ISourceFileIndex> sources;
            if (_headerToSourceMap.TryGetValue(path, out sources) == false)
            {
                sources = new HashSet<ISourceFileIndex>();
                _headerToSourceMap.Add(path, sources);
            }
            sources.Add(sourceIndex);
        }
    }
}
