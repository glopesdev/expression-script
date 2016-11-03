using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<char> Char()
        {
            return input => input.Take(1)
                                 .Select(x => new Result<char>(x, input.Substring(1)));
        }

        public static Parser<char> Char(Func<char, bool> predicate)
        {
            return Char().Where(predicate);
        }

        public static Parser<char> Char(char value)
        {
            return Char(x => x == value);
        }

        public static Parser<char> Char(IEnumerable<char> values)
        {
            return Char(values.Contains);
        }

        public static Parser<char> Char(params char[] values)
        {
            return Char(values.Contains);
        }

        public static Parser<char> DecimalDigit()
        {
            return Char(x => '0' <= x && x <= '9');
        }

        public static Parser<string> DecimalDigits()
        {
            return DecimalDigit().Many(1);
        }

        public static Parser<char> HexDigit()
        {
            return Char(x => '0' <= x && x <= '9' ||
                             'A' <= x && x <= 'F' ||
                             'a' <= x && x <= 'f');
        }

        public static Parser<string> HexDigits()
        {
            return HexDigit().Many(1);
        }

        public static Parser<string> String(string value)
        {
            if (value == string.Empty) return Return(string.Empty);
            var x = value[0];
            var xs = value.Substring(1);
            return from rc in Char(x)
                   from rs in String(xs)
                   select rc + rs;
        }

        public static Parser<string> String(IEnumerable<string> values)
        {
            return Or(values.Select(String));
        }

        public static Parser<string> String(params string[] values)
        {
            return Or(values.Select(String));
        }
    }
}
