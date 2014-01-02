using System;
using System.Collections.Generic;
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

            ICodeBrowser b = new JumpToBrowser(proj.Index);

            List<ICodeLocation> results = new List<ICodeLocation>();
            results.AddRange(b.BrowseFrom(new CodeLocation("TestCode\\class_a.h", 67)));

            /*ICodeLocation loc = b.JumpTo(new CodeLocation("TestCode\\class_a.h", 67));
            Assert.IsNotNull(loc);*/
        }
    }
}
