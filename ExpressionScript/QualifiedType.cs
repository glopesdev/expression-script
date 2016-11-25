using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    struct QualifiedType
    {
        public Type Type;
        public string TypeName;

        public QualifiedType(Type type, string typeName)
        {
            Type = type;
            TypeName = typeName;
        }

        public QualifiedType Add(GenericName identifier, ITypeResolver resolver)
        {
            var type = Type;
            if (type != null)
            {
                throw new InvalidOperationException("Cannot further qualify an existing type.");
            }

            var typeArguments = identifier.TypeArguments;
            var typeName = TypeName != null ? TypeName + "." + identifier.Name : identifier.Name;
            if (typeArguments != Type.EmptyTypes)
            {
                typeName += "`" + typeArguments.Length;
            }

            type = resolver.GetType(typeName);
            if (typeArguments != Type.EmptyTypes)
            {
                type = type.MakeGenericType(typeArguments);
            }

            return new QualifiedType(type, typeName);
        }
    }
}
