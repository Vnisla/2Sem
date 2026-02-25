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
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        return FindNode(key) != null;
    }
    public bool ContainsValue(V value)
    {
        return ContainsValue(root, value);
    }
    public ISet<KeyValuePair<K, V>> EntrySet()
    {
        var set = new HashSet<KeyValuePair<K, V>>();
        InOrderTraversal(root, node =>
            set.Add(new KeyValuePair<K, V>(node.Key, node.Value)));
        return set;
    }
    public V Get(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
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
    public V Remove(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var removedValue = default(V);
        root = Remove(root, key, ref removedValue);
        if (!EqualityComparer<V>.Default.Equals(removedValue, default(V)))
            size--;
        return removedValue;
    }
    public int Size()
    {
        return size;
    }
    //возврат первого ключа
    public K FirstKey()
    {
        if (root == null)
            throw new InvalidOperationException("Tree is empty");

        return FindMin(root).Key;
    }
    //возврат последнего ключа
    public K LastKey()
    {
        if (root == null)
            throw new InvalidOperationException("Tree is empty");
        return FindMax(root).Key;
    }
    //отображение с ключами меньше e
    public MyTreeMap<K, V> HeadMap(K end)
    {
        if (end == null)
            throw new ArgumentNullException(nameof(end));
        var map = new MyTreeMap<K, V>(comparator);
        BuildHeadMap(root, end, map);
        return map;
    }
    //отображение с ключами от s до e
    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        if (start == null)
            throw new ArgumentNullException(nameof(start));
        if (end == null)
            throw new ArgumentNullException(nameof(end));
        var map = new MyTreeMap<K, V>(comparator);
        BuildSubMap(root, start, end, map);
        return map;
    }
    //отображение с ключами больше start
    public MyTreeMap<K, V> TailMap(K start)
    {
        if (start == null)
            throw new ArgumentNullException(nameof(start));
        var map = new MyTreeMap<K, V>(comparator);
        BuildTailMap(root, start, map);
        return map;
    }
    //пара с ключом меньше заданного
    public KeyValuePair<K, V>? LowerEntry(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindLowerNode(root, key);
        return node != null
            ? new KeyValuePair<K, V>(node.Key, node.Value)
            : (KeyValuePair<K, V>?)null;
    }
    //пара с ключом меньше или равным заданному
    public KeyValuePair<K, V>? FloorEntry(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindFloorNode(root, key);
        return node != null
            ? new KeyValuePair<K, V>(node.Key, node.Value)
            : (KeyValuePair<K, V>?)null;
    }
    //пара с ключом больше заданного
    public KeyValuePair<K, V>? HigherEntry(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindHigherNode(root, key);
        return node != null
            ? new KeyValuePair<K, V>(node.Key, node.Value)
            : (KeyValuePair<K, V>?)null;
    }
    //пара с ключом больше или равным заданному
    public KeyValuePair<K, V>? CeilingEntry(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindCeilingNode(root, key);
        return node != null
            ? new KeyValuePair<K, V>(node.Key, node.Value)
            : (KeyValuePair<K, V>?)null;
    }
    //ключ меньше заданного
    public K LowerKey(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindLowerNode(root, key);
        return node != null ? node.Key : default(K);
    }
    //ключ меньше или равен заданному
    public K FloorKey(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindFloorNode(root, key);
        return node != null ? node.Key : default(K);
    }
    //ключ больше заданного
    public K HigherKey(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindHigherNode(root, key);
        return node != null ? node.Key : default(K);
    }
    //ключ больше или равен задан.
    public K CeilingKey(K key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        var node = FindCeilingNode(root, key);
        return node != null ? node.Key : default(K);
    }
    //удаление и возврат первого элемента
    public KeyValuePair<K, V>? PollFirstEntry()
    {
        if (root == null) return null;
        var minNode = FindMin(root);
        var entry = new KeyValuePair<K, V>(minNode.Key, minNode.Value);
        Remove(minNode.Key);
        return entry;
    }
    //удаление и возврат последнего элемента
    public KeyValuePair<K, V>? PollLastEntry()
    {
        if (root == null) return null;
        var maxNode = FindMax(root);
        var entry = new KeyValuePair<K, V>(maxNode.Key, maxNode.Value);
        Remove(maxNode.Key);
        return entry;
    }
    //возврат первого элемента без удаления
    public KeyValuePair<K, V>? FirstEntry()
    {
        if (root == null) return null;
        var minNode = FindMin(root);
        return new KeyValuePair<K, V>(minNode.Key, minNode.Value);
    }
    //возврат последнего элемента без удаления
    public KeyValuePair<K, V>? LastEntry()
    {
        if (root == null) return null;

        var maxNode = FindMax(root);
        return new KeyValuePair<K, V>(maxNode.Key, maxNode.Value);
    }

    //Поиск узла по ключу
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
    //Рекурсивный поиск
    private bool ContainsValue(Node node, V value)
    {
        if (node == null) return false;

        if (EqualityComparer<V>.Default.Equals(node.Value, value))
            return true;
        return ContainsValue(node.Left, value) || ContainsValue(node.Right, value);
    }
    //Рекурсивная вставка
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
    //Рекурсивное удаление
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
            //1: нет левого сына
            if (node.Left == null) return node.Right;
            //2:нет правого сына
            if (node.Right == null) return node.Left;
            //3: оба потомка
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
    private Node FindMin(Node node)
    {
        if (node == null) return null;
        while (node.Left != null)
            node = node.Left;
        return node;
    }
    private Node FindMax(Node node)
    {
        if (node == null) return null;
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
    //Построение headMap
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
    //Построение subMap
    private void BuildSubMap(Node node, K start, K end, MyTreeMap<K, V> map)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0 &&
            comparator.Compare(node.Key, end) < 0)
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
    //Построение tailMap
    private void BuildTailMap(Node node, K start, MyTreeMap<K, V> map)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0)
        {
            map.Put(node.Key, node.Value);
            BuildTailMap(node.Left, start, map);
            BuildTailMap(node.Right, start, map);
        }
        else
        {
            BuildTailMap(node.Right, start, map);
        }
    }
    //Сим. обход дерева
    private void InOrderTraversal(Node node, Action<Node> action)
    {
        if (node == null) return;
        InOrderTraversal(node.Left, action);
        action(node);
        InOrderTraversal(node.Right, action);
    }
    //Переопределение ToString для удобного вывода
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
        Console.WriteLine("Создано пустое отображение");
        map.Put(5, "five");
        map.Put(3, "three");
        map.Put(7, "seven");
        map.Put(1, "one");
        map.Put(9, "nine");
        map.Put(4, "four");
        Console.WriteLine($"После добавления: {map}");
        Console.WriteLine($"12. Size: {map.Size()}"); 
        //Первый и последний ключи
        Console.WriteLine($"13. FirstKey: {map.FirstKey()}"); 
        Console.WriteLine($"14. LastKey: {map.LastKey()}");  
        //Получение значения
        Console.WriteLine($"7. Get(5): {map.Get(5)}");
        //Проверка наличия
        Console.WriteLine($"4. ContainsKey(3): {map.ContainsKey(3)}"); 
        Console.WriteLine($"5. ContainsValue('seven'): {map.ContainsValue("seven")}"); 
        //Множества ключей и пар
        Console.Write("9. KeySet: ");
        foreach (var key in map.KeySet())
            Console.Write(key + " "); 
        Console.WriteLine();
        Console.WriteLine("6. EntrySet: " + map); 
        //Поиск ближайших элементов
        Console.WriteLine($"18. LowerEntry(5): {map.LowerEntry(5)}"); 
        Console.WriteLine($"19. FloorEntry(5): {map.FloorEntry(5)}");   
        Console.WriteLine($"20. HigherEntry(5): {map.HigherEntry(5)}");
        Console.WriteLine($"21. CeilingEntry(6): {map.CeilingEntry(6)}"); 
        //Поиск ближайших ключей
        Console.WriteLine($"22. LowerKey(5): {map.LowerKey(5)}");   
        Console.WriteLine($"23. FloorKey(5): {map.FloorKey(5)}"); 
        Console.WriteLine($"24. HigherKey(5): {map.HigherKey(5)}");
        Console.WriteLine($"25. CeilingKey(6): {map.CeilingKey(6)}"); 
        //Подмножества
        var headMap = map.HeadMap(5);
        Console.WriteLine($"15. HeadMap(<5): {headMap}");
        var subMap = map.SubMap(3, 7);
        Console.WriteLine($"16. SubMap(3<=key<7): {subMap}"); 
        var tailMap = map.TailMap(5);
        Console.WriteLine($"17. TailMap(>=5): {tailMap}"); 
        //Первый/последний без удаления
        Console.WriteLine($"28. FirstEntry: {map.FirstEntry()}"); 
        Console.WriteLine($"29. LastEntry: {map.LastEntry()}");   
        //Удаление первого/последнего
        var first = map.PollFirstEntry();
        Console.WriteLine($"26. PollFirstEntry удалил: {first}"); 
        Console.WriteLine($"После PollFirstEntry: {map}"); 
        var last = map.PollLastEntry();
        Console.WriteLine($"27. PollLastEntry удалил: {last}"); 
        Console.WriteLine($"После PollLastEntry: {map}"); 
        //Удаление по ключу
        var removed = map.Remove(5);
        Console.WriteLine($"11. Remove(5) удалил: {removed}");
        Console.WriteLine($"После Remove: {map}"); 
        //Проверка на пустоту
        Console.WriteLine($"8. IsEmpty? {map.IsEmpty()}"); 
        //Очистка
        map.Clear();
        Console.WriteLine($"3. После Clear, Size: {map.Size()}"); 
        Console.WriteLine($"8. IsEmpty? {map.IsEmpty()}"); 
    }
}
