using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
//MyHashMap 
public class MyHashMap<K, V>
{
    private class Entry
    {
        public K Key;
        public V Value;
        public Entry Next;
        public Entry(K key, V value) { Key = key; Value = value; Next = null; }
    }
    private Entry[] table;
    private int size;
    private float loadFactor;
    private int threshold;
    public MyHashMap() : this(16, 0.75f) { }
    public MyHashMap(int initialCapacity) : this(initialCapacity, 0.75f) { }
    public MyHashMap(int initialCapacity, float loadFactor)
    {
        if (initialCapacity <= 0) throw new ArgumentException("Initial capacity must be positive");
        if (loadFactor <= 0 || float.IsNaN(loadFactor)) throw new ArgumentException("Load factor must be positive");
        this.loadFactor = loadFactor;
        table = new Entry[initialCapacity];
        threshold = (int)(initialCapacity * loadFactor);
        size = 0;
    }
    private int GetBucketIndex(K key)
    {
        int hashCode = key?.GetHashCode() ?? 0;
        return (hashCode & 0x7FFFFFFF) % table.Length;
    }
    public void Put(K key, V value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        int index = GetBucketIndex(key);
        Entry current = table[index];
        while (current != null)
        {
            if (current.Key.Equals(key))
            {
                current.Value = value;
                return;
            }
            current = current.Next;
        }
        Entry newEntry = new Entry(key, value);
        newEntry.Next = table[index];
        table[index] = newEntry;
        size++;
        if (size >= threshold) Resize();
    }
    public V Get(K key)
    {
        if (key == null) return default(V);
        int index = GetBucketIndex(key);
        Entry current = table[index];
        while (current != null)
        {
            if (current.Key.Equals(key)) return current.Value;
            current = current.Next;
        }
        return default(V);
    }
    public bool ContainsKey(K key)
    {
        if (key == null) return false;
        int index = GetBucketIndex(key);
        Entry current = table[index];
        while (current != null)
        {
            if (current.Key.Equals(key)) return true;
            current = current.Next;
        }
        return false;
    }
    public int Size() => size;
    public void Clear()
    {
        for (int i = 0; i < table.Length; i++) table[i] = null;
        size = 0;
    }
    private void Resize()
    {
        int newCapacity = table.Length * 2;
        Entry[] oldTable = table;
        table = new Entry[newCapacity];
        size = 0;
        threshold = (int)(newCapacity * loadFactor);
        for (int i = 0; i < oldTable.Length; i++)
        {
            Entry current = oldTable[i];
            while (current != null)
            {
                Put(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
    public IEnumerable<K> Keys()
    {
        for (int i = 0; i < table.Length; i++)
        {
            Entry current = table[i];
            while (current != null)
            {
                yield return current.Key;
                current = current.Next;
            }
        }
    }
}
public enum VariableType { Int, Float, Double }

public class VariableInfo
{
    public VariableType Type { get; set; }
    public string Value { get; set; }
}
class Program23
{
    static void Main()
    {
        string inputFile = "input.txt";
        string outputFile = "output.txt";
        string errorsFile = "errors.txt";
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Файл {inputFile} не найден.");
            return;
        }
        string content = File.ReadAllText(inputFile);
        Regex definitionRegex = new Regex(
            @"\b(int|float|double)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+)\s*;",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        var matches = definitionRegex.Matches(content);
        var map = new MyHashMap<string, VariableInfo>();
        var duplicateNames = new HashSet<string>();
        var invalidDefinitions = new List<string>();
        foreach (Match match in matches)
        {
            string typeStr = match.Groups[1].Value.ToLower();
            string varName = match.Groups[2].Value;
            string valueStr = match.Groups[3].Value;
            VariableType type;
            switch (typeStr)
            {
                case "int": type = VariableType.Int; break;
                case "float": type = VariableType.Float; break;
                case "double": type = VariableType.Double; break;
                default: continue;
            }
            if (map.ContainsKey(varName))
                duplicateNames.Add(varName);
            else
                map.Put(varName, new VariableInfo { Type = type, Value = valueStr });
        }
        string[] rawDefinitions = content.Split(';');
        foreach (string def in rawDefinitions)
        {
            if (string.IsNullOrWhiteSpace(def)) continue;
            if (!definitionRegex.IsMatch(def + ";"))
                invalidDefinitions.Add(def.Trim());
        }
        using (StreamWriter sw = new StreamWriter(outputFile))
        {
            foreach (var key in map.Keys())
            {
                var info = map.Get(key);
                string typeName = info.Type.ToString().ToLower();
                sw.WriteLine($"{typeName} => {key}({info.Value})");
            }
        }
        using (StreamWriter sw = new StreamWriter(errorsFile))
        {
            if (duplicateNames.Count > 0)
            {
                sw.WriteLine("Переопределения переменных:");
                foreach (var name in duplicateNames)
                    sw.WriteLine(name);
            }
            if (invalidDefinitions.Count > 0)
            {
                sw.WriteLine("Некорректные определения:");
                foreach (var def in invalidDefinitions)
                    sw.WriteLine(def);
            }
            if (duplicateNames.Count == 0 && invalidDefinitions.Count == 0)
                sw.WriteLine("Ошибок не найдено.");
        }
        Console.WriteLine("Обработка завершена.");
        Console.WriteLine($"Результаты: {outputFile}");
        Console.WriteLine($"Ошибки: {errorsFile}");
    }
}