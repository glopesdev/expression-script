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

        public static Parser<char> Char(char c)
        {
            return Char(x => x == c);
        }

        public static Parser<char> DecimalDigit()
        {
            return Char(x => '0' <= x && x <= '9');
        }
    }
}
