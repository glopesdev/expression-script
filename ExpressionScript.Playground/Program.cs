using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TInput = System.String;

namespace ExpressionScript.Playground
{
    public interface IResult<out TValue>
    {
        TValue Value { get; }

        TInput Tail { get; }
    }

    public class Result<TValue> : IResult<TValue>
    {
        public Result(TValue value, TInput tail)
        {
            Value = value;
            Tail = tail;
        }

        public TValue Value { get; private set; }

        public TInput Tail { get; private set; }
    }

    public delegate IEnumerable<IResult<TValue>> Parser<out TValue>(TInput input);

    static class Parser
    {
        static IEnumerable<TValue> Return<TValue>(TValue value)
        {
            yield return value;
        }

        public static Parser<TValue> Result<TValue>(TValue value)
        {
            return input => Return(new Result<TValue>(value, input));
        }

        public static Parser<TValue> Empty<TValue>()
        {
            return input => Enumerable.Empty<IResult<TValue>>();
        }

        public static Parser<TValue> Defer<TValue>(Func<Parser<TValue>> parserFactory)
        {
            return input => parserFactory()(input);
        }

        public static Parser<char> Char()
        {
            return input => input.Take(1).Select(x => new Result<char>(x, input.Substring(1)));
        }

        public static Parser<TValue> First<TValue>(this Parser<TValue> parser)
        {
            return input => parser(input).Take(1);
        }

        public static Parser<TValue> Where<TValue>(this Parser<TValue> parser, Func<TValue, bool> predicate)
        {
            return input => parser(input).Where(x => predicate(x.Value));
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
                   iresult => new Result<TResult>(resultSelector(result.Value, iresult.Value),
                                                  iresult.Tail)));
        }

        public static Parser<char> TakeWhere(Func<char, bool> predicate)
        {
            return Char().SelectMany(x => predicate(x) ? Result(x) : Empty<char>());
        }

        public static Parser<TValue> Plus<TValue>(this Parser<TValue> first, Parser<TValue> second)
        {
            return input => first(input).Concat(second(input));
        }

        public static Parser<TValue> Plus<TValue>(this IEnumerable<Parser<TValue>> parsers)
        {
            return input => parsers.SelectMany(parser => parser(input));
        }

        public static Parser<TValue> Plus<TValue>(params Parser<TValue>[] parsers)
        {
            return Plus((IEnumerable<Parser<TValue>>)parsers);
        }

        public static Parser<TValue> Or<TValue>(this Parser<TValue> first, Parser<TValue> second)
        {
            return First(first.Plus(second));
        }

        public static Parser<TValue> Or<TValue>(this IEnumerable<Parser<TValue>> parsers)
        {
            return First(parsers.Plus());
        }

        public static Parser<TValue> Or<TValue>(params Parser<TValue>[] parsers)
        {
            return Or((IEnumerable<Parser<TValue>>)parsers);
        }

        public static Parser<IEnumerable<TValue>> Many<TValue>(this Parser<TValue> parser)
        {
            return (from x in parser
                    from xs in Many(parser)
                    select Return(x).Concat(xs))
                    .Or(Result(Enumerable.Empty<TValue>()));
        }

        public static Parser<string> Many(this Parser<char> parser)
        {
            return (from x in parser
                    from xs in Many(parser)
                    select x + xs)
                    .Or(Result(string.Empty));
        }

        public static Parser<IEnumerable<TValue>> AtLeastOnce<TValue>(this Parser<TValue> parser)
        {
            return from x in parser
                   from xs in Many(parser)
                   select Return(x).Concat(xs);
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
                   select Return(x).Concat(xs);
        }

        public static Parser<IEnumerable<TValue>> ManySeparatedBy<TValue, TSeparator>(this Parser<TValue> parser, Parser<TSeparator> separator)
        {
            return parser.AtLeastOnceSeparatedBy(separator).Or(Result(Enumerable.Empty<TValue>()));
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
                        .Or(Result(x));
            return parser.SelectMany(rest);

            //return from x in parser
            //       from fys in
            //           Many(from f in func
            //                from y in parser
            //                select new { f, y })
            //       select fys.Aggregate(x, (a, fy) => fy.f(a, fy.y));
        }

        public static Parser<TValue> ChainLeft<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func, TValue defaultValue)
        {
            return parser.ChainLeft(func).Or(Result(defaultValue));
        }

        public static Parser<TValue> ChainRight<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func)
        {
            return parser.SelectMany(x =>
                   (from f in func
                    from y in parser.ChainRight(func)
                    select f(x, y))
                    .Or(Result(x)));
        }

        public static Parser<TValue> ChainRight<TValue>(this Parser<TValue> parser, Parser<Func<TValue, TValue, TValue>> func, TValue defaultValue)
        {
            return parser.ChainRight(func).Or(Result(defaultValue));
        }

        public static Parser<string> WhiteSpace()
        {
            return Many(TakeWhere(x => char.IsWhiteSpace(x))).Select(x => string.Empty);
        }

        public static Parser<string> Comment(string prefix)
        {
            return from x in String(prefix)
                   from s in Many(TakeWhere(c => c != '\n'))
                   select string.Empty;
        }

        public static Parser<char> Char(char c)
        {
            return TakeWhere(x => x == c);
        }

        public static Parser<char> Digit()
        {
            return TakeWhere(x => '0' <= x && x <= '9');
        }

        public static Parser<char> Lowercase()
        {
            return TakeWhere(x => 'a' <= x && x <= 'z');
        }

        public static Parser<char> Uppercase()
        {
            return TakeWhere(x => 'A' <= x && x <= 'Z');
        }

        public static Parser<char> Letter()
        {
            return Lowercase().Or(Uppercase());
        }

        public static Parser<char> Alphanumeric()
        {
            return Letter().Or(Digit());
        }

        public static Parser<string> Word()
        {
            return Letter().Many();
        }

        public static Parser<string> String(string s)
        {
            if (s == string.Empty) return Result(string.Empty);
            var x = s[0];
            var xs = s.Substring(1);
            return from rc in Char(x)
                   from rs in String(xs)
                   select rc + rs;
        }

        public static Parser<int> Natural()
        {
            return from xs in Digit().AtLeastOnce()
                   select int.Parse(xs);
        }

        public static Parser<int> Int()
        {
            return from c in String("-").Or(String(string.Empty))
                   from n in Digit().AtLeastOnce()
                   select int.Parse(c + n);
        }

        public static Parser<Expression> Constant()
        {
            return from x in Int()
                   select Expression.Constant(x);
        }

        #region Literals

        static Parser<ConstantExpression> Boolean()
        {
            return String("true").Select(x => Expression.Constant(true)).Or(
                   String("false").Select(x => Expression.Constant(false)));
        }

        static Parser<string> DecimalDigits()
        {
            return DecimalDigit().AtLeastOnce();
        }

        static Parser<char> DecimalDigit()
        {
            return TakeWhere(x => '0' <= x && x <= '9');
        }

        static Parser<char> HexDigit()
        {
            return TakeWhere(x => '0' <= x && x <= '9' ||
                                  'A' <= x && x <= 'F' ||
                                  'a' <= x && x <= 'f');
        }

        #endregion
    }

    class Program
    {
        static Parser<int> Expr()
        {
            return Parser.Defer(Term).ChainLeft(Addop());
        }

        static Parser<Func<int, int, int>> Addop()
        {
            return Parser.Or(
                Parser.Char('+').Select(p => (Func<int, int, int>)((x, y) => x + y)),
                Parser.Char('-').Select(p => (Func<int, int, int>)((x, y) => x - y)));
        }

        static Parser<Func<int, int, int>> Expop()
        {
            return Parser.Char('^').Select(p => (Func<int, int, int>)((x, y) => (int)Math.Pow(x, y)));
        }

        static Parser<int> Term()
        {
            return Parser.Defer(Factor).ChainRight(Expop());
        }

        static Parser<int> Factor()
        {
            return Parser.Int().Or(Expr().BracketedBy(Parser.Char('('), Parser.Char(')')));
        }

        static void Main(string[] args)
        {
            var digits = from c in Parser.Char()
                         where '0' <= c && c <= '9'
                         select c;

            var natural = Parser.Natural();
            var many = Parser.Many(Parser.Lowercase());

            var parser = from x in Parser.Lowercase()
                         from y in Parser.Lowercase()
                         where y == 'c'
                         from z in Parser.Lowercase()
                         select new { x, y, z };

            var ints = natural.ManySeparatedBy(Parser.Char(',')).BracketedBy(Parser.Char('['), Parser.Char(']'));

            var expr = Expr();
            var result = expr("1+2-(3+4)").FirstOrDefault();
            Console.WriteLine(result);
        }
    }
}
