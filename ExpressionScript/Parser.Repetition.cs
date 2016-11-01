using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<TValue> Except<TValue>(this Parser<TValue> first, Parser<TValue> second)
        {
            return input => first(input).Except(second(input), ResultValueComparer<TValue>.Default);
        }

        public static Parser<TValue> Except<TValue>(this Parser<TValue> first, Parser<TValue> second, IEqualityComparer<TValue> comparer)
        {
            var valueComparer = new ResultValueComparer<TValue>(comparer);
            return input => first(input).Except(second(input), valueComparer);
        }

        public static Parser<TValue> Except<TValue>(this Parser<TValue> first, IEnumerable<TValue> values)
        {
            return first.Except(values.AsParser());
        }

        public static Parser<TValue> Except<TValue>(this Parser<TValue> first, IEnumerable<TValue> values, IEqualityComparer<TValue> comparer)
        {
            return first.Except(values.AsParser(), comparer);
        }

        public static Parser<TValue> Except<TValue>(this Parser<TValue> first, params TValue[] values)
        {
            return first.Except(values.AsParser());
        }

        public static Parser<TValue> Optional<TValue>(this Parser<TValue> parser)
        {
            return parser.Or(Return(default(TValue)));
        }

        public static Parser<TValue> Optional<TValue>(this Parser<TValue> parser, TValue defaultValue)
        {
            return parser.Or(Return(defaultValue));
        }

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

        public static Parser<TAccumulate> Many<TValue, TAccumulate>(
            this Parser<TValue> parser,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return (from x in parser
                    from xs in Many(parser, seed, accumulator)
                    select accumulator(xs, x))
                    .Or(Return(seed));
        }

        public static Parser<TValue> Many<TValue>(this Parser<TValue> parser, Func<TValue, TValue, TValue> accumulator)
        {
            return Many(parser, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser)
        {
            return Many(parser, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> Many(this Parser<char> parser)
        {
            return Many(parser, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<TAccumulate> AtLeastOnce<TValue, TAccumulate>(
            this Parser<TValue> parser,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return from x in parser
                   from xs in Many(parser, seed, accumulator)
                   select accumulator(xs, x);
        }

        public static Parser<TValue> AtLeastOnce<TValue>(this Parser<TValue> parser, Func<TValue, TValue, TValue> accumulator)
        {
            return AtLeastOnce(parser, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> AtLeastOnce<TValue>(this Parser<TValue> parser)
        {
            return AtLeastOnce(parser, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> AtLeastOnce(this Parser<char> parser)
        {
            return AtLeastOnce(parser, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<TAccumulate> AtLeastOnceSeparatedBy<TValue, TSeparator, TAccumulate>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return from x in parser
                   from xs in
                       Many(from s in separator
                            from y in parser
                            select y, seed, accumulator)
                   select accumulator(xs, x);
        }

        public static Parser<TValue> AtLeastOnceSeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            Func<TValue, TValue, TValue> accumulator)
        {
            return AtLeastOnceSeparatedBy(parser, separator, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> AtLeastOnceSeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator)
        {
            return AtLeastOnceSeparatedBy(
                parser, separator,
                Enumerable.Empty<TValue>(),
                (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> AtLeastOnceSeparatedBy<TSeparator>(
            this Parser<char> parser,
            Parser<TSeparator> separator)
        {
            return AtLeastOnceSeparatedBy(parser, separator, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<TAccumulate> ManySeparatedBy<TValue, TSeparator, TAccumulate>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return parser.AtLeastOnceSeparatedBy(separator, seed, accumulator).Concat(Return(seed));
        }

        public static Parser<TValue> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManySeparatedBy(parser, separator, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator)
        {
            return ManySeparatedBy(
                parser, separator,
                Enumerable.Empty<TValue>(),
                (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> ManySeparatedBy<TSeparator>(
            this Parser<char> parser,
            Parser<TSeparator> separator)
        {
            return ManySeparatedBy(parser, separator, string.Empty, (xs, x) => x + xs);
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
