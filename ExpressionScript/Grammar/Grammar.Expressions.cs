﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public static partial class Parser
    {
        static Func<TValue, TValue> Operator<TValue>(Func<TValue, TValue> func)
        {
            return func;
        }

        static Func<TValue, TValue, TValue> Operator<TValue>(Func<TValue, TValue, TValue> func)
        {
            return func;
        }

        public static Parser<Expression> ExpressionTree()
        {
            return NonAssignmentExpression().Or(LambdaExpression());
        }

        public static Parser<Expression> NonAssignmentExpression()
        {
            return ConditionalExpression();
        }

        public static Parser<Expression> ConditionalExpression()
        {
            return NullCoalescingExpression().SelectMany(x =>
                (from q in Token(Char('?'))
                 from e1 in ExpressionTree()
                 from c in Token(Char(':'))
                 from e2 in ExpressionTree()
                 select Expression.Condition(x, e1, e2))
                 .Or(Return(x)));
        }

        public static Parser<Expression> NullCoalescingExpression()
        {
            return ConditionalOrExpression().ChainLeft(
                Token(String("??")).Select(x => Operator<Expression>(Expression.Coalesce)));
        }

        public static Parser<Expression> ConditionalOrExpression()
        {
            return ConditionalAndExpression().ChainLeft(
                Token(String("||")).Select(x => Operator<Expression>(Expression.OrElse)));
        }

        public static Parser<Expression> ConditionalAndExpression()
        {
            return InclusiveOrExpression().ChainLeft(
                Token(String("&&")).Select(x => Operator<Expression>(Expression.AndAlso)));
        }

        public static Parser<Expression> InclusiveOrExpression()
        {
            return ExclusiveOrExpression().ChainLeft(
                Token(Char('|')).Select(x => Operator<Expression>(Expression.Or)));
        }

        public static Parser<Expression> ExclusiveOrExpression()
        {
            return AndExpression().ChainLeft(
                Token(Char('^')).Select(x => Operator<Expression>(Expression.ExclusiveOr)));
        }

        public static Parser<Expression> AndExpression()
        {
            return EqualityExpression().ChainLeft(
                Token(Char('&')).Select(x => Operator<Expression>(Expression.And)));
        }

        public static Parser<Expression> EqualityExpression()
        {
            return RelationalExpression().ChainLeft(Or(
                Token(String("==")).Select(x => Operator<Expression>(Expression.Equal)),
                Token(String("!=")).Select(x => Operator<Expression>(Expression.NotEqual))));
        }

        public static Parser<Expression> RelationalExpression()
        {
            return ShiftExpression().ChainLeft(Or(
                Token(Char('<')).Select(x => Operator<Expression>(Expression.LessThan)),
                Token(Char('>')).Select(x => Operator<Expression>(Expression.GreaterThan)),
                Token(String("<=")).Select(x => Operator<Expression>(Expression.LessThanOrEqual)),
                Token(String(">=")).Select(x => Operator<Expression>(Expression.GreaterThanOrEqual))));
        }

        public static Parser<Expression> ShiftExpression()
        {
            return AdditiveExpression().ChainLeft(Or(
                String("<<").Select(x => Operator<Expression>(Expression.LeftShift)),
                String(">>").Select(x => Operator<Expression>(Expression.RightShift))));
        }

        public static Parser<Expression> AdditiveExpression()
        {
            return MultiplicativeExpression().ChainLeft(Or(
                Token(Char('+')).Select(x => Operator<Expression>(Expression.Add)),
                Token(Char('-')).Select(x => Operator<Expression>(Expression.Subtract))));
        }

        public static Parser<Expression> MultiplicativeExpression()
        {
            return UnaryExpression().ChainLeft(Or(
                Token(Char('*')).Select(x => Operator<Expression>(Expression.Multiply)),
                Token(Char('/')).Select(x => Operator<Expression>(Expression.Divide)),
                Token(Char('%')).Select(x => Operator<Expression>(Expression.Modulo))));
        }

        public static Parser<Expression> UnaryExpression()
        {
            return PrimaryExpression()
                .PreIncrementExpression()
                .PreDecrementExpression();
        }

        public static Parser<Expression> PrimaryExpression()
        {
            return Or(
                PrimaryNoArrayCreationExpression(),
                ArrayCreationExpression());
        }

        public static Parser<Expression> PrimaryNoArrayCreationExpression()
        {
            return Or(
                Literal(),
                SimpleName(),
                StaticMemberAccess(),
                ParenthesizedExpression(),
                TypeofExpression(),
                DefaultValueExpression())
                .MemberAccess()
                .PostIncrementExpression()
                .PostDecrementExpression();
        }

        public static Parser<Expression> ArrayCreationExpression()
        {
            return from keyword in Token(String("new"))
                   from type in Type()
                   from o in Token(Char('['))
                   from c in Token(Char(']'))
                   from initializers in ExpressionTree().ManySeparatedBy(Token(Char(',')))
                                                        .BracketedBy(Token(Char('{')), Token(Char('}')))
                                                        .Optional(Enumerable.Empty<Expression>())
                   select Expression.NewArrayInit(type, initializers);
        }

        public static Parser<Expression> SimpleName()
        {
            return from identifier in Token(Identifier())
                   from state in State()
                   let variable = state.GetVariable(identifier)
                   where variable != null
                   select variable;
        }

        public static Parser<Expression> ParenthesizedExpression()
        {
            return Defer(ExpressionTree).BracketedBy(
                Token(Char('(')),
                Token(Char(')')));
        }

        public static Parser<Expression> PostIncrementExpression(this Parser<Expression> parser)
        {
            return from x in parser
                   from e in Token(String("++")).Or(Return(string.Empty))
                   select string.IsNullOrEmpty(e) ? x : Expression.PostIncrementAssign(x);
        }

        public static Parser<Expression> PostDecrementExpression(this Parser<Expression> parser)
        {
            return from x in parser
                   from e in Token(String("--")).Or(Return(string.Empty))
                   select string.IsNullOrEmpty(e) ? x : Expression.PostDecrementAssign(x);
        }

        public static Parser<Expression> PreIncrementExpression(this Parser<Expression> parser)
        {
            return from e in Token(String("++")).Or(Return(string.Empty))
                   from x in parser
                   select string.IsNullOrEmpty(e) ? x : Expression.PreIncrementAssign(x);
        }

        public static Parser<Expression> PreDecrementExpression(this Parser<Expression> parser)
        {
            return from e in Token(String("--")).Or(Return(string.Empty))
                   from x in parser
                   select string.IsNullOrEmpty(e) ? x : Expression.PreDecrementAssign(x);
        }

        public static Parser<IEnumerable<Expression>> ArgumentList()
        {
            return ExpressionTree().ManySeparatedBy(Token(Char(',')));
        }

        public static Parser<Expression> StaticMemberAccess()
        {
            return from type in Token(OuterType())
                   from staticMember in StaticAccess(type)
                   where staticMember.Identifier != null
                   from memberAccess in Or(StaticInvocationExpression(staticMember),
                                           Defer(() => StaticMemberAccessExpression(staticMember)))
                   select memberAccess;
        }

        static Parser<Expression> StaticMemberAccessExpression(StaticAccess staticMember)
        {
            var identifier = staticMember.Identifier.Value;
            var propertyOrField = staticMember.Type.GetMember(identifier.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (propertyOrField.Length == 0)
            {
                throw new ArgumentException("The specified member name was not found.", "propertyOrFieldName");
            }

            if (propertyOrField.Length > 1)
            {
                throw new ArgumentException("The specified member name is ambiguous.", "propertyOrFieldName");
            }

            return Return(Expression.MakeMemberAccess(null, propertyOrField[0]));
        }

        static Parser<Expression> StaticInvocationExpression(StaticAccess staticMember)
        {
            return from o in Token(Char('('))
                   from arguments in ArgumentList()
                   from c in Token(Char(')'))
                   let identifier = staticMember.Identifier.Value
                   select Expression.Call(staticMember.Type, identifier.Name, identifier.TypeArguments, arguments.ToArray());
        }

        public static Parser<Expression> MemberAccess(this Parser<Expression> parser)
        {
            return parser.ChainLeft(
                Or(from c in Token(Char('.'))
                   from identifier in Identifier()
                   from accessOperator in Or(InvocationExpression(identifier),
                                             MemberAccessExpression(identifier))
                   select accessOperator,
                   ElementAccessExpression()));
        }

        public static Parser<Func<Expression, Expression>> MemberAccessExpression(string propertyOrFieldName)
        {
            return Return(Operator<Expression>(x => Expression.PropertyOrField(x, propertyOrFieldName)));
        }

        public static Parser<Func<Expression, Expression>> InvocationExpression(string methodName)
        {
            return from o in Token(Char('('))
                   from arguments in ArgumentList()
                   from c in Token(Char(')'))
                   select Operator<Expression>(x => Expression.Call(x, methodName, null, arguments.ToArray()));
        }

        public static Parser<Func<Expression, Expression>> ElementAccessExpression()
        {
            return from o in Token(Char('['))
                   from arguments in ArgumentList()
                   from c in Token(Char(']'))
                   select Operator<Expression>(x =>
                   {
                       var type = x.Type;
                       if (type.IsArray) return Expression.ArrayAccess(x, arguments);
                       else
                       {
                           var defaultMembers = type.GetDefaultMembers();
                           var indexer = (PropertyInfo)defaultMembers.Single(member => member.MemberType == MemberTypes.Property);
                           return Expression.Property(x, indexer, arguments);
                       }
                   });
        }

        public static Parser<Expression> TypeofExpression()
        {
            return from keyword in Token(String("typeof"))
                   from type in Or(Type(),
                                   String("void").Select(x => typeof(void)))
                                   .BracketedBy(Token(Char('(')), Token(Char(')')))
                   select Expression.Constant(type);
        }

        public static Parser<Expression> DefaultValueExpression()
        {
            return from keyword in Token(String("default"))
                   from type in Type().BracketedBy(Token(Char('(')), Token(Char(')')))
                   select Expression.Default(type);
        }

        public static Parser<Expression> LambdaExpression()
        {
            return ExplicitAnonymousFunctionSignature().Or(ImplicitAnonymousFunctionSignature()).WithState(
                (parameters, state) => state.CreateScope(parameters),
                parameters => from arrow in Token(String("=>"))
                              from body in ExpressionTree()
                              from state in State()
                              select Expression.Lambda(state.ExpectedType, body, parameters));
        }

        public static Parser<IEnumerable<ParameterExpression>> ExplicitAnonymousFunctionSignature()
        {
            return from o in Token(Char('('))
                   from parameters in ExplicitAnonymousFunctionParameter().ManySeparatedBy(Token(Char(',')))
                   from c in Token(Char(')'))
                   select parameters;
        }

        public static Parser<ParameterExpression> ExplicitAnonymousFunctionParameter()
        {
            return from type in Token(Type())
                   from identifier in Token(Identifier())
                   select Expression.Parameter(type, identifier);
        }

        public static Parser<IEnumerable<ParameterExpression>> ImplicitAnonymousFunctionSignature()
        {
            return from o in Token(Char('(')).Optional()
                   from state in State()
                   where state.ExpectedType != null && state.ExpectedType.IsSubclassOf(typeof(Delegate))
                   let expectedParameters = state.ExpectedType.GetMethod("Invoke").GetParameters()
                   where o != default(char) || expectedParameters.Length == 1
                   from parameters in ImplicitAnonymousFunctionParameters(expectedParameters)
                   from c in o == default(char) ? Return(default(char)) : Token(Char(')'))
                   select parameters;
        }

        public static Parser<IEnumerable<ParameterExpression>> ImplicitAnonymousFunctionParameters(ParameterInfo[] expectedParameters)
        {
            return Token(Identifier()).ManySeparatedBy(
                Token(Char(',')),
                expectedParameters.Length,
                new { n = 0, xs = Enumerable.Empty<ParameterExpression>() },
                (acc, i) => i >= expectedParameters.Length,
                (acc, x) => new { n = acc.n + 1, xs = acc.xs.Concat(Expression.Parameter(expectedParameters[acc.n].ParameterType, x)) })
                .Select(acc => acc.xs);
        }
    }
}
