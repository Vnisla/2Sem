using System;
using System.Collections.Generic;
public class MySet<T>
{
    private List<T> items = new List<T>();
    public void Add(T item)
    {
        if (!Contains(item))
            items.Add(item);
    }
    public bool Contains(T item)
    {
        return items.Contains(item);
    }
    public int Size()
    {
        return items.Count;
    }
    public bool IsEmpty()
    {
        return items.Count == 0;
    }
    public List<T> ToList()
    {
        return new List<T>(items);
    }
}
public class MyHashMap<K, V>
{
   
    private class Node
    {
        public K Key;
        public V Value;
        public Node Next;
        public int Hash;
        public Node(K key, V value, int hash, Node next)
        {
            Key = key;
            Value = value;
            Hash = hash;
            Next = next;
        }
    }
    private Node[] buckets;
    private int size;
    private float loadFactor;
    private int threshold;
    public MyHashMap() : this(16, 0.75f) { }
    public MyHashMap(int initialCapacity) : this(initialCapacity, 0.75f) { }
    public MyHashMap(int initialCapacity, float loadFactor)
    {
        if (initialCapacity <= 0)
            throw new ArgumentException("Начальная ёмкость должна быть > 0");
        if (loadFactor <= 0)
            throw new ArgumentException("loadFactor должен быть > 0");
        this.loadFactor = loadFactor;
        buckets = new Node[initialCapacity];
        threshold = (int)(initialCapacity * loadFactor);
        size = 0;
    }
    public void Clear()
    {
        buckets = new Node[buckets.Length];
        size = 0;
    }
    public bool ContainsKey(object key)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);
        Node node = buckets[index];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
                return true;
            node = node.Next;
        }
        return false;
    }
    public bool ContainsValue(object value)
    {
        foreach (Node bucket in buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                if (Equals(node.Value, value))
                    return true;
                node = node.Next;
            }
        }
        return false;
    }
    public MySet<KeyValuePair<K, V>> EntrySet()
    {
        MySet<KeyValuePair<K, V>> set = new MySet<KeyValuePair<K, V>>();
        foreach (Node bucket in buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                set.Add(new KeyValuePair<K, V>(node.Key, node.Value));
                node = node.Next;
            }
        }
        return set;
    }
    public V Get(object key)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);
        Node node = buckets[index];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
                return node.Value;
            node = node.Next;
        }
        return default(V);
    }
    public bool IsEmpty()
    {
        return size == 0;
    }
    public MySet<K> KeySet()
    {
        MySet<K> set = new MySet<K>();
        foreach (Node bucket in buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                set.Add(node.Key);
                node = node.Next;
            }
        }
        return set;
    }
    public V Put(K key, V value)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);
        Node node = buckets[index];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
            {
                V oldValue = node.Value;
                node.Value = value;
                return oldValue;
            }
            node = node.Next;
        }
        buckets[index] = new Node(key, value, hash, buckets[index]);
        size++;
        if (size > threshold)
            Resize();
        return default(V);
    }
    public V Remove(object key)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);
        Node prev = null;
        Node curr = buckets[index];
        while (curr != null)
        {
            if (curr.Hash == hash && Equals(curr.Key, key))
            {
                if (prev == null)
                    buckets[index] = curr.Next;
                else
                    prev.Next = curr.Next;
                size--;
                return curr.Value;
            }
            prev = curr;
            curr = curr.Next;
        }
        return default(V);
    }
    public int Size()
    {
        return size;
    }
    private int GetHash(object key)
    {
        return key == null ? 0 : key.GetHashCode();
    }
    private int GetIndex(int hash)
    {
        return Math.Abs(hash) % buckets.Length;
    }
    private void Resize()
    {
        int newCapacity = buckets.Length * 2;
        Node[] newBuckets = new Node[newCapacity];
        foreach (Node bucket in buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                Node next = node.Next;
                int newIndex = Math.Abs(node.Hash) % newCapacity;
                node.Next = newBuckets[newIndex];
                newBuckets[newIndex] = node;
                node = next;
            }
        }
        buckets = newBuckets;
        threshold = (int)(newCapacity * loadFactor);
    }
}
class Program
{
    static void Main()
    {
        Console.WriteLine("Тест MyHashMap\n");
        MyHashMap<string, int> map = new MyHashMap<string, int>();
        map.Put("один", 1);
        map.Put("два", 2);
        map.Put("три", 3);
        map.Put("два", 22);
        Console.WriteLine($"Размер: {map.Size()}");         
        Console.WriteLine($"Пусто? {map.IsEmpty()}");       
        Console.WriteLine($"Ключ 'два' есть? {map.ContainsKey("два")}");
        Console.WriteLine($"Значение 22 есть? {map.ContainsValue(22)}"); 
        Console.WriteLine($"Значение для 'один': {map.Get("один")}"); 
        int removed = map.Remove("два");
        Console.WriteLine($"Удалено значение: {removed}");
        Console.WriteLine($"Размер после удаления: {map.Size()}"); 
        Console.Write("\nКлючи: ");
        foreach (var k in map.KeySet().ToList())
            Console.Write(k + " ");  
        Console.WriteLine();
        Console.WriteLine("Пары:");
        foreach (var pair in map.EntrySet().ToList())
            Console.WriteLine($"  {pair.Key} -> {pair.Value}");
        map.Clear();
        Console.WriteLine($"\nПосле очистки размер: {map.Size()}");
        Console.WriteLine($"Пусто? {map.IsEmpty()}");
    }
}