using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        [TestMethod]
        public void TestBoolean_TrueLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser("true").Single();
            Assert.AreEqual(true, result.Value.Value);
        }

        [TestMethod]
        public void TestBoolean_FalseLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser("false").Single();
            Assert.AreEqual(false, result.Value.Value);
        }

        [TestMethod]
        public void TestBoolean_InvalidLiteral_ReturnNullExpression()
        {
            var parser = Parser.Boolean();
            var result = parser("tfrue").SingleOrDefault();
            Assert.IsNull(result);
        }
    }
}
