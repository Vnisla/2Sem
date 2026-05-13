using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// =====================================================================
// 1. Интерфейс отображения
// =====================================================================
public interface IMyMap<K, V>
{
    V Get(K key);
    void Put(K key, V value);
    bool ContainsKey(K key);
    V Remove(K key);
    int Size();
    void Clear();
}

// =====================================================================
// 2. MyHashMap (хэш-таблица с цепочками)
// =====================================================================
public class MyHashMap<K, V> : IMyMap<K, V>
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

    public void Put(K key, V value)
    {
        int hash = GetHash(key);
        int idx = GetIndex(hash);
        Node node = _buckets[idx];
        while (node != null)
        {
            if (node.Hash == hash && Equals(node.Key, key))
            {
                node.Value = value;
                return;
            }
            node = node.Next;
        }
        _buckets[idx] = new Node(key, value, hash, _buckets[idx]);
        _size++;
        if (_size > _threshold) Resize();
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
}

// =====================================================================
// 3. MyTreeMap (дерево поиска) – для чистоты используем SortedDictionary,
//    но в реальной работе вы должны подставить своё дерево из задачи 18.
// =====================================================================
public class MyTreeMap<K, V> : IMyMap<K, V>
{
    private readonly SortedDictionary<K, V> _dict;
    public MyTreeMap() => _dict = new SortedDictionary<K, V>();
    public V Get(K key) => _dict.TryGetValue(key, out V val) ? val : default(V);
    public void Put(K key, V value) => _dict[key] = value;
    public bool ContainsKey(K key) => _dict.ContainsKey(key);
    public V Remove(K key)
    {
        if (_dict.TryGetValue(key, out V val))
        {
            _dict.Remove(key);
            return val;
        }
        return default(V);
    }
    public int Size() => _dict.Count;
    public void Clear() => _dict.Clear();
}

// =====================================================================
// 4. Бенчмарк
// =====================================================================
public static class Benchmark
{
    // Генерация уникальных ключей (перемешанные целые числа)
    public static int[] GenerateKeys(int count)
    {
        int[] keys = Enumerable.Range(0, count).ToArray();
        Random rng = new Random(42);
        for (int i = count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (keys[i], keys[j]) = (keys[j], keys[i]);
        }
        return keys;
    }

    // Измерение Put: каждый раз создаётся новая карта
    public static long MeasurePut(Func<IMyMap<int, int>> factory, int[] keys)
    {
        var map = factory();
        Stopwatch sw = Stopwatch.StartNew();
        foreach (int key in keys)
            map.Put(key, key);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    // Измерение Get: карта уже заполнена
    public static long MeasureGet(IMyMap<int, int> map, int[] keys)
    {
        Stopwatch sw = Stopwatch.StartNew();
        foreach (int key in keys)
            _ = map.Get(key);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    // Измерение Remove: карта уже заполнена
    public static long MeasureRemove(IMyMap<int, int> map, int[] keys)
    {
        Stopwatch sw = Stopwatch.StartNew();
        foreach (int key in keys)
            map.Remove(key);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }
}

// =====================================================================
// 5. Главная программа
// =====================================================================
class Program
{
    static void Main()
    {
        // Размеры данных по условию: 10^5, 10^6, 10^7, 10^8
        int[] sizes = { 100_000, 1_000_000, 10_000_000, 100_000_000 };
        int runs = 20;               // количество запусков для усреднения

        Console.WriteLine("=== Сравнение MyHashMap и MyTreeMap ===\n");
        Console.WriteLine($"Будет выполнено {runs} запусков для каждого размера и каждой операции.");
        Console.WriteLine("ВНИМАНИЕ: для размера 100 млн (10^8) требуется много памяти и времени.\n");

        // Фабрики для создания чистых карт
        var hashMapFactory = () => new MyHashMap<int, int>();
        var treeMapFactory = () => new MyTreeMap<int, int>();

        // Структура для хранения результатов: [операция][размер][тип_карты] = среднее время (мс)
        var results = new Dictionary<string, Dictionary<int, Dictionary<string, double>>>();

        // Инициализация
        string[] operations = { "Put", "Get", "Remove" };
        foreach (var op in operations)
            results[op] = new Dictionary<int, Dictionary<string, double>>();

        // Прогрев JIT
        var warmup = new MyHashMap<int, int>();
        warmup.Put(1, 1);
        warmup.Get(1);
        warmup.Remove(1);

        // Основной цикл по размерам
        foreach (int size in sizes)
        {
            Console.WriteLine($"\n=== Размер: {size:N0} элементов ===");
            int[] keys = Benchmark.GenerateKeys(size);  // уникальные ключи

            // ---- 1. Put ----
            long sumPutHashMap = 0, sumPutTreeMap = 0;
            for (int run = 0; run < runs; run++)
            {
                sumPutHashMap += Benchmark.MeasurePut(hashMapFactory, keys);
                sumPutTreeMap += Benchmark.MeasurePut(treeMapFactory, keys);
            }
            double avgPutHashMap = sumPutHashMap / (double)runs;
            double avgPutTreeMap = sumPutTreeMap / (double)runs;
            results["Put"][size] = new Dictionary<string, double>
            {
                ["HashMap"] = avgPutHashMap,
                ["TreeMap"] = avgPutTreeMap
            };
            Console.WriteLine($"Put   -> HashMap: {avgPutHashMap:F2} ms, TreeMap: {avgPutTreeMap:F2} ms");

            // ---- 2. Get ----
            // Создаём заполненные карты (один раз, так как Get не изменяет структуру)
            var hashMapFilled = new MyHashMap<int, int>();
            var treeMapFilled = new MyTreeMap<int, int>();
            foreach (int key in keys)
            {
                hashMapFilled.Put(key, key);
                treeMapFilled.Put(key, key);
            }

            long sumGetHashMap = 0, sumGetTreeMap = 0;
            for (int run = 0; run < runs; run++)
            {
                sumGetHashMap += Benchmark.MeasureGet(hashMapFilled, keys);
                sumGetTreeMap += Benchmark.MeasureGet(treeMapFilled, keys);
            }
            double avgGetHashMap = sumGetHashMap / (double)runs;
            double avgGetTreeMap = sumGetTreeMap / (double)runs;
            results["Get"][size] = new Dictionary<string, double>
            {
                ["HashMap"] = avgGetHashMap,
                ["TreeMap"] = avgGetTreeMap
            };
            Console.WriteLine($"Get   -> HashMap: {avgGetHashMap:F2} ms, TreeMap: {avgGetTreeMap:F2} ms");

            // ---- 3. Remove ----
            // Для Remove каждый замер требует новой заполненной карты, т.к. элементы удаляются
            long sumRemoveHashMap = 0, sumRemoveTreeMap = 0;
            for (int run = 0; run < runs; run++)
            {
                // Создаём и заполняем
                var hm = new MyHashMap<int, int>();
                var tm = new MyTreeMap<int, int>();
                foreach (int key in keys)
                {
                    hm.Put(key, key);
                    tm.Put(key, key);
                }
                sumRemoveHashMap += Benchmark.MeasureRemove(hm, keys);
                sumRemoveTreeMap += Benchmark.MeasureRemove(tm, keys);
            }
            double avgRemoveHashMap = sumRemoveHashMap / (double)runs;
            double avgRemoveTreeMap = sumRemoveTreeMap / (double)runs;
            results["Remove"][size] = new Dictionary<string, double>
            {
                ["HashMap"] = avgRemoveHashMap,
                ["TreeMap"] = avgRemoveTreeMap
            };
            Console.WriteLine($"Remove -> HashMap: {avgRemoveHashMap:F2} ms, TreeMap: {avgRemoveTreeMap:F2} ms");
        }

        // Сохранение результатов в CSV для построения графиков
        string csvPath = "benchmark_results.csv";
        using (var writer = new StreamWriter(csvPath))
        {
            writer.WriteLine("Operation,Size,HashMap_ms,TreeMap_ms");
            foreach (var op in operations)
            {
                foreach (var size in sizes)
                {
                    double hashVal = results[op][size]["HashMap"];
                    double treeVal = results[op][size]["TreeMap"];
                    writer.WriteLine($"{op},{size},{hashVal},{treeVal}");
                }
            }
        }

        Console.WriteLine($"\nРезультаты сохранены в файл: {csvPath}");
        Console.WriteLine("Для построения графиков откройте этот файл в Excel или любом другом инструменте.");
        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
