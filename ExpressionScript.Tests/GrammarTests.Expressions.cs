using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace ExpressionScript.Tests
{
    public partial class ParserTests
    {
        object Evaluate(Expression expression)
        {
            var lambda = Expression.Lambda(expression).Compile();
            return lambda.DynamicInvoke();
        }

        [TestMethod]
        public void AdditiveExpression_IntegerAddition_ReturnsSum()
        {
            var parser = Parser.AdditiveExpression();
            var result = parser.Parse("1+1");
            Assert.AreEqual(1 + 1, Evaluate(result.Value));
        }

        [TestMethod]
        public void MultiplicativeExpression_IntegerMultiplication_ReturnsProduct()
        {
            var parser = Parser.MultiplicativeExpression();
            var result = parser.Parse("1*1");
            Assert.AreEqual(1 * 1, Evaluate(result.Value));
        }
    }
}
