using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    public class NameTable
    {
        public static readonly NameTable Empty = new NameTable(Map<string, Type>.Empty, Map<string, ParameterExpression>.Empty);

        readonly Map<string, Type> types;
        readonly Map<string, ParameterExpression> variables;

        private NameTable(Map<string, Type> types, Map<string, ParameterExpression> variables)
        {
            this.types = types;
            this.variables = variables;
        }

        public NameTable CreateScope()
        {
            return new NameTable(types, variables);
        }

        public NameTable CreateScope(IEnumerable<ParameterExpression> parameters)
        {
            var variables = this.variables;
            foreach (var parameter in parameters)
            {
                variables = variables.Add(parameter.Name, parameter);
            }

            return new NameTable(types, variables);
        }

        public NameTable AddType(string name, Type type)
        {
            return new NameTable(types.Add(name, type), variables);
        }

        public NameTable AddVariable(ParameterExpression variable)
        {
            return new NameTable(types, variables.Add(variable.Name, variable));
        }

        public Type GetType(string name)
        {
            return types[name];
        }

        public ParameterExpression GetVariable(string name)
        {
            return variables[name];
        }

        public IEnumerable<KeyValuePair<string, Type>> GetScopeTypes()
        {
            return types;
        }

        public IEnumerable<ParameterExpression> GetScopeVariables()
        {
            return variables.Select(x => x.Value);
        }
    }
}
