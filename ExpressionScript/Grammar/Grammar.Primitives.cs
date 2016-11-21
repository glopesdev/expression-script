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
                   from w in WhitespaceOrLineTerminators().ManySeparatedBy(Comment())
                   select x;
        }

        public static Parser<string> WhitespaceOrLineTerminators()
        {
            return Whitespace().Or(NewLineCharacter()).Many(string.Empty, (xs, x) => xs);
        }

        public static Parser<char> Whitespace()
        {
            return Char(x =>
                char.GetUnicodeCategory(x) == UnicodeCategory.SpaceSeparator ||
                x == '\u0009' ||  // horizontal tab
                x == '\u000B' ||  // vertical tab
                x == '\u000C');   // form feed
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

        public static Parser<Type> TypeName()
        {
            return from typeName in Identifier().ManySeparatedBy(Char('.'), 1)
                   from typeArguments in TypeArgumentList().Or(Return(Enumerable.Empty<Type>()))
                                                           .Select(x => x.ToArray())
                   let suffix = typeArguments.Length > 0 ? "`" + typeArguments.Length : string.Empty
                   let type = System.Type.GetType(string.Join(".", typeName) + suffix)
                   select type.IsGenericType ? type.MakeGenericType(typeArguments) : type;
        }

        public static Parser<Type> Type()
        {
            return Or(
                ValueType(),
                ReferenceType(),
                TypeName());
        }

        public static Parser<Type> ValueType()
        {
            return StructType();
        }

        public static Parser<Type> ReferenceType()
        {
            return ClassType();
        }

        public static Parser<Type> StructType()
        {
            return SimpleType();
        }

        public static Parser<Type> ClassType()
        {
            return Or(
                String("object").Select(x => typeof(object)),
                String("string").Select(x => typeof(string)));
        }

        public static Parser<Type> SimpleType()
        {
            return Or(
                NumericType(),
                String("bool").Select(x => typeof(bool)));
        }

        public static Parser<Type> NumericType()
        {
            return Or(
                IntegralType(),
                FloatingPointType(),
                String("decimal").Select(x => typeof(decimal)));
        }

        public static Parser<Type> IntegralType()
        {
            return Or(
                String("sbyte").Select(x => typeof(sbyte)),
                String("byte").Select(x => typeof(byte)),
                String("short").Select(x => typeof(short)),
                String("ushort").Select(x => typeof(ushort)),
                String("int").Select(x => typeof(int)),
                String("uint").Select(x => typeof(uint)),
                String("long").Select(x => typeof(long)),
                String("ulong").Select(x => typeof(ulong)),
                String("char").Select(x => typeof(char)));
        }

        public static Parser<Type> FloatingPointType()
        {
            return Or(
                String("float").Select(x => typeof(float)),
                String("double").Select(x => typeof(double)));
        }

        public static Parser<IEnumerable<Type>> TypeArgumentList()
        {
            return TypeArguments().BracketedBy(
                Token(Char('<')),
                Token(Char('>')));
        }

        public static Parser<IEnumerable<Type>> TypeArguments()
        {
            return Type().ManySeparatedBy(Token(Char(',')));
        }

        public static Parser<string> Identifier()
        {
            return AvailableIdentifier().Or(from c in Char('@')
                                            from identifier in IdentifierOrKeyword()
                                            select identifier);
        }

        public static Parser<string> AvailableIdentifier()
        {
            return IdentifierOrKeyword().Except(Keyword());
        }

        public static Parser<string> Keyword()
        {
            return String(
                "abstract", "as", "base", "bool", "break",
                "byte", "case", "catch", "char", "checked",
                "class", "const", "continue", "decimal", "default",
                "delegate", "do", "double", "else", "enum",
                "event", "explicit", "extern", "false", "finally",
                "fixed", "float", "for", "foreach", "goto",
                "if", "implicit", "in", "int", "interface",
                "internal", "is", "lock", "long", "namespace",
                "new", "null", "object", "operator", "out",
                "override", "params", "private", "protected", "public",
                "readonly", "ref", "return", "sbyte", "sealed",
                "short", "sizeof", "stackalloc", "static", "string",
                "struct", "switch", "this", "throw", "true",
                "try", "typeof", "uint", "ulong", "unchecked",
                "unsafe", "ushort", "using", "virtual", "void",
                "volatile", "while");
        }

        public static Parser<string> IdentifierOrKeyword()
        {
            return from c in IdentifierStartCharacter()
                   from s in IdentifierPartCharacter().Many()
                   select c + s;
        }

        public static Parser<char> IdentifierStartCharacter()
        {
            return LetterCharacter().Or(Char('\u005F')); // underscore
        }

        public static Parser<char> IdentifierPartCharacter()
        {
            return Or(
                LetterCharacter(),
                DecimalDigitCharacter(),
                ConnectingCharacter(),
                CombiningCharacter(),
                FormattingCharacter());
        }

        public static Parser<char> LetterCharacter()
        {
            return Or(
                UnicodeEscapeSequence(),
                Char()).Where(x =>
                {
                    var category = char.GetUnicodeCategory(x);
                    return category == UnicodeCategory.UppercaseLetter ||
                           category == UnicodeCategory.LowercaseLetter ||
                           category == UnicodeCategory.TitlecaseLetter ||
                           category == UnicodeCategory.ModifierLetter ||
                           category == UnicodeCategory.OtherLetter ||
                           category == UnicodeCategory.LetterNumber;
                });
        }

        public static Parser<char> CombiningCharacter()
        {
            return Or(
                UnicodeEscapeSequence(),
                Char()).Where(x =>
                {
                    var category = char.GetUnicodeCategory(x);
                    return category == UnicodeCategory.NonSpacingMark ||
                           category == UnicodeCategory.SpacingCombiningMark;
                });
        }

        public static Parser<char> DecimalDigitCharacter()
        {
            return Or(
                UnicodeEscapeSequence(),
                Char()).Where(x =>
                {
                    var category = char.GetUnicodeCategory(x);
                    return category == UnicodeCategory.DecimalDigitNumber;
                }); 
        }

        public static Parser<char> ConnectingCharacter()
        {
            return Or(
                UnicodeEscapeSequence(),
                Char()).Where(x =>
                {
                    var category = char.GetUnicodeCategory(x);
                    return category == UnicodeCategory.ConnectorPunctuation;
                });
        }

        public static Parser<char> FormattingCharacter()
        {
            return Or(
                UnicodeEscapeSequence(),
                Char()).Where(x =>
                {
                    var category = char.GetUnicodeCategory(x);
                    return category == UnicodeCategory.Format;
                });
        }

        public static Parser<Type> PredefinedType()
        {
            return Or(
                String("bool").Select(x => typeof(bool)),
                String("byte").Select(x => typeof(byte)),
                String("char").Select(x => typeof(char)),
                String("decimal").Select(x => typeof(decimal)),
                String("double").Select(x => typeof(double)),
                String("float").Select(x => typeof(float)),
                String("int").Select(x => typeof(int)),
                String("long").Select(x => typeof(long)),
                String("object").Select(x => typeof(object)),
                String("sbyte").Select(x => typeof(sbyte)),
                String("short").Select(x => typeof(short)),
                String("string").Select(x => typeof(string)),
                String("uint").Select(x => typeof(uint)),
                String("ulong").Select(x => typeof(ulong)),
                String("ushort").Select(x => typeof(ushort)));
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
