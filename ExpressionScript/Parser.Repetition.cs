using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<TValue> Concat<TValue>(this Parser<TValue> first, Parser<TValue> second)
        {
            return input => first(input).Concat(EnumerableEx.Defer(() => second(input)));
        }

        public static Parser<TValue> Concat<TValue>(this IEnumerable<Parser<TValue>> parsers)
        {
            return input => parsers.SelectMany(parser => parser(input));
        }

        public static Parser<TValue> Concat<TValue>(params Parser<TValue>[] parsers)
        {
            return Concat((IEnumerable<Parser<TValue>>)parsers);
        }

        public static Parser<TValue> First<TValue>(this Parser<TValue> parser)
        {
            return input => parser(input).Take(1);
        }

        public static Parser<TValue> Or<TValue>(this Parser<TValue> first, Parser<TValue> second)
        {
            return First(first.Concat(second));
        }

        public static Parser<TValue> Or<TValue>(this IEnumerable<Parser<TValue>> parsers)
        {
            return First(parsers.Concat());
        }

        public static Parser<TValue> Or<TValue>(params Parser<TValue>[] parsers)
        {
            return Or((IEnumerable<Parser<TValue>>)parsers);
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser)
        {
            return (from x in parser
                    from xs in Many(parser)
                    select EnumerableEx.Concat(x, xs))
                    .Or(Return(Enumerable.Empty<TValue>()));
        }

        public static Parser<string> Many(this Parser<char> parser)
        {
            return (from x in parser
                    from xs in Many(parser)
                    select x + xs)
                    .Or(Return(string.Empty));
        }

        public static Parser<IEnumerable<TValue>> AtLeastOnce<TValue>(this Parser<TValue> parser)
        {
            return from x in parser
                   from xs in Many(parser)
                   select EnumerableEx.Concat(x, xs);
        }

        public static Parser<string> AtLeastOnce(this Parser<char> parser)
        {
            return from x in parser
                   from xs in Many(parser)
                   select x + xs;
        }

        public static Parser<IEnumerable<TValue>> AtLeastOnceSeparatedBy<TValue, TSeparator>(this Parser<TValue> parser, Parser<TSeparator> separator)
        {
            return from x in parser
                   from xs in
                       Many(from s in separator
                            from y in parser
                            select y)
                   select EnumerableEx.Concat(x, xs);
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(this Parser<TValue> parser, Parser<TSeparator> separator)
        {
            return parser.AtLeastOnceSeparatedBy(separator).Concat(Return(Enumerable.Empty<TValue>()));
        }

        public static Parser<TValue> BracketedBy<TValue, TOpen, TClose>(this Parser<TValue> parser, Parser<TOpen> open, Parser<TClose> close)
        {
            return from o in open
                   from x in parser
                   from c in close
                   select x;
        }

        public static Parser<TValue> ChainLeft<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func)
        {
            var rest = default(Func<TValue, Parser<TValue>>);
            rest = x => func.SelectMany(f =>
                        parser.SelectMany(y => rest(f(x, y))))
                        .Or(Return(x));
            return parser.SelectMany(rest);
        }

        public static Parser<TValue> ChainLeft<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func, TValue defaultValue)
        {
            return parser.ChainLeft(func).Or(Return(defaultValue));
        }

        public static Parser<TValue> ChainRight<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func)
        {
            return parser.SelectMany(x =>
                   (from f in func
                    from y in parser.ChainRight(func)
                    select f(x, y))
                    .Or(Return(x)));
        }

        public static Parser<TValue> ChainRight<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func, TValue defaultValue)
        {
            return parser.ChainRight(func).Or(Return(defaultValue));
        }
    }
}
