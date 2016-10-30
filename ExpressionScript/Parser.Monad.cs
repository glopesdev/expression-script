using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<TValue> Return<TValue>(TValue value)
        {
            return input => EnumerableEx.Return(new Result<TValue>(value, input));
        }

        public static Parser<TValue> Empty<TValue>()
        {
            return input => Enumerable.Empty<IResult<TValue>>();
        }

        public static Parser<char> Char()
        {
            return input => input.Take(1)
                                 .Select(x => new Result<char>(x, input.Substring(1)));
        }
    }
}
