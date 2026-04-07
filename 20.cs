using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    static void Main()
    {
        Console.WriteLine(" 6 Топологическая сортировка Тарьян");
        var ts = new TopologicalSort(6);
        ts.AddEdge(5, 0);
        ts.AddEdge(4, 0);
        ts.AddEdge(4, 1);
        ts.AddEdge(3, 1);
        ts.AddEdge(2, 3);
        ts.AddEdge(2, 1);
        var order = ts.Sort();
        if (order != null)
            Console.WriteLine("Топологический порядок: " + string.Join(", ", order));
        else
            Console.WriteLine("Граф содержит цикл");
        Console.WriteLine("\n 12 Максимальный поток (проталкивание предпотока)");
        var flow = new PushRelabel(6);
        flow.AddEdge(0, 1, 16);
        flow.AddEdge(0, 2, 13);
        flow.AddEdge(1, 2, 10);
        flow.AddEdge(1, 3, 12);
        flow.AddEdge(2, 1, 4);
        flow.AddEdge(2, 4, 14);
        flow.AddEdge(3, 2, 9);
        flow.AddEdge(3, 5, 20);
        flow.AddEdge(4, 3, 7);
        flow.AddEdge(4, 5, 4);
        long maxFlow = flow.MaxFlow(0, 5);
        Console.WriteLine($"Максимальный поток: {maxFlow}");
        Console.WriteLine("\n 18 Проверка изоморфности графов ");
        var g1 = new GraphIsomorphism(4);
        g1.AddEdge(0, 1);
        g1.AddEdge(0, 2);
        g1.AddEdge(1, 2);
        g1.AddEdge(2, 3);
        var g2 = new GraphIsomorphism(4);
        g2.AddEdge(0, 2);
        g2.AddEdge(0, 3);
        g2.AddEdge(1, 2);
        g2.AddEdge(1, 3);
        bool isomorphic = g1.IsIsomorphic(g2);
        Console.WriteLine($"Графы изоморфны: {isomorphic}");
    }
}
// 6 Топологическая сортировка алгоритм Тарьяна
public class TopologicalSort
{
    private List<int>[] graph;
    private bool[] visited;
    private bool[] recursionStack;
    private Stack<int> stack;
    private bool hasCycle;
    public TopologicalSort(int vertices)
    {
        graph = new List<int>[vertices];
        for (int i = 0; i < vertices; i++)
            graph[i] = new List<int>();
    }
    public void AddEdge(int from, int to) => graph[from].Add(to);
    public List<int> Sort()
    {
        int n = graph.Length;
        visited = new bool[n];
        recursionStack = new bool[n];
        stack = new Stack<int>();
        hasCycle = false;
        for (int i = 0; i < n; i++)
            if (!visited[i] && !hasCycle)
                DFS(i);
        if (hasCycle) return null;
        return stack.ToList();
    }
    private void DFS(int v)
    {
        visited[v] = true;
        recursionStack[v] = true;
        foreach (int neighbor in graph[v])
        {
            if (!visited[neighbor])
                DFS(neighbor);
            else if (recursionStack[neighbor])
                hasCycle = true;
            if (hasCycle) return;
        }
        recursionStack[v] = false;
        stack.Push(v);
    }
}
// 12 Максимальный поток (проталкивание предпотока)
public class PushRelabel
{
    private class Edge
    {
        public int To;
        public long Capacity;
        public int ReverseIndex;
        public Edge(int to, long capacity, int reverseIndex)
        {
            To = to;
            Capacity = capacity;
            ReverseIndex = reverseIndex;
        }
    }
    private List<Edge>[] graph;
    private long[] excess;
    private int[] height;
    private int n;
    public PushRelabel(int vertices)
    {
        n = vertices;
        graph = new List<Edge>[n];
        for (int i = 0; i < n; i++)
            graph[i] = new List<Edge>();
        excess = new long[n];
        height = new int[n];
    }
    public void AddEdge(int from, int to, long capacity)
    {
        var forward = new Edge(to, capacity, graph[to].Count);
        var backward = new Edge(from, 0, graph[from].Count);
        graph[from].Add(forward);
        graph[to].Add(backward);
    }
    public long MaxFlow(int source, int sink)
    {
        height[source] = n;
        //Инициализация предпотока
        foreach (var e in graph[source])
        {
            Push(source, e);
        }
        //Основной цикл
        for (int v = 0; v < n; v++)
        {
            if (v != source && v != sink)
                Discharge(v);
        }
        return excess[sink];
    }
    private void Push(int from, Edge e)
    {
        long flow = Math.Min(excess[from], e.Capacity);
        e.Capacity -= flow;
        graph[e.To][e.ReverseIndex].Capacity += flow;
        excess[from] -= flow;
        excess[e.To] += flow;
    }
    private void Relabel(int v)
    {
        int minHeight = int.MaxValue;
        foreach (var e in graph[v])
        {
            if (e.Capacity > 0)
                minHeight = Math.Min(minHeight, height[e.To]);
        }
        if (minHeight < int.MaxValue)
            height[v] = minHeight + 1;
    }
    private void Discharge(int v)
    {
        while (excess[v] > 0)
        {
            bool pushed = false;
            foreach (var e in graph[v])
            {
                if (e.Capacity > 0 && height[v] == height[e.To] + 1)
                {
                    Push(v, e);
                    pushed = true;
                    if (excess[v] == 0) break;
                }
            }
            if (!pushed && excess[v] > 0)
                Relabel(v);
        }
    }
}
//18 Проверка изоморфности графов (простой перебор)
public class GraphIsomorphism
{
    private int n;
    private bool[,] adjacency;
    public GraphIsomorphism(int vertices)
    {
        n = vertices;
        adjacency = new bool[n, n];
    }
    public void AddEdge(int u, int v)
    {
        adjacency[u, v] = true;
        adjacency[v, u] = true;
    }
    public bool IsIsomorphic(GraphIsomorphism other)
    {
        if (n != other.n) return false;
        int[] perm = new int[n];
        bool[] used = new bool[n];
        return TryAllPermutations(0, other, perm, used);
    }
    private bool TryAllPermutations(int pos, GraphIsomorphism other, int[] perm, bool[] used)
    {
        if (pos == n)
        {
            //Проверяем сохраняет ли перестановка смежность
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (adjacency[i, j] != other.adjacency[perm[i], perm[j]])
                        return false;
            return true;
        }
        for (int v = 0; v < n; v++)
        {
            if (!used[v])
            {
                perm[pos] = v;
                used[v] = true;
                if (TryAllPermutations(pos + 1, other, perm, used))
                    return true;
                used[v] = false;
            }
        }
        return false;
    }
}
