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
            var result = parser.Parse(CharInput);
            Assert.AreEqual(CharInput[0], result.Value);
        }
    }
}
