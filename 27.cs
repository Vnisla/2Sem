using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// =====================================================================
// MyHashMap (из задачи 21)
// =====================================================================
public class MyHashMap<K, V>
{
    private class Node
    {
        public K Key;
        public V Value;
        public int Hash;
        public Node Next;
        public Node(K key, V value, int hash, Node next)
        {
            Key = key; Value = value; Hash = hash; Next = next;
        }
    }

    private Node[] _buckets;
    private int _size;
    private readonly float _loadFactor;
    private int _threshold;

    public MyHashMap() : this(16, 0.75f) { }
    public MyHashMap(int initialCapacity, float loadFactor)
    {
        _buckets = new Node[initialCapacity];
        _loadFactor = loadFactor;
        _threshold = (int)(initialCapacity * loadFactor);
    }

    private int GetHash(K key) => key == null ? 0 : key.GetHashCode();
    private int GetIndex(int hash) => (hash & 0x7FFFFFFF) % _buckets.Length;

    public V Put(K key, V value)
    {
        int hash = GetHash(key);
        int idx = GetIndex(hash);
        Node node = _buckets[idx];
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
        _buckets[idx] = new Node(key, value, hash, _buckets[idx]);
        _size++;
        if (_size > _threshold) Resize();
        return default(V);
    }

    public V Get(K key)
    {
        int hash = GetHash(key);
        int idx = GetIndex(hash);
        Node node = _buckets[idx];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
                return node.Value;
            node = node.Next;
        }
        return default(V);
    }

    public bool ContainsKey(K key) => !Equals(Get(key), default(V));
    public V Remove(K key)
    {
        int hash = GetHash(key);
        int idx = GetIndex(hash);
        Node prev = null;
        Node curr = _buckets[idx];
        while (curr != null)
        {
            if (curr.Hash == hash && Equals(curr.Key, key))
            {
                if (prev == null) _buckets[idx] = curr.Next;
                else prev.Next = curr.Next;
                _size--;
                return curr.Value;
            }
            prev = curr;
            curr = curr.Next;
        }
        return default(V);
    }
    public int Size() => _size;
    public void Clear()
    {
        _buckets = new Node[_buckets.Length];
        _size = 0;
    }
    private void Resize()
    {
        int newCap = _buckets.Length * 2;
        Node[] newBuckets = new Node[newCap];
        foreach (Node bucket in _buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                Node next = node.Next;
                int newIdx = (node.Hash & 0x7FFFFFFF) % newCap;
                node.Next = newBuckets[newIdx];
                newBuckets[newIdx] = node;
                node = next;
            }
        }
        _buckets = newBuckets;
        _threshold = (int)(newCap * _loadFactor);
    }

    public IEnumerable<KeyValuePair<K, V>> EntrySet()
    {
        foreach (Node bucket in _buckets)
        {
            Node node = bucket;
            while (node != null)
            {
                yield return new KeyValuePair<K, V>(node.Key, node.Value);
                node = node.Next;
            }
        }
    }
}

// =====================================================================
// MyHashSet (из задачи 25)
// =====================================================================
public class MyHashSet<E>
{
    private static readonly object PRESENT = new object();
    private readonly MyHashMap<E, object> map;

    public MyHashSet() : this(16, 0.75f) { }
    public MyHashSet(int initialCapacity) : this(initialCapacity, 0.75f) { }
    public MyHashSet(int initialCapacity, float loadFactor)
    {
        map = new MyHashMap<E, object>(initialCapacity, loadFactor);
    }
    public MyHashSet(E[] a) : this()
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (E item in a) Add(item);
    }

    public bool Add(E item) => map.Put(item, PRESENT) == null;
    public void AddAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (E item in a) Add(item);
    }
    public void Clear() => map.Clear();
    public bool Contains(object o)
    {
        if (o is E e) return map.ContainsKey(e);
        return false;
    }
    public bool ContainsAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (E item in a) if (!Contains(item)) return false;
        return true;
    }
    public bool IsEmpty() => map.Size() == 0;
    public bool Remove(object o)
    {
        if (o is E e) return map.Remove(e) != null;
        return false;
    }
    public void RemoveAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (E item in a) Remove(item);
    }
    public void RetainAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        var temp = new HashSet<E>(a);
        var toRemove = new List<E>();
        foreach (var entry in map.EntrySet())
            if (!temp.Contains(entry.Key))
                toRemove.Add(entry.Key);
        foreach (var key in toRemove) Remove(key);
    }
    public int Size() => map.Size();
    public object[] ToArray()
    {
        object[] result = new object[Size()];
        int i = 0;
        foreach (var entry in map.EntrySet())
            result[i++] = entry.Key;
        return result;
    }
    public E[] ToArray(E[] a)
    {
        if (a == null)
        {
            E[] newArr = new E[Size()];
            int i = 0;
            foreach (var entry in map.EntrySet())
                newArr[i++] = entry.Key;
            return newArr;
        }
        int size = Size();
        E[] result = a.Length >= size ? a : new E[size];
        int index = 0;
        foreach (var entry in map.EntrySet())
            result[index++] = entry.Key;
        if (result.Length > size)
            result[size] = default(E);
        return result;
    }

    public IEnumerable<E> Elements()
    {
        foreach (var entry in map.EntrySet())
            yield return entry.Key;
    }
}

// =====================================================================
// Главная программа для задачи 27
// =====================================================================
class Program27
{
    static void Main()
    {
        string filePath = "input.txt";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл input.txt не найден!");
            return;
        }

        var uniqueWords = new MyHashSet<string>();
        Regex wordRegex = new Regex(@"[A-Za-z]+");

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                MatchCollection matches = wordRegex.Matches(line);
                foreach (Match match in matches)
                {
                    string word = match.Value.ToLower();
                    uniqueWords.Add(word);
                }
            }
        }

        Console.WriteLine("Уникальные слова (без учёта регистра):");
        foreach (string word in uniqueWords.Elements())
        {
            Console.WriteLine(word);
        }
    }
}
