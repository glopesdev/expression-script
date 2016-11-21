using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionScript
{
    class Map<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public static readonly Map<TKey, TValue> Empty = new Map<TKey, TValue>(null);
        readonly Node root;

        private Map(Node root)
        {
            this.root = root;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGetValue(key, out value))
                {
                    throw new ArgumentException("The specified key was not found.", "key");
                }

                return value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var current = root;
            while (current != null)
            {
                var comparison = Comparer<TKey>.Default.Compare(key, current.key);
                if (comparison == 0)
                {
                    value = current.value;
                    return true;
                }

                if (comparison < 0) current = current.left;
                else current = current.right;
            }

            value = default(TValue);
            return false;
        }

        public Map<TKey, TValue> Add(TKey key, TValue value)
        {
            var node = Add(root, key, value);
            return new Map<TKey, TValue>(node);
        }

        static Node Add(Node node, TKey key, TValue value)
        {
            if (node == null)
            {
                return new Node(key, value, null, null);
            }

            var comparison = Comparer<TKey>.Default.Compare(key, node.key);
            if (comparison == 0) throw new ArgumentException("The specified key already exists.", "key");

            Node left, right;
            if (comparison < 0)
            {
                left = Add(node.left, key, value);
                right = node.right;
            }
            else
            {
                left = node.left;
                right = Add(node.right, key, value);
            }

            node = new Node(node.key, node.value, left, right);
            var leftHeight = node.left == null ? 0 : node.left.height;
            var rightHeight = node.right == null ? 0 : node.right.height;
            var balance = leftHeight - rightHeight;

            if (balance > 1) // left unbalanced
            {
                var innerLeftHeight = node.left.left == null ? 0 : node.left.left.height;
                var innerRightHeight = node.left.right == null ? 0 : node.left.right.height;
                var innerBalance = innerLeftHeight - innerRightHeight;
                if (innerBalance == 1) // single rotation
                {
                    node = RotateRight(node);
                }
                else // double rotation
                {
                    left = RotateLeft(left);
                    node = new Node(node.key, node.value, left, right);
                    node = RotateRight(node);
                }
            }
            else if (balance < -1) // right unbalanced
            {
                var innerLeftHeight = node.right.left == null ? 0 : node.right.left.height;
                var innerRightHeight = node.right.right == null ? 0 : node.right.right.height;
                var innerBalance = innerLeftHeight - innerRightHeight;
                if (innerBalance == 1) // double rotation
                {
                    right = RotateRight(right);
                    node = new Node(node.key, node.value, left, right);
                    node = RotateLeft(node);
                }
                else // single rotation
                {
                    node = RotateLeft(node);
                }
            }

            return node;
        }

        static Node RotateLeft(Node node)
        {
            var left = new Node(node.key, node.value, node.left, node.right.left);
            return new Node(node.right.key, node.right.value, left, node.right.right);
        }

        static Node RotateRight(Node node)
        {
            var right = new Node(node.key, node.value, node.left.right, node.right);
            return new Node(node.left.key, node.left.value, node.left.left, right);
        }

        [DebuggerDisplay("\\{{key}, {value}\\}")]
        class Node
        {
            internal readonly TKey key;
            internal readonly TValue value;
            internal readonly int height;
            internal readonly Node left;
            internal readonly Node right;

            public Node(TKey key, TValue value, Node left, Node right)
            {
                this.key = key;
                this.value = value;
                this.left = left;
                this.right = right;

                var leftHeight = left == null ? 0 : left.height;
                var rightHeight = right == null ? 0 : right.height;
                height = Math.Max(leftHeight, rightHeight) + 1;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (root == null) yield break;

            var current = root;
            var stack = new Stack<Node>(root.height);
            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.left;
                }

                current = stack.Pop();
                yield return new KeyValuePair<TKey, TValue>(current.key, current.value);
                current = current.right;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
