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
            Map<string, Type>.Empty,
            Map<string, ParameterExpression>.Empty,
            ImmutableStack<ParameterExpression>.Empty);

        readonly Map<string, Type> types;
        readonly Map<string, ParameterExpression> nameTable;
        readonly IImmutableStack<ParameterExpression> variables;

        private ParserContext(
            Type expectedType,
            Map<string, Type> types, Map<string,
            ParameterExpression> nameTable,
            IImmutableStack<ParameterExpression> variables)
        {
            this.types = types;
            this.nameTable = nameTable;
            this.variables = variables;
            ExpectedType = expectedType;
        }

        public Type ExpectedType { get; private set; }

        public ParserContext CreateScope()
        {
            if (variables == ImmutableStack<ParameterExpression>.Empty) return this;
            else return new ParserContext(ExpectedType, types, nameTable, ImmutableStack<ParameterExpression>.Empty);
        }

        public ParserContext CreateScope(IEnumerable<ParameterExpression> parameters)
        {
            var nameTable = this.nameTable;
            foreach (var parameter in parameters)
            {
                nameTable = nameTable.Add(parameter.Name, parameter);
            }

            return new ParserContext(ExpectedType, types, nameTable, ImmutableStack<ParameterExpression>.Empty);
        }

        public ParserContext AddType(string name, Type type)
        {
            return new ParserContext(ExpectedType, types.Add(name, type), nameTable, variables);
        }

        public ParserContext AddVariable(ParameterExpression variable)
        {
            return new ParserContext(ExpectedType, types, nameTable.Add(variable.Name, variable), variables.Push(variable));
        }

        public ParserContext SetExpectedType(Type expectedType)
        {
            return new ParserContext(expectedType, types, nameTable, variables);
        }

        public Type GetType(string name)
        {
            return types[name];
        }

        public ParameterExpression GetVariable(string name)
        {
            return nameTable[name];
        }

        public IEnumerable<KeyValuePair<string, Type>> GetScopeTypes()
        {
            return types;
        }

        public IEnumerable<ParameterExpression> GetScopeVariables()
        {
            return variables;
        }
    }
}
