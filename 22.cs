using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
public class MySet<T>
{
    private List<T> items = new List<T>();
    public void Add(T item)
    {
        if (!Contains(item))
            items.Add(item);
    }
    public bool Contains(T item) => items.Contains(item);
    public int Size() => items.Count;
    public List<T> ToList() => new List<T>(items);
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
            Key = key; Value = value; Hash = hash; Next = next;
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
        if (initialCapacity <= 0) throw new ArgumentException("Ёмкость >0");
        if (loadFactor <= 0) throw new ArgumentException("loadFactor >0");
        buckets = new Node[initialCapacity];
        this.loadFactor = loadFactor;
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
        int idx = Math.Abs(hash) % buckets.Length;
        Node node = buckets[idx];
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
        int idx = Math.Abs(hash) % buckets.Length;
        Node node = buckets[idx];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
                return node.Value;
            node = node.Next;
        }
        return default(V);
    }
    public bool IsEmpty() => size == 0;
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
        int idx = Math.Abs(hash) % buckets.Length;
        Node node = buckets[idx];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
            {
                V old = node.Value;
                node.Value = value;
                return old;
            }
            node = node.Next;
        }
        buckets[idx] = new Node(key, value, hash, buckets[idx]);
        size++;
        if (size > threshold) Resize();
        return default(V);
    }
    public V Remove(object key)
    {
        int hash = GetHash(key);
        int idx = Math.Abs(hash) % buckets.Length;
        Node prev = null;
        Node curr = buckets[idx];
        while (curr != null)
        {
            if (curr.Hash == hash && Equals(curr.Key, key))
            {
                if (prev == null) buckets[idx] = curr.Next;
                else prev.Next = curr.Next;
                size--;
                return curr.Value;
            }
            prev = curr;
            curr = curr.Next;
        }
        return default(V);
    }
    public int Size() => size;
    private int GetHash(object key) => key == null ? 0 : key.GetHashCode();
    private void Resize()
    {
        int newCap = buckets.Length * 2;
        Node[] newBuckets = new Node[newCap];
        foreach (Node bucket in buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                Node next = node.Next;
                int newIdx = Math.Abs(node.Hash) % newCap;
                node.Next = newBuckets[newIdx];
                newBuckets[newIdx] = node;
                node = next;
            }
        }
        buckets = newBuckets;
        threshold = (int)(newCap * loadFactor);
    }
}
class Program
{
    static void Main()
    {
        string filePath = "input.txt";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл input.txt не найден!");
            return;
        }
        Regex tagRegex = new Regex(@"<\/?[A-Za-z][A-Za-z0-9]*>");
        MyHashMap<string, int> tagCount = new MyHashMap<string, int>();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                MatchCollection matches = tagRegex.Matches(line);
                foreach (Match match in matches)
                {
                    string rawTag = match.Value; 
                    string normalized = rawTag.TrimStart('<').TrimEnd('>');
                    if (normalized.StartsWith("/"))
                        normalized = normalized.Substring(1);
                    normalized = normalized.ToLower();
                    if (tagCount.ContainsKey(normalized))
                    {
                        int old = tagCount.Get(normalized);
                        tagCount.Put(normalized, old + 1);
                    }
                    else
                    {
                        tagCount.Put(normalized, 1);
                    }
                }
            }
        }
        Console.WriteLine("Результаты подсчёта тегов (без учёта регистра и '/') :");
        var entries = tagCount.EntrySet().ToList();
        foreach (var pair in entries)
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }
}