using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class ParserContext
    {
        public static readonly ParserContext Empty = new ParserContext(
            null,
            ExpressionScript.TypeResolver.Default,
            Map<string, ParameterExpression>.Empty,
            ImmutableStack<ParameterExpression>.Empty);

        readonly Map<string, ParameterExpression> nameTable;
        readonly IImmutableStack<ParameterExpression> variables;

        private ParserContext(
            Type expectedType,
            ITypeResolver typeResolver,
            Map<string, ParameterExpression> nameTable,
            IImmutableStack<ParameterExpression> variables)
        {
            this.nameTable = nameTable;
            this.variables = variables;
            ExpectedType = expectedType;
            TypeResolver = typeResolver;
        }

        public Type ExpectedType { get; private set; }

        public ITypeResolver TypeResolver { get; private set; }

        public ParserContext CreateScope()
        {
            if (variables == ImmutableStack<ParameterExpression>.Empty) return this;
            else return new ParserContext(ExpectedType, TypeResolver, nameTable, ImmutableStack<ParameterExpression>.Empty);
        }

        public ParserContext CreateScope(IEnumerable<ParameterExpression> parameters)
        {
            var nameTable = this.nameTable;
            foreach (var parameter in parameters)
            {
                nameTable = nameTable.Add(parameter.Name, parameter);
            }

            return new ParserContext(ExpectedType, TypeResolver, nameTable, ImmutableStack<ParameterExpression>.Empty);
        }

        public ParserContext AddVariable(ParameterExpression variable)
        {
            return new ParserContext(ExpectedType, TypeResolver, nameTable.Add(variable.Name, variable), variables.Push(variable));
        }

        public ParserContext SetExpectedType(Type expectedType)
        {
            return new ParserContext(expectedType, TypeResolver, nameTable, variables);
        }

        public ParameterExpression GetVariable(string name)
        {
            ParameterExpression variable;
            nameTable.TryGetValue(name, out variable);
            return variable;
        }

        public IEnumerable<ParameterExpression> GetScopeVariables()
        {
            return variables;
        }
    }
}
