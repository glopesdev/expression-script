using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    interface IImmutableStack<T> : IEnumerable<T>
    {
        IImmutableStack<T> Push(T value);
        IImmutableStack<T> Pop();
        T Peek();
    }

    class ImmutableStack<T> : IImmutableStack<T>
    {
        public static readonly IImmutableStack<T> Empty = new EmptyStack();
        readonly T head;
        readonly IImmutableStack<T> tail;

        private ImmutableStack(T head, IImmutableStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public IImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this);
        }

        public IImmutableStack<T> Pop()
        {
            return tail;
        }

        public T Peek()
        {
            return head;
        }

        public IEnumerator<T> GetEnumerator()
        {
            IImmutableStack<T> stack = this;
            do
            {
                yield return stack.Peek();
                stack = stack.Pop();
            }
            while (stack != Empty);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class EmptyStack : IImmutableStack<T>
        {
            public IImmutableStack<T> Push(T value)
            {
                return new ImmutableStack<T>(value, this);
            }

            public IImmutableStack<T> Pop()
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            public T Peek()
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
