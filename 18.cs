using System;
using System.Collections.Generic;
public class MyTreeMap<K, V> where K : IComparable<K>
{
    private class Node
    {
        public K Key { get; set; }
        public V Value { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }
        public Node(K key, V value)
        {
            Key = key;
            Value = value;
            Left = null;
            Right = null;
        }
    }
    private Node root;
    private int size;
    private readonly IComparer<K> comparator;
    public MyTreeMap() : this(Comparer<K>.Default) { }
    public MyTreeMap(IComparer<K> comp)
    {
        comparator = comp ?? Comparer<K>.Default;
        root = null;
        size = 0;
    }
    public void Clear()
    {
        root = null;
        size = 0;
    }
    public bool ContainsKey(K key)
    {
        return FindNode(key) != null;
    }
    public bool ContainsValue(V value)
    {
        return ContainsValue(root, value);
    }
    private bool ContainsValue(Node node, V value)
    {
        if (node == null) return false;
        if (EqualityComparer<V>.Default.Equals(node.Value, value))
            return true;
        return ContainsValue(node.Left, value) || ContainsValue(node.Right, value);
    }
    public ISet<KeyValuePair<K, V>> EntrySet()
    {
        var set = new HashSet<KeyValuePair<K, V>>();
        InOrderTraversal(root, node => set.Add(new KeyValuePair<K, V>(node.Key, node.Value)));
        return set;
    }
    public V Get(K key)
    {
        var node = FindNode(key);
        return node != null ? node.Value : default(V);
    }
    public bool IsEmpty()
    {
        return size == 0;
    }
    public ISet<K> KeySet()
    {
        var set = new HashSet<K>();
        InOrderTraversal(root, node => set.Add(node.Key));
        return set;
    }
    public V Put(K key, V value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var oldValue = default(V);
        root = Put(root, key, value, ref oldValue);
        return oldValue;
    }
    private Node Put(Node node, K key, V value, ref V oldValue)
    {
        if (node == null)
        {
            size++;
            return new Node(key, value);
        }
        int cmp = comparator.Compare(key, node.Key);
        if (cmp < 0)
        {
            node.Left = Put(node.Left, key, value, ref oldValue);
        }
        else if (cmp > 0)
        {
            node.Right = Put(node.Right, key, value, ref oldValue);
        }
        else
        {
            oldValue = node.Value;
            node.Value = value;
        }
        return node;
    }
    public V Remove(K key)
    {
        var removedValue = default(V);
        root = Remove(root, key, ref removedValue);
        if (!EqualityComparer<V>.Default.Equals(removedValue, default(V)))
            size--;
        return removedValue;
    }
    private Node Remove(Node node, K key, ref V value)
    {
        if (node == null) return null;
        int cmp = comparator.Compare(key, node.Key);
        if (cmp < 0)
        {
            node.Left = Remove(node.Left, key, ref value);
        }
        else if (cmp > 0)
        {
            node.Right = Remove(node.Right, key, ref value);
        }
        else
        {
            value = node.Value;
            if (node.Left == null) return node.Right;
            if (node.Right == null) return node.Left;
            var minNode = FindMin(node.Right);
            node.Key = minNode.Key;
            node.Value = minNode.Value;
            node.Right = RemoveMin(node.Right);
        }
        return node;
    }
    private Node RemoveMin(Node node)
    {
        if (node.Left == null) return node.Right;
        node.Left = RemoveMin(node.Left);
        return node;
    }
    public int Size()
    {
        return size;
    }
    public K FirstKey()
    {
        if (root == null)
            throw new InvalidOperationException("Tree is empty");
        return FindMin(root).Key;
    }
    public K LastKey()
    {
        if (root == null)
            throw new InvalidOperationException("Tree is empty");
        return FindMax(root).Key;
    }
    public MyTreeMap<K, V> HeadMap(K end)
    {
        var map = new MyTreeMap<K, V>(comparator);
        BuildHeadMap(root, end, map);
        return map;
    }
    private void BuildHeadMap(Node node, K end, MyTreeMap<K, V> map)
    {
        if (node == null) return;

        if (comparator.Compare(node.Key, end) < 0)
        {
            map.Put(node.Key, node.Value);
            BuildHeadMap(node.Left, end, map);
            BuildHeadMap(node.Right, end, map);
        }
        else
        {
            BuildHeadMap(node.Left, end, map);
        }
    }
    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        var map = new MyTreeMap<K, V>(comparator);
        BuildSubMap(root, start, end, map);
        return map;
    }
    private void BuildSubMap(Node node, K start, K end, MyTreeMap<K, V> map)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0 && comparator.Compare(node.Key, end) < 0)
        {
            map.Put(node.Key, node.Value);
        }
        if (comparator.Compare(node.Key, start) > 0)
        {
            BuildSubMap(node.Left, start, end, map);
        }
        if (comparator.Compare(node.Key, end) < 0)
        {
            BuildSubMap(node.Right, start, end, map);
        }
    }
    public MyTreeMap<K, V> TailMap(K start)
    {
        var map = new MyTreeMap<K, V>(comparator);
        BuildTailMap(root, start, map);
        return map;
    }
    private void BuildTailMap(Node node, K start, MyTreeMap<K, V> map)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0)
        {
            map.Put(node.Key, node.Value);
            BuildTailMap(node.Right, start, map);
        }
        BuildTailMap(node.Left, start, map);
    }
    public KeyValuePair<K, V>? LowerEntry(K key)
    {
        var node = FindLowerNode(root, key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }
    public KeyValuePair<K, V>? FloorEntry(K key)
    {
        var node = FindFloorNode(root, key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }
    public KeyValuePair<K, V>? HigherEntry(K key)
    {
        var node = FindHigherNode(root, key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }
    public KeyValuePair<K, V>? CeilingEntry(K key)
    {
        var node = FindCeilingNode(root, key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }
    public K LowerKey(K key)
    {
        var node = FindLowerNode(root, key);
        return node != null ? node.Key : default(K);
    }
    public K FloorKey(K key)
    {
        var node = FindFloorNode(root, key);
        return node != null ? node.Key : default(K);
    }
    public K HigherKey(K key)
    {
        var node = FindHigherNode(root, key);
        return node != null ? node.Key : default(K);
    }
    public K CeilingKey(K key)
    {
        var node = FindCeilingNode(root, key);
        return node != null ? node.Key : default(K);
    }
    public KeyValuePair<K, V>? PollFirstEntry()
    {
        if (root == null) return null;
        var minNode = FindMin(root);
        var entry = new KeyValuePair<K, V>(minNode.Key, minNode.Value);
        Remove(minNode.Key);
        return entry;
    }
    public KeyValuePair<K, V>? PollLastEntry()
    {
        if (root == null) return null;
        var maxNode = FindMax(root);
        var entry = new KeyValuePair<K, V>(maxNode.Key, maxNode.Value);
        Remove(maxNode.Key);
        return entry;
    }
    public KeyValuePair<K, V>? FirstEntry()
    {
        if (root == null) return null;
        var minNode = FindMin(root);
        return new KeyValuePair<K, V>(minNode.Key, minNode.Value);
    }
    public KeyValuePair<K, V>? LastEntry()
    {
        if (root == null) return null;

        var maxNode = FindMax(root);
        return new KeyValuePair<K, V>(maxNode.Key, maxNode.Value);
    }
    private Node FindNode(K key)
    {
        var current = root;
        while (current != null)
        {
            int cmp = comparator.Compare(key, current.Key);
            if (cmp == 0) return current;
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }
    private Node FindMin(Node node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }
    private Node FindMax(Node node)
    {
        while (node.Right != null)
            node = node.Right;
        return node;
    }
    private Node FindLowerNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            if (comparator.Compare(node.Key, key) < 0)
            {
                result = node;
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }
        return result;
    }
    private Node FindFloorNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            int cmp = comparator.Compare(node.Key, key);
            if (cmp <= 0)
            {
                result = node;
                if (cmp == 0) break;
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }
        return result;
    }
    private Node FindHigherNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            if (comparator.Compare(node.Key, key) > 0)
            {
                result = node;
                node = node.Left;
            }
            else
            {
                node = node.Right;
            }
        }
        return result;
    }
    private Node FindCeilingNode(Node node, K key)
    {
        Node result = null;
        while (node != null)
        {
            int cmp = comparator.Compare(node.Key, key);
            if (cmp >= 0)
            {
                result = node;
                if (cmp == 0) break;
                node = node.Left;
            }
            else
            {
                node = node.Right;
            }
        }
        return result;
    }
    private void InOrderTraversal(Node node, Action<Node> action)
    {
        if (node == null) return;
        InOrderTraversal(node.Left, action);
        action(node);
        InOrderTraversal(node.Right, action);
    }
    public override string ToString()
    {
        var entries = new List<string>();
        InOrderTraversal(root, node => entries.Add($"{node.Key}={node.Value}"));
        return $"{{{string.Join(", ", entries)}}}";
    }
}
public class Program
{
    public static void Main()
    {
        var map = new MyTreeMap<int, string>();
        map.Put(3, "Three");
        map.Put(1, "One");
        map.Put(4, "Four");
        map.Put(2, "Two");
        Console.WriteLine($"Map: {map}");
        Console.WriteLine($"Size: {map.Size()}");
        Console.WriteLine($"Contains key 2: {map.ContainsKey(2)}");
        Console.WriteLine($"Get key 2: {map.Get(2)}");
        Console.WriteLine($"First key: {map.FirstKey()}");
        Console.WriteLine($"Last key: {map.LastKey()}");
        var headMap = map.HeadMap(3);
        Console.WriteLine($"HeadMap(<3): {headMap}");
        var tailMap = map.TailMap(2);
        Console.WriteLine($"TailMap(>=2): {tailMap}");
        var subMap = map.SubMap(2, 4);
        Console.WriteLine($"SubMap(2<=key<4): {subMap}");
        Console.WriteLine($"Lower entry than 3: {map.LowerEntry(3)}");
        Console.WriteLine($"Floor entry for 3: {map.FloorEntry(3)}");
        Console.WriteLine($"Higher entry than 2: {map.HigherEntry(2)}");
        Console.WriteLine($"Ceiling entry for 2: {map.CeilingEntry(2)}");
        var removed = map.Remove(2);
        Console.WriteLine($"Removed value for key 2: {removed}");
        Console.WriteLine($"After removal: {map}");
    }
}