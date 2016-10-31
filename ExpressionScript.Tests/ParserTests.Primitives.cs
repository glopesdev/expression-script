using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        const string CharInput = "abFb67";

        [TestMethod]
        public void TestChar()
        {
            var parser = Parser.Char();
            var results = parser(CharInput).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(CharInput[0], results[0].Value);
        }
    }
}
