using System;
using System.Collections;
using System.Collections.Generic;
namespace MyCollections
{
    internal class TreeNode<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
        public TreeNode<K, V> Left { get; set; }
        public TreeNode<K, V> Right { get; set; }
        public TreeNode<K, V> Parent { get; set; }
        public bool IsRed { get; set; }
        public TreeNode(K key, V value)
        {
            Key = key;
            Value = value;
            IsRed = true;
        }
    }
    public class NaturalComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }
    }
    public interface ITreeMapComparator<T>
    {
        int Compare(T a, T b);
    }
    public class ComparatorAdapter<T> : IComparer<T>
    {
        private readonly ITreeMapComparator<T> _comparator;
        public ComparatorAdapter(ITreeMapComparator<T> comparator)
        {
            _comparator = comparator ?? throw new ArgumentNullException(nameof(comparator));
        }
        public int Compare(T x, T y)
        {
            return _comparator.Compare(x, y);
        }
    }
    public class MyTreeMap<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private TreeNode<K, V> _root;
        private readonly IComparer<K> _comparator;
        private int _size;
        public MyTreeMap()
        {
            _comparator = new NaturalComparer<K>();
            _root = null;
            _size = 0;
        }
        public MyTreeMap(IComparer<K> comparator)
        {
            _comparator = comparator ?? new NaturalComparer<K>();
            _root = null;
            _size = 0;
        }
        public MyTreeMap(ITreeMapComparator<K> comparator)
        {
            _comparator = new ComparatorAdapter<K>(comparator);
            _root = null;
            _size = 0;
        }
        //Свойства
        public int Size => _size;
        public bool IsEmpty => _size == 0;
        public IComparer<K> Comparator => _comparator;
        //Очистка
        public void Clear()
        {
            _root = null;
            _size = 0;
        }
        //Проверка наличия ключа
        public bool ContainsKey(K key)
        {
            return FindNode(key) != null;
        }
        //Получение значения
        public V Get(K key)
        {
            var node = FindNode(key);
            return node != null ? node.Value : default(V);
        }
        //Добавление/обновление
        public V Put(K key, V value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var newNode = new TreeNode<K, V>(key, value);
            var insertedNode = InsertNode(newNode);
            if (insertedNode == null)
            {
                var existingNode = FindNode(key);
                V oldValue = existingNode.Value;
                existingNode.Value = value;
                return oldValue;
            }
            _size++;
            FixAfterInsertion(insertedNode);
            return default(V);
        }
        //Удаление
        public V Remove(K key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var node = FindNode(key);
            if (node == null)
                return default(V);
            V oldValue = node.Value;
            DeleteNode(node);
            _size--;
            return oldValue;
        }
        //Поиск первого ключа
        public K FirstKey()
        {
            if (_root == null)
                throw new InvalidOperationException("TreeMap is empty");
            var node = GetMinimum(_root);
            return node.Key;
        }
        //Поиск последнего ключа
        public K LastKey()
        {
            if (_root == null)
                throw new InvalidOperationException("TreeMap is empty");
            var node = GetMaximum(_root);
            return node.Key;
        }
        //Поиск первого элемента
        public KeyValuePair<K, V> FirstEntry()
        {
            if (_root == null)
                throw new InvalidOperationException("TreeMap is empty");
            var node = GetMinimum(_root);
            return new KeyValuePair<K, V>(node.Key, node.Value);
        }
        //Поиск последнего элемента
        public KeyValuePair<K, V> LastEntry()
        {
            if (_root == null)
                throw new InvalidOperationException("TreeMap is empty");
            var node = GetMaximum(_root);
            return new KeyValuePair<K, V>(node.Key, node.Value);
        }
        //Поиск узла по ключу
        private TreeNode<K, V> FindNode(K key)
        {
            var current = _root;
            while (current != null)
            {
                int cmp = _comparator.Compare(key, current.Key);
                if (cmp < 0)
                    current = current.Left;
                else if (cmp > 0)
                    current = current.Right;
                else
                    return current;
            }
            return null;
        }
        //Вставка узла
        private TreeNode<K, V> InsertNode(TreeNode<K, V> newNode)
        {
            if (_root == null)
            {
                _root = newNode;
                _root.IsRed = false;
                return newNode;
            }
            TreeNode<K, V> current = _root;
            TreeNode<K, V> parent = null;
            while (current != null)
            {
                parent = current;
                int cmp = _comparator.Compare(newNode.Key, current.Key);
                if (cmp < 0)
                    current = current.Left;
                else if (cmp > 0)
                    current = current.Right;
                else
                    return null; 
            }
            newNode.Parent = parent;
            int cmp2 = _comparator.Compare(newNode.Key, parent.Key);
            if (cmp2 < 0)
                parent.Left = newNode;
            else
                parent.Right = newNode;

            return newNode;
        }
        //Исправление после вставки
        private void FixAfterInsertion(TreeNode<K, V> node)
        {
            node.IsRed = true;
            while (node != null && node != _root && IsRed(node.Parent))
            {
                if (IsLeftChild(node.Parent))
                {
                    var uncle = GetRightChild(GetGrandparent(node));
                    if (IsRed(uncle))
                    {
                        //1: дядя красный
                        node.Parent.IsRed = false;
                        uncle.IsRed = false;
                        GetGrandparent(node).IsRed = true;
                        node = GetGrandparent(node);
                    }
                    else
                    {
                        //2: дядя чёрный
                        if (!IsLeftChild(node))
                        {
                            node = node.Parent;
                            RotateLeft(node);
                        }
                        node.Parent.IsRed = false;
                        GetGrandparent(node).IsRed = true;
                        RotateRight(GetGrandparent(node));
                    }
                }
                else
                {
                    var uncle = GetLeftChild(GetGrandparent(node));
                    if (IsRed(uncle))
                    {
                        node.Parent.IsRed = false;
                        uncle.IsRed = false;
                        GetGrandparent(node).IsRed = true;
                        node = GetGrandparent(node);
                    }
                    else
                    {
                        if (IsLeftChild(node))
                        {
                            node = node.Parent;
                            RotateRight(node);
                        }
                        node.Parent.IsRed = false;
                        GetGrandparent(node).IsRed = true;
                        RotateLeft(GetGrandparent(node));
                    }
                }
            }
            _root.IsRed = false;
        }
        //Удаление узла
        private void DeleteNode(TreeNode<K, V> node)
        {
            if (node.Left != null && node.Right != null)
            {
                var successor = GetMinimum(node.Right);
                CopyNode(successor, node);
                node = successor;
            }
            var replacement = node.Left ?? node.Right;
            if (replacement != null)
            {
                replacement.Parent = node.Parent;
                if (node.Parent == null)
                    _root = replacement;
                else if (node == node.Parent.Left)
                    node.Parent.Left = replacement;
                else
                    node.Parent.Right = replacement;
                node.Left = node.Right = node.Parent = null;
                if (!IsRed(node))
                    FixAfterDeletion(replacement);
            }
            else if (node.Parent == null)
            {
                _root = null;
            }
            else
            {
                if (!IsRed(node))
                    FixAfterDeletion(node);
                if (node.Parent != null)
                {
                    if (node == node.Parent.Left)
                        node.Parent.Left = null;
                    else if (node == node.Parent.Right)
                        node.Parent.Right = null;
                    node.Parent = null;
                }
            }
        }
        //Исправление после удаления
        private void FixAfterDeletion(TreeNode<K, V> node)
        {
            while (node != _root && !IsRed(node))
            {
                if (IsLeftChild(node))
                {
                    var sibling = GetRightChild(node.Parent);
                    if (IsRed(sibling))
                    {
                        sibling.IsRed = false;
                        node.Parent.IsRed = true;
                        RotateLeft(node.Parent);
                        sibling = GetRightChild(node.Parent);
                    }
                    if (!IsRed(GetLeftChild(sibling)) && !IsRed(GetRightChild(sibling)))
                    {
                        sibling.IsRed = true;
                        node = node.Parent;
                    }
                    else
                    {
                        if (!IsRed(GetRightChild(sibling)))
                        {
                            if (GetLeftChild(sibling) != null)
                                GetLeftChild(sibling).IsRed = false;
                            sibling.IsRed = true;
                            RotateRight(sibling);
                            sibling = GetRightChild(node.Parent);
                        }
                        sibling.IsRed = IsRed(node.Parent);
                        node.Parent.IsRed = false;
                        if (GetRightChild(sibling) != null)
                            GetRightChild(sibling).IsRed = false;
                        RotateLeft(node.Parent);
                        node = _root;
                    }
                }
                else
                {
                    var sibling = GetLeftChild(node.Parent);
                    if (IsRed(sibling))
                    {
                        sibling.IsRed = false;
                        node.Parent.IsRed = true;
                        RotateRight(node.Parent);
                        sibling = GetLeftChild(node.Parent);
                    }
                    if (!IsRed(GetRightChild(sibling)) && !IsRed(GetLeftChild(sibling)))
                    {
                        sibling.IsRed = true;
                        node = node.Parent;
                    }
                    else
                    {
                        if (!IsRed(GetLeftChild(sibling)))
                        {
                            if (GetRightChild(sibling) != null)
                                GetRightChild(sibling).IsRed = false;
                            sibling.IsRed = true;
                            RotateLeft(sibling);
                            sibling = GetLeftChild(node.Parent);
                        }
                        sibling.IsRed = IsRed(node.Parent);
                        node.Parent.IsRed = false;
                        if (GetLeftChild(sibling) != null)
                            GetLeftChild(sibling).IsRed = false;
                        RotateRight(node.Parent);
                        node = _root;
                    }
                }
            }
            if (node != null)
                node.IsRed = false;
        }
        //Вспомогательные методы навигации
        private TreeNode<K, V> GetParent(TreeNode<K, V> node) => node?.Parent;
        private TreeNode<K, V> GetGrandparent(TreeNode<K, V> node) => node?.Parent?.Parent;
        private TreeNode<K, V> GetLeftChild(TreeNode<K, V> node) => node?.Left;
        private TreeNode<K, V> GetRightChild(TreeNode<K, V> node) => node?.Right;
        private bool IsLeftChild(TreeNode<K, V> node)
        {
            if (node?.Parent == null) return false;
            return node == node.Parent.Left;
        }
        private bool IsRed(TreeNode<K, V> node) => node != null && node.IsRed;
        //Вращения
        private void RotateLeft(TreeNode<K, V> node)
        {
            if (node?.Right == null) return;

            var rightChild = node.Right;
            node.Right = rightChild.Left;
            if (rightChild.Left != null)
                rightChild.Left.Parent = node;
            rightChild.Parent = node.Parent;
            if (node.Parent == null)
                _root = rightChild;
            else if (node == node.Parent.Left)
                node.Parent.Left = rightChild;
            else
                node.Parent.Right = rightChild;
            rightChild.Left = node;
            node.Parent = rightChild;
        }
        private void RotateRight(TreeNode<K, V> node)
        {
            if (node?.Left == null) return;

            var leftChild = node.Left;
            node.Left = leftChild.Right;
            if (leftChild.Right != null)
                leftChild.Right.Parent = node;
            leftChild.Parent = node.Parent;
            if (node.Parent == null)
                _root = leftChild;
            else if (node == node.Parent.Right)
                node.Parent.Right = leftChild;
            else
                node.Parent.Left = leftChild;
            leftChild.Right = node;
            node.Parent = leftChild;
        }
        //Поиск минимального узла
        private TreeNode<K, V> GetMinimum(TreeNode<K, V> node)
        {
            while (node?.Left != null)
                node = node.Left;
            return node;
        }
        //Поиск максимального узла
        private TreeNode<K, V> GetMaximum(TreeNode<K, V> node)
        {
            while (node?.Right != null)
                node = node.Right;
            return node;
        }
        //Копирование данных узла
        private void CopyNode(TreeNode<K, V> source, TreeNode<K, V> destination)
        {
            destination.Key = source.Key;
            destination.Value = source.Value;
        }
        //Реализация итератора
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return InOrderTraversal(_root).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        private IEnumerable<KeyValuePair<K, V>> InOrderTraversal(TreeNode<K, V> node)
        {
            if (node == null)
                yield break;
            foreach (var item in InOrderTraversal(node.Left))
                yield return item;
            yield return new KeyValuePair<K, V>(node.Key, node.Value);
            foreach (var item in InOrderTraversal(node.Right))
                yield return item;
        }
    }
    public class MyTreeSet<T> : IEnumerable<T>
    {
        private readonly MyTreeMap<T, object> _map;
        private static readonly object PRESENT = new object();
        //Конструкторы
        public MyTreeSet()
        {
            _map = new MyTreeMap<T, object>();
        }
        public MyTreeSet(IComparer<T> comparator)
        {
            _map = new MyTreeMap<T, object>(comparator);
        }
        public MyTreeSet(ITreeMapComparator<T> comparator)
        {
            _map = new MyTreeMap<T, object>(comparator);
        }
        public MyTreeSet(T[] array)
        {
            _map = new MyTreeMap<T, object>();
            if (array != null)
            {
                AddAll(array);
            }
        }
        public MyTreeSet(SortedSet<T> sortedSet)
        {
            _map = new MyTreeMap<T, object>();
            if (sortedSet != null)
            {
                foreach (var item in sortedSet)
                {
                    Add(item);
                }
            }
        }
        //Основные операции
        public bool Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            return _map.Put(item, PRESENT) == null;
        }
        public bool AddAll(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            bool modified = false;
            foreach (var item in array)
            {
                if (Add(item))
                    modified = true;
            }
            return modified;
        }
        public void Clear()
        {
            _map.Clear();
        }
        public bool Contains(object obj)
        {
            if (obj == null) return false;
            try
            {
                return _map.ContainsKey((T)obj);
            }
            catch
            {
                return false;
            }
        }
        public bool ContainsAll(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            foreach (var item in array)
            {
                if (!Contains(item))
                    return false;
            }
            return true;
        }
        public bool IsEmpty()
        {
            return _map.Size == 0;
        }
        public bool Remove(object obj)
        {
            if (obj == null) return false;
            try
            {
                return _map.Remove((T)obj) != null;
            }
            catch
            {
                return false;
            }
        }
        public bool RemoveAll(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            bool modified = false;
            foreach (var item in array)
            {
                if (Remove(item))
                    modified = true;
            }
            return modified;
        }
        public bool RetainAll(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            var toRemove = new List<T>();
            foreach (var item in this)
            {
                bool found = false;
                foreach (var keep in array)
                {
                    if (Equals(item, keep))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    toRemove.Add(item);
            }
            bool modified = false;
            foreach (var item in toRemove)
            {
                if (Remove(item))
                    modified = true;
            }
            return modified;
        }
        public int Size()
        {
            return _map.Size;
        }
        public T[] ToArray()
        {
            var list = new List<T>();
            foreach (var item in this)
            {
                list.Add(item);
            }
            return list.ToArray();
        }
        public T[] ToArray(T[] a)
        {
            if (a == null)
                return ToArray();
            var list = new List<T>();
            foreach (var item in this)
            {
                list.Add(item);
            }
            if (a.Length >= list.Count)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    a[i] = list[i];
                }
                if (a.Length > list.Count)
                    a[list.Count] = default(T);
                return a;
            }
            return list.ToArray();
        }
        //Методы для NavigableSet
        public T First()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Set is empty");
            return _map.FirstKey();
        }
        public T Last()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Set is empty");
            return _map.LastKey();
        }
        public MyTreeSet<T> SubSet(T fromElement, T toElement)
        {
            if (fromElement == null || toElement == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                int cmpFrom = _map.Comparator.Compare(item, fromElement);
                int cmpTo = _map.Comparator.Compare(item, toElement);
                if (cmpFrom >= 0 && cmpTo < 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public MyTreeSet<T> HeadSet(T toElement)
        {
            if (toElement == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, toElement) < 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public MyTreeSet<T> TailSet(T fromElement)
        {
            if (fromElement == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, fromElement) >= 0)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public T Ceiling(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            T result = default(T);
            bool found = false;
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, obj) >= 0)
                {
                    if (!found || _map.Comparator.Compare(item, result) < 0)
                    {
                        result = item;
                        found = true;
                    }
                }
            }
            return found ? result : default(T);
        }
        public T Floor(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            T result = default(T);
            bool found = false;
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, obj) <= 0)
                {
                    if (!found || _map.Comparator.Compare(item, result) > 0)
                    {
                        result = item;
                        found = true;
                    }
                }
            }
            return found ? result : default(T);
        }
        public T Higher(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            T result = default(T);
            bool found = false;
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, obj) > 0)
                {
                    if (!found || _map.Comparator.Compare(item, result) < 0)
                    {
                        result = item;
                        found = true;
                    }
                }
            }
            return found ? result : default(T);
        }
        public T Lower(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            T result = default(T);
            bool found = false;
            foreach (var item in this)
            {
                if (_map.Comparator.Compare(item, obj) < 0)
                {
                    if (!found || _map.Comparator.Compare(item, result) > 0)
                    {
                        result = item;
                        found = true;
                    }
                }
            }
            return found ? result : default(T);
        }
        public MyTreeSet<T> HeadSet(T upperBound, bool inclusive)
        {
            if (upperBound == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                int cmp = _map.Comparator.Compare(item, upperBound);
                if (cmp < 0 || (inclusive && cmp == 0))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public MyTreeSet<T> SubSet(T lowerBound, bool lowIncl, T upperBound, bool highIncl)
        {
            if (lowerBound == null || upperBound == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                int cmpLow = _map.Comparator.Compare(item, lowerBound);
                int cmpHigh = _map.Comparator.Compare(item, upperBound);
                bool lowOk = cmpLow > 0 || (lowIncl && cmpLow == 0);
                bool highOk = cmpHigh < 0 || (highIncl && cmpHigh == 0);
                if (lowOk && highOk)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public MyTreeSet<T> TailSet(T fromElement, bool inclusive)
        {
            if (fromElement == null)
                throw new ArgumentNullException();
            var result = new MyTreeSet<T>(_map.Comparator);
            foreach (var item in this)
            {
                int cmp = _map.Comparator.Compare(item, fromElement);
                if (cmp > 0 || (inclusive && cmp == 0))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public T PollFirst()
        {
            if (IsEmpty())
                return default(T);
            var first = First();
            Remove(first);
            return first;
        }
        public T PollLast()
        {
            if (IsEmpty())
                return default(T);
            var last = Last();
            Remove(last);
            return last;
        }
        public IEnumerator<T> DescendingIterator()
        {
            var list = new List<T>();
            foreach (var item in this)
            {
                list.Add(item);
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                yield return list[i];
            }
        }
        public MyTreeSet<T> DescendingSet()
        {
            var result = new MyTreeSet<T>(_map.Comparator);
            var list = new List<T>();
            foreach (var item in this)
            {
                list.Add(item);
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                result.Add(list[i]);
            }
            return result;
        }
        //Реализация IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var kvp in _map)
            {
                yield return kvp.Key;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public override string ToString()
        {
            return "[" + string.Join(", ", ToArray()) + "]";
        }
    }
    class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("Тестирование MyTreeSet\n");
                //Базовые операции
                Console.WriteLine("1. Базовые операции:");
                var set = new MyTreeSet<int>();
                Console.WriteLine($"Add(5): {set.Add(5)}");
                Console.WriteLine($"Add(3): {set.Add(3)}");
                Console.WriteLine($"Add(7): {set.Add(7)}");
                Console.WriteLine($"Add(1): {set.Add(1)}");
                Console.WriteLine($"Add(9): {set.Add(9)}");
                Console.WriteLine($"Add(5) again: {set.Add(5)}");
                Console.WriteLine($"Set: {set}");
                Console.WriteLine($"Size: {set.Size()}");
                Console.WriteLine($"Contains(3): {set.Contains(3)}");
                Console.WriteLine($"Contains(10): {set.Contains(10)}");
                //First и Last
                Console.WriteLine($"\n2. First and Last:");
                Console.WriteLine($"First: {set.First()}");
                Console.WriteLine($"Last: {set.Last()}");
                //Поисковые методы
                Console.WriteLine($"\n3. Search methods:");
                Console.WriteLine($"Ceiling(4): {set.Ceiling(4)}");
                Console.WriteLine($"Floor(4): {set.Floor(4)}");
                Console.WriteLine($"Higher(5): {set.Higher(5)}");
                Console.WriteLine($"Lower(5): {set.Lower(5)}");
                //Подмножества
                Console.WriteLine($"\n4. Subsets:");
                Console.WriteLine($"HeadSet(5): {set.HeadSet(5)}");
                Console.WriteLine($"TailSet(5): {set.TailSet(5)}");
                Console.WriteLine($"SubSet(3, 7): {set.SubSet(3, 7)}");
                //Poll методы
                Console.WriteLine($"\n5. Poll methods:");
                Console.WriteLine($"PollFirst: {set.PollFirst()}");
                Console.WriteLine($"After PollFirst: {set}");
                Console.WriteLine($"PollLast: {set.PollLast()}");
                Console.WriteLine($"After PollLast: {set}");
                //Descending iterator
                Console.WriteLine($"\n6. Descending iterator:");
                Console.Write("Elements in reverse order: ");
                var descIter = set.DescendingIterator();
                while (descIter.MoveNext())
                {
                    Console.Write(descIter.Current + " ");
                }
                Console.WriteLine();
                //Массовые операции
                Console.WriteLine($"\n7. Bulk operations:");
                var array = new int[] { 10, 20, 30, 20, 40 };
                var setFromArray = new MyTreeSet<int>(array);
                Console.WriteLine($"Set from array: {setFromArray}");
                var set2 = new MyTreeSet<int>();
                set2.AddAll(new int[] { 100, 200, 300 });
                Console.WriteLine($"After AddAll: {set2}");
                Console.WriteLine($"ContainsAll [100, 200]: {set2.ContainsAll(new int[] { 100, 200 })}");
                //ToArray
                Console.WriteLine($"\n8. ToArray:");
                var arr = set.ToArray();
                Console.WriteLine($"ToArray(): [{string.Join(", ", arr)}]");
                //Clear
                Console.WriteLine($"\n9. Clear:");
                Console.WriteLine($"IsEmpty before Clear: {set2.IsEmpty()}");
                set2.Clear();
                Console.WriteLine($"IsEmpty after Clear: {set2.IsEmpty()}");
                //Error handling
                Console.WriteLine($"\n10. Error handling:");
                try
                {
                    var emptySet = new MyTreeSet<int>();
                    Console.WriteLine($"First on empty set: {emptySet.First()}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                Console.WriteLine("\nТестирование завершено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
            }
        }
    }
}