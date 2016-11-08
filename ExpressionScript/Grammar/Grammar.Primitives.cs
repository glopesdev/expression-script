using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        public static Parser<TValue> Token<TValue>(this Parser<TValue> parser)
        {
            return from x in parser
                   from w in Whitespace().ManySeparatedBy(Comment())
                   select x;
        }

        public static Parser<string> Whitespace()
        {
            return Char(x =>
                char.GetUnicodeCategory(x) == UnicodeCategory.SpaceSeparator ||
                x == '\u0009' ||  // horizontal tab
                x == '\u000B' ||  // vertical tab
                x == '\u000C')    // form feed
                .Many(string.Empty, (xs, x) => xs);
        }

        public static Parser<string> Comment()
        {
            return SingleLineComment().Or(DelimitedComment());
        }

        public static Parser<string> SingleLineComment()
        {
            return from x in String("//")
                   from s in Char().Except(NewLineCharacter()).Many()
                   select s;
        }

        public static Parser<string> DelimitedComment()
        {
            return from x in String("/*")
                   from s in DelimitedCommentSection().Many((xs, c) => xs + c)
                   from a in Char('*').Many(1)
                   from c in Char('/')
                   select s;
        }

        public static Parser<string> DelimitedCommentSection()
        {
            return Or(
                String("/"),
                Char('*').Many().SelectMany(x => NotSlashOrAsterisk().Select(c => x + c)));
        }

        public static Parser<char> NotSlashOrAsterisk()
        {
            return Char().Except('/', '*');
        }

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

        public static Parser<char> SimpleEscapeSequence()
        {
            return Or(
                String(@"\'").Select(x => '\''),
                String("\\\"").Select(x => '\"'),
                String(@"\\").Select(x => '\\'),
                String(@"\0").Select(x => '\0'),
                String(@"\a").Select(x => '\a'),
                String(@"\b").Select(x => '\b'),
                String(@"\f").Select(x => '\f'),
                String(@"\n").Select(x => '\n'),
                String(@"\r").Select(x => '\r'),
                String(@"\t").Select(x => '\t'),
                String(@"\v").Select(x => '\v'));
        }

        public static Parser<char> HexadecimalEscapeSequence()
        {
            return from prefix in String(@"\x")
                   from digits in HexDigit().Many(1, 4)
                   select (char)int.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        public static Parser<char> UnicodeEscapeSequence()
        {
            return (from prefix in String(@"\u")
                    from digits in HexDigit().Many(4)
                    select (char)int.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture))
                    .Or(from prefix in String(@"\U")
                        from digits in HexDigit().Many(8)
                        select (char)int.Parse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
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

        public static Parser<string> RegularStringLiteral()
        {
            return RegularStringLiteralCharacters().BracketedBy(
                String("\""),
                String("\""));
        }

        public static Parser<string> RegularStringLiteralCharacters()
        {
            return RegularStringLiteralCharacter().Many();
        }

        public static Parser<char> RegularStringLiteralCharacter()
        {
            return Or(
                SingleRegularStringLiteralCharacter(),
                SimpleEscapeSequence(),
                HexadecimalEscapeSequence(),
                UnicodeEscapeSequence());
        }

        public static Parser<char> SingleRegularStringLiteralCharacter()
        {
            return Char().Except(Char(
                '\u0022', // quotation mark
                '\u005C') // backslash
                .Concat(NewLineCharacter()));
        }

        public static Parser<string> VerbatimStringLiteral()
        {
            return VerbatimStringLiteralCharacters().BracketedBy(
                String("@\""),
                String("\""));
        }

        public static Parser<string> VerbatimStringLiteralCharacters()
        {
            return VerbatimStringLiteralCharacter().Many();
        }

        public static Parser<char> VerbatimStringLiteralCharacter()
        {
            return Or(
                SingleVerbatimStringLiteralCharacter(),
                QuoteEscapeSequence());
        }

        public static Parser<char> SingleVerbatimStringLiteralCharacter()
        {
            return Char().Except(Char('\u0022')); // quotation mark
        }

        public static Parser<char> QuoteEscapeSequence()
        {
            return String(@"""""").Select(x => '\"');
        }
    }
}
