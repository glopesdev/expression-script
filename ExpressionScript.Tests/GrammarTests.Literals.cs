using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        #region Boolean Literal

        [TestMethod]
        public void TestBoolean_TrueLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("true");
            Assert.AreEqual(true, result.Value.Value);
        }

        [TestMethod]
        public void TestBoolean_FalseLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("false");
            Assert.AreEqual(false, result.Value.Value);
        }

        [TestMethod]
        public void TestBoolean_InvalidLiteral_ReturnNullExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("tfrue");
            Assert.IsNull(result);
        }

        #endregion

        #region Integer Literal

        private void TestIntegerLiteral_ReturnExpression(string input, object value)
        {
            var parser = Parser.Integer();
            var result = parser.Parse(input);
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

        #endregion

        #region Real Literal

        private void TestRealLiteral_ReturnExpression(string input, object value)
        {
            var parser = Parser.Real();
            var result = parser.Parse(input);
            Assert.AreEqual(value, result.Value.Value);
        }

        [TestMethod]
        public void TestReal_FloatLiteral_ReturnFloatExpression()
        {
            TestRealLiteral_ReturnExpression("0.0f", 0.0f);
            TestRealLiteral_ReturnExpression("0.0e42f", 0.0e42f);
            TestRealLiteral_ReturnExpression(".0f", .0f);
            TestRealLiteral_ReturnExpression(".0e42f", .0e42f);
            TestRealLiteral_ReturnExpression("0f", 0f);
            TestRealLiteral_ReturnExpression("0e42f", 0e42f);
        }

        [TestMethod]
        public void TestReal_DoubleLiteral_ReturnDoubleExpression()
        {
            TestRealLiteral_ReturnExpression("0.0", 0.0);
            TestRealLiteral_ReturnExpression("0.0e42", 0.0e42);
            TestRealLiteral_ReturnExpression(".0", .0);
            TestRealLiteral_ReturnExpression(".0e42", .0e42);
            TestRealLiteral_ReturnExpression("0e42", 0e42);

            TestRealLiteral_ReturnExpression("0.0d", 0.0d);
            TestRealLiteral_ReturnExpression("0.0e42d", 0.0e42d);
            TestRealLiteral_ReturnExpression(".0d", .0d);
            TestRealLiteral_ReturnExpression(".0e42d", .0e42d);
            TestRealLiteral_ReturnExpression("0d", 0d);
            TestRealLiteral_ReturnExpression("0e42d", 0e42d);
        }

        [TestMethod]
        public void TestReal_DecimalLiteral_ReturnDoubleExpression()
        {
            TestRealLiteral_ReturnExpression("0.0m", 0.0m);
            TestRealLiteral_ReturnExpression("0.0e42m", 0.0e42m);
            TestRealLiteral_ReturnExpression(".0m", .0m);
            TestRealLiteral_ReturnExpression(".0e42m", .0e42m);
            TestRealLiteral_ReturnExpression("0m", 0m);
            TestRealLiteral_ReturnExpression("0e42m", 0e42m);
        }

        #endregion
    }
}
