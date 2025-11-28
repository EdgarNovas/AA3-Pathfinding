using Edgar;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingExperiment : MonoBehaviour
{
    public int numInstances = 20;

    private List<int> exploredAStar = new List<int>();
    private List<int> exploredBFS = new List<int>();
    private List<int> exploredDijkstra = new List<int>();
    private List<int> exploredGreedy = new List<int>();

    private Grid3D grid;
    private PathFinding path;

    void Start()
    {
        grid = FindObjectOfType<Grid3D>();
        path = FindObjectOfType<PathFinding>();

        RunExperiments();
        PrintSummary();
    }

    void RunExperiments()
    {
        for (int i = 0; i < numInstances; i++)
        {
            // 1. Obtener posiciones aleatorias válidas
            Node start = GetRandomWalkable();
            Node target = GetRandomWalkable();

            int explored = 0;

            path.BFS(start, target, ref explored);
            exploredBFS.Add(explored);

            // 2. A*
            explored = 0;
            path.AStar(start, target, ref explored);
            exploredAStar.Add(explored);

            // 3. Dijkstra
            explored = 0;
            path.Dijkstra(start, target, ref explored);
            exploredDijkstra.Add(explored);

            // 4. Greedy
            explored = 0;
            path.Greedy(start, target, ref explored);
            exploredGreedy.Add(explored);
        }
    }

    Node GetRandomWalkable()
    {
        Node n = null;

        int sizeX = grid.GridArray.GetLength(0);
        int sizeY = grid.GridArray.GetLength(1);
        do
        {
            int x = Random.Range(0, sizeX);
            int y = Random.Range(0, sizeY);

            n = grid.GridArray[x, y];
        }
        while (!n.walkable);
        return n;
    }

    void PrintSummary()
    {
        Debug.Log("===== RESULTADOS TEST=====");
        Debug.Log(GetSummary("A*", exploredAStar));
        Debug.Log(GetSummary("BFS", exploredBFS));
        Debug.Log(GetSummary("Dijkstra", exploredDijkstra));
        Debug.Log(GetSummary("Greedy", exploredGreedy));
        Debug.Log("===== FINAL TEST=====");
    }

    string GetSummary(string name, List<int> values)
    {
        int min = int.MaxValue;
        int max = int.MinValue;
        float sum = 0;

        foreach (int v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
            sum += v;
        }

        float avg = sum / values.Count;

        return $"{name} -> Min: {min}, Max: {max}, Avg: {avg}";
    }
}
