using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    struct StaticAccess
    {
        public Type Type;
        public GenericName? Identifier;

        public StaticAccess(Type type, GenericName? identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public StaticAccess Add(GenericName identifier)
        {
            var type = Type;
            var name = identifier.Name;
            var typeArguments = identifier.TypeArguments;
            if (typeArguments != Type.EmptyTypes)
            {
                name += "`" + typeArguments.Length;
            }

            var nestedType = type.GetNestedType(name);
            if (nestedType == null) return new StaticAccess(type, identifier);

            if (typeArguments != Type.EmptyTypes)
            {
                nestedType = nestedType.MakeGenericType(typeArguments);
            }

            return new StaticAccess(nestedType, null);
        }
    }
}
