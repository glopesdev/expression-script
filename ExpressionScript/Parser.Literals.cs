using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<ConstantExpression> Boolean()
        {
            return String("true").Select(x => Expression.Constant(true)).Or(
                   String("false").Select(x => Expression.Constant(false)));
        }

        public static Parser<ConstantExpression> Integer()
        {
            return HexadecimalInteger().Or(DecimalInteger());
        }

        public static Parser<ConstantExpression> DecimalInteger()
        {
            return IntegerLiteral(DecimalDigits(), NumberStyles.None);
        }

        public static Parser<ConstantExpression> HexadecimalInteger()
        {
            return IntegerLiteral(from prefix in String("0x", "0X")
                                  from digits in HexDigits()
                                  select digits, NumberStyles.AllowHexSpecifier);
        }

        static Parser<ConstantExpression> IntegerLiteral(Parser<string> digits, NumberStyles style)
        {
            return digits.SelectMany(
                x => IntegerTypeSuffix(),
                (x, type) =>
                {
                    switch (type)
                    {
                        case TypeCode.Int32: return Expression.Constant(int.Parse(x, style));
                        case TypeCode.Int64: return Expression.Constant(long.Parse(x, style));
                        case TypeCode.UInt32: return Expression.Constant(uint.Parse(x, style));
                        case TypeCode.UInt64: return Expression.Constant(ulong.Parse(x, style));
                        default: throw new InvalidOperationException("Invalid type suffix implied from parser.");
                    }
                });
        }
    }
}
