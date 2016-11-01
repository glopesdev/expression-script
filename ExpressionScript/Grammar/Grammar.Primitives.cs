using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<TypeCode> IntegerTypeSuffix()
        {
            return Or(
                Char('U', 'u').SelectMany(s1 => Or(
                    Char('L', 'l').Select(s2 => TypeCode.UInt64),
                    Return(TypeCode.UInt32))),
                Char('L', 'l').SelectMany(s1 => Or(
                    Char('U', 'u').Select(s2 => TypeCode.UInt64),
                    Return(TypeCode.Int64))),
                Return(TypeCode.Int32));
        }

        public static Parser<string> Exponent()
        {
            return from e in Char('e', 'E')
                   from sign in ExponentSign()
                   from x in DecimalDigits()
                   select e + sign + x;
        }

        public static Parser<string> ExponentSign()
        {
            return Or(
                Char('+').Select(x => "+"),
                Char('-').Select(x => "-"),
                Return(string.Empty));
        }

        public static Parser<TypeCode> RealTypeSuffix()
        {
            return Or(
                Char('F', 'f').Select(s => TypeCode.Single),
                Char('D', 'd').Select(s => TypeCode.Double),
                Char('M', 'm').Select(s => TypeCode.Decimal));
        }

        public static Parser<char> SingleCharacter()
        {
            return Char().Except(Char(
                '\u0027', // apostrophe
                '\u005C') // backslash
                .Concat(NewLineCharacter()));
        }

        public static Parser<char> NewLineCharacter()
        {
            return Char(
                '\u000D',  // carriage return
                '\u000A',  // line feed
                '\u0085',  // next line
                '\u2028',  // line separator
                '\u2029'); // paragraph separator
        }
    }
}
