using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        const string ManyInput = "1234567";
        const string ManySeparatedByInput = "1,2,3,4,5,6,7";

        [TestMethod]
        public void Many_ZeroOrMoreChars_ReturnsString()
        {
            var parser = Parser.Char().Many();
            var result = parser.Parse(ManyInput);
            Assert.AreEqual(ManyInput.Length, result.Value.Length);
        }

        [TestMethod]
        public void Many_AtLeastOneChar_ReturnsString()
        {
            var parser = Parser.Char().Many(1);
            var result = parser.Parse(ManyInput);
            Assert.AreEqual(ManyInput.Length, result.Value.Length);
        }

        [TestMethod]
        public void Many_AtLeastOneCharEmptyString_ReturnsFailure()
        {
            var parser = Parser.Char().Many(1);
            var result = parser.Parse(string.Empty);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void Many_AtMostOne_ReturnsSingletonString()
        {
            var parser = Parser.Char().Many(0, 1);
            var result = parser.Parse(ManyInput);
            Assert.AreEqual(1, result.Value.Length);
            Assert.AreEqual(ManyInput.Length - 1, result.Tail.Length);
        }

        [TestMethod]
        public void Many_ZeroOrMoreDigitAccumulator_ReturnsTally()
        {
            var parser = Parser.Char().Many(0, (xs, x) => xs + int.Parse(new string(x, 1)));
            var result = parser.Parse(ManyInput);
            Assert.AreEqual(ManyInput.Length * (ManyInput.Length + 1) / 2, result.Value);
        }

        [TestMethod]
        public void Many_ZeroOrMoreDigitAccumulatorEmptyString_ReturnsTally()
        {
            var max = 1;
            var parser = Parser.Char().Many(
                min: 0, max: max,
                seed: 0, accumulator: (xs, x) => xs + int.Parse(new string(x, 1)));
            var result = parser.Parse(ManyInput);
            Assert.AreEqual(max * (max + 1) / 2, result.Value);
        }

        [TestMethod]
        public void ManySeparatedBy_CharList_ReturnsString()
        {
            var parser = Parser.Char().ManySeparatedBy(Parser.Char(','));
            var result = parser.Parse(ManySeparatedByInput);
            Assert.AreEqual(ManySeparatedByInput.Split(',').Length, result.Value.Length);
        }

        [TestMethod]
        public void ManySeparatedBy_DigitAccumulator_ReturnsTally()
        {
            var ndigits = ManySeparatedByInput.Split(',').Length;
            var parser = Parser.DecimalDigits().Select(x => int.Parse(x))
                                               .ManySeparatedBy(Parser.Char(','), (xs, x) => xs + x);
            var result = parser.Parse(ManySeparatedByInput);
            Assert.AreEqual(ndigits * (ndigits + 1) / 2, result.Value);
        }
    }
}
