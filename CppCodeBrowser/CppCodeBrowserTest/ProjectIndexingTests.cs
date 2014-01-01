using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CppCodeBrowser;

namespace CppCodeBrowserTest
{
    [TestClass]
    public class ProjectIndexingTests
    {
        [TestMethod]
        public void IndexProject()
        {
            IIndexBuilder ib = new IndexBuilder();
            Project proj = new Project("test", ib);

            proj.AddSourceFile("TestCode\\main.cpp", null);
            proj.AddSourceFile("TestCode\\class_a.cpp", null);

            IFileIndex index = proj.Index.GetIndexForFile("TestCode\\class_a.cpp");
        }
    }
}
