using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        #region Boolean Literal

        [TestMethod]
        public void Boolean_TrueLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("true");
            Assert.AreEqual(true, result.Value.Value);
        }

        [TestMethod]
        public void Boolean_FalseLiteral_ReturnTrueExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("false");
            Assert.AreEqual(false, result.Value.Value);
        }

        [TestMethod]
        public void Boolean_InvalidLiteral_ReturnNullExpression()
        {
            var parser = Parser.Boolean();
            var result = parser.Parse("tfrue");
            Assert.IsNull(result);
        }

        #endregion

        #region Integer Literal

        private void IntegerLiteral_ReturnExpression(string input, object value)
        {
            var parser = Parser.Integer();
            var result = parser.Parse(input);
            Assert.AreEqual(value, result.Value.Value);
        }

        [TestMethod]
        public void Integer_Int32DecimalLiteral_ReturnInt32Expression()
        {
            IntegerLiteral_ReturnExpression("42", 42);
        }

        [TestMethod]
        public void Integer_UInt32DecimalLiteral_ReturnUInt32Expression()
        {
            IntegerLiteral_ReturnExpression("42u", 42u);
        }

        [TestMethod]
        public void Integer_Int64DecimalLiteral_ReturnInt64Expression()
        {
            IntegerLiteral_ReturnExpression("42l", 42L);
        }

        [TestMethod]
        public void Integer_UInt64DecimalLiteral_ReturnUInt64Expression()
        {
            IntegerLiteral_ReturnExpression("42lu", 42UL);
        }

        [TestMethod]
        public void TestInteger_Int32HexadecimalLiteral_ReturnInt32Expression()
        {
            IntegerLiteral_ReturnExpression("0x42", 0x42);
        }

        [TestMethod]
        public void TestInteger_UInt32HexadecimalLiteral_ReturnUInt32Expression()
        {
            IntegerLiteral_ReturnExpression("0x42u", 0x42u);
        }

        [TestMethod]
        public void Integer_Int64HexadecimalLiteral_ReturnInt64Expression()
        {
            IntegerLiteral_ReturnExpression("0X42L", 0x42L);
        }

        [TestMethod]
        public void Integer_UInt64HexadecimalLiteral_ReturnUInt64Expression()
        {
            IntegerLiteral_ReturnExpression("0X42Ul", 0x42UL);
        }

        #endregion

        #region Real Literal

        private void RealLiteral_ReturnExpression(string input, object value)
        {
            var parser = Parser.Real();
            var result = parser.Parse(input);
            Assert.AreEqual(value, result.Value.Value);
        }

        [TestMethod]
        public void Real_FloatLiteral_ReturnFloatExpression()
        {
            RealLiteral_ReturnExpression("0.0f", 0.0f);
            RealLiteral_ReturnExpression("0.0e42f", 0.0e42f);
            RealLiteral_ReturnExpression(".0f", .0f);
            RealLiteral_ReturnExpression(".0e42f", .0e42f);
            RealLiteral_ReturnExpression("0f", 0f);
            RealLiteral_ReturnExpression("0e42f", 0e42f);
        }

        [TestMethod]
        public void Real_DoubleLiteral_ReturnDoubleExpression()
        {
            RealLiteral_ReturnExpression("0.0", 0.0);
            RealLiteral_ReturnExpression("0.0e42", 0.0e42);
            RealLiteral_ReturnExpression(".0", .0);
            RealLiteral_ReturnExpression(".0e42", .0e42);
            RealLiteral_ReturnExpression("0e42", 0e42);

            RealLiteral_ReturnExpression("0.0d", 0.0d);
            RealLiteral_ReturnExpression("0.0e42d", 0.0e42d);
            RealLiteral_ReturnExpression(".0d", .0d);
            RealLiteral_ReturnExpression(".0e42d", .0e42d);
            RealLiteral_ReturnExpression("0d", 0d);
            RealLiteral_ReturnExpression("0e42d", 0e42d);
        }

        [TestMethod]
        public void Real_DecimalLiteral_ReturnDoubleExpression()
        {
            RealLiteral_ReturnExpression("0.0m", 0.0m);
            RealLiteral_ReturnExpression("0.0e42m", 0.0e42m);
            RealLiteral_ReturnExpression(".0m", .0m);
            RealLiteral_ReturnExpression(".0e42m", .0e42m);
            RealLiteral_ReturnExpression("0m", 0m);
            RealLiteral_ReturnExpression("0e42m", 0e42m);
        }

        #endregion
    }
}
