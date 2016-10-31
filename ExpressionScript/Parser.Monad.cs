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

        public static Parser<TValue> Defer<TValue>(Func<Parser<TValue>> parserFactory)
        {
            return input => parserFactory()(input);
        }

        public static Parser<TValue> Where<TValue>(this Parser<TValue> parser, Func<TValue, bool> predicate)
        {
            return input => parser(input).Where(result => predicate(result.Value));
        }

        public static Parser<TResult> Select<TValue, TResult>(this Parser<TValue> parser, Func<TValue, TResult> selector)
        {
            return input => parser(input).Select(result => new Result<TResult>(selector(result.Value), result.Tail));
        }

        public static Parser<TResult> SelectMany<TValue, TResult>(this Parser<TValue> parser, Func<TValue, Parser<TResult>> parserSelector)
        {
            return input => parser(input).SelectMany(result => parserSelector(result.Value)(result.Tail));
        }

        public static Parser<TResult> SelectMany<TValue, TIntermediate, TResult>(
            this Parser<TValue> parser,
            Func<TValue, Parser<TIntermediate>> parserSelector,
            Func<TValue, TIntermediate, TResult> resultSelector)
        {
            return input => parser(input).SelectMany(
                   result => parserSelector(result.Value)(result.Tail).Select(
                   iresult => new Result<TResult>(
                       resultSelector(result.Value, iresult.Value),
                       iresult.Tail)));
        }
    }
}
