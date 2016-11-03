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

        #region ManyWhile

        public static Parser<TAccumulate> ManyWhile<TValue, TAccumulate>(
            this Parser<TValue> parser,
            Func<TValue, int, bool> predicate,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return ManyWhileIndexed(parser, predicate, seed, accumulator, 0);
        }

        static Parser<TAccumulate> ManyWhileIndexed<TValue, TAccumulate>(
            Parser<TValue> parser,
            Func<TValue, int, bool> predicate,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator,
            int index)
        {
            return (from x in parser
                    where predicate(x, index)
                    from xs in ManyWhileIndexed(parser, predicate, seed, accumulator, index + 1)
                    select accumulator(xs, x))
                    .Or(Return(seed));
        }

        public static Parser<TValue> ManyWhile<TValue>(
            this Parser<TValue> parser,
            Func<TValue, int, bool> predicate,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManyWhile(parser, predicate, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> ManyWhile<TValue>(
            this Parser<TValue> parser,
            Func<TValue, int, bool> predicate)
        {
            return ManyWhile(parser, predicate, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> ManyWhile(this Parser<char> parser, Func<char, int, bool> predicate)
        {
            return ManyWhile(parser, predicate, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<TAccumulate> ManyWhile<TValue, TAccumulate>(
            this Parser<TValue> parser,
            Func<TValue, bool> predicate,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return Many(parser.Where(predicate), seed, accumulator);
        }

        public static Parser<TValue> ManyWhile<TValue>(
            this Parser<TValue> parser,
            Func<TValue, bool> predicate,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManyWhile(parser, predicate, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> ManyWhile<TValue>(this Parser<TValue> parser, Func<TValue, bool> predicate)
        {
            return ManyWhile(parser, predicate, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> ManyWhile(this Parser<char> parser, Func<char, bool> predicate)
        {
            return ManyWhile(parser, predicate, string.Empty, (xs, x) => x + xs);
        }

        #endregion

        #region Many

        static Parser<TAccumulate> ManyIndexed<TValue, TAccumulate>(
            Parser<TValue> parser,
            int min, int? max,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator,
            int index)
        {
            if (index >= max) return Return(seed);
            var result = from x in parser
                         from xs in ManyIndexed(parser, min, max, seed, accumulator, index + 1)
                         select accumulator(xs, x);
            if (index >= min) return result.Or(Return(seed));
            return result;
        }

        public static Parser<TAccumulate> Many<TValue, TAccumulate>(
            this Parser<TValue> parser,
            int min, int? max,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return ManyIndexed(parser, min, max, seed, accumulator, 0);
        }

        public static Parser<TAccumulate> Many<TValue, TAccumulate>(
            this Parser<TValue> parser,
            int min,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return Many(parser, min, null, seed, accumulator);
        }

        public static Parser<TAccumulate> Many<TValue, TAccumulate>(
            this Parser<TValue> parser,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return Many(parser, 0, null, seed, accumulator);
        }

        public static Parser<TValue> Many<TValue>(
            this Parser<TValue> parser,
            int min, int? max,
            Func<TValue, TValue, TValue> accumulator)
        {
            return Many(parser, min, max, default(TValue), accumulator);
        }

        public static Parser<TValue> Many<TValue>(
            this Parser<TValue> parser,
            int min,
            Func<TValue, TValue, TValue> accumulator)
        {
            return Many(parser, min, null, default(TValue), accumulator);
        }

        public static Parser<TValue> Many<TValue>(
            this Parser<TValue> parser,
            Func<TValue, TValue, TValue> accumulator)
        {
            return Many(parser, 0, null, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser, int min, int? max)
        {
            return Many(parser, min, max, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser, int min)
        {
            return Many(parser, min, null, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser)
        {
            return Many(parser, 0, null, Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> Many(this Parser<char> parser, int min, int? max)
        {
            return Many(parser, min, max, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<string> Many(this Parser<char> parser, int min)
        {
            return Many(parser, min, null, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<string> Many(this Parser<char> parser)
        {
            return Many(parser, 0, null, string.Empty, (xs, x) => x + xs);
        }

        #endregion

        #region ManySeparatedBy

        public static Parser<TAccumulate> ManySeparatedBy<TValue, TSeparator, TAccumulate>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min, int? max,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            if (max < 1) return Empty<TAccumulate>();
            var result = from x in parser
                         from xs in
                             ManyIndexed(from s in separator
                                         from y in parser
                                         select y,
                                         min, max,
                                         seed, accumulator, 1)
                         select accumulator(xs, x);
            if (min < 1) return result.Or(Return(seed));
            return result;
        }

        public static Parser<TAccumulate> ManySeparatedBy<TValue, TSeparator, TAccumulate>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return ManySeparatedBy(parser, separator, min, null, seed, accumulator);
        }

        public static Parser<TAccumulate> ManySeparatedBy<TValue, TSeparator, TAccumulate>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            TAccumulate seed,
            Func<TAccumulate, TValue, TAccumulate> accumulator)
        {
            return ManySeparatedBy(parser, separator, 0, null, seed, accumulator);
        }

        public static Parser<TValue> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min, int? max,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManySeparatedBy(parser, separator, min, max, default(TValue), accumulator);
        }

        public static Parser<TValue> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManySeparatedBy(parser, separator, min, null, default(TValue), accumulator);
        }

        public static Parser<TValue> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            Func<TValue, TValue, TValue> accumulator)
        {
            return ManySeparatedBy(parser, separator, 0, null, default(TValue), accumulator);
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min,
            int? max)
        {
            return ManySeparatedBy(
                parser, separator, min, max,
                Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator,
            int min)
        {
            return ManySeparatedBy(
                parser, separator, min, null,
                Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(
            this Parser<TValue> parser,
            Parser<TSeparator> separator)
        {
            return ManySeparatedBy(
                parser, separator, 0, null,
                Enumerable.Empty<TValue>(), (xs, x) => EnumerableEx.Concat(x, xs));
        }

        public static Parser<string> ManySeparatedBy<TSeparator>(
            this Parser<char> parser,
            Parser<TSeparator> separator,
            int min,
            int? max)
        {
            return ManySeparatedBy(parser, separator, min, max, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<string> ManySeparatedBy<TSeparator>(
            this Parser<char> parser,
            Parser<TSeparator> separator,
            int min)
        {
            return ManySeparatedBy(parser, separator, min, null, string.Empty, (xs, x) => x + xs);
        }

        public static Parser<string> ManySeparatedBy<TSeparator>(
            this Parser<char> parser,
            Parser<TSeparator> separator)
        {
            return ManySeparatedBy(parser, separator, 0, null, string.Empty, (xs, x) => x + xs);
        }

        #endregion

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
