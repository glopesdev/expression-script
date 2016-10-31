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

        private void TestIntegerLiteral_ReturnExpression(string input, object value)
        {
            var parser = Parser.Integer();
            var result = parser(input).Single();
            Assert.AreEqual(value, result.Value.Value);
        }

        [TestMethod]
        public void TestInteger_Int32DecimalLiteral_ReturnInt32Expression()
        {
            TestIntegerLiteral_ReturnExpression("42", 42);
        }

        [TestMethod]
        public void TestInteger_UInt32DecimalLiteral_ReturnUInt32Expression()
        {
            TestIntegerLiteral_ReturnExpression("42u", 42u);
        }

        [TestMethod]
        public void TestInteger_Int64DecimalLiteral_ReturnInt64Expression()
        {
            TestIntegerLiteral_ReturnExpression("42l", 42L);
        }

        [TestMethod]
        public void TestInteger_UInt64DecimalLiteral_ReturnUInt64Expression()
        {
            TestIntegerLiteral_ReturnExpression("42lu", 42UL);
        }

        [TestMethod]
        public void TestInteger_Int32HexadecimalLiteral_ReturnInt32Expression()
        {
            TestIntegerLiteral_ReturnExpression("0x42", 0x42);
        }

        [TestMethod]
        public void TestInteger_UInt32HexadecimalLiteral_ReturnUInt32Expression()
        {
            TestIntegerLiteral_ReturnExpression("0x42u", 0x42u);
        }

        [TestMethod]
        public void TestInteger_Int64HexadecimalLiteral_ReturnInt64Expression()
        {
            TestIntegerLiteral_ReturnExpression("0X42L", 0x42L);
        }

        [TestMethod]
        public void TestInteger_UInt64HexadecimalLiteral_ReturnUInt64Expression()
        {
            TestIntegerLiteral_ReturnExpression("0X42Ul", 0x42UL);
        }
    }
}
