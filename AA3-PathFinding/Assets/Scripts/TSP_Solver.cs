using System.Collections.Generic;
using UnityEngine;
using Edgar;
using System.Collections;

public class TSP_Solver : MonoBehaviour
{
    public PathFinding pathfinding;
    public Grid3D grid;

    public TSP_Solver(Grid3D grid, PathFinding pathfinding)
    {
        this.grid = grid;
        this.pathfinding = pathfinding;
    }

    private void Start()
    {
        TSP_Solver solver = new TSP_Solver(grid, pathfinding);

        Node start = grid.NodeFromWorldPoint(transform.position);

        List<Node> destinos = new List<Node>()
    {
        grid.NodeFromWorldPoint(new Vector3(5, 0, 3)),
        grid.NodeFromWorldPoint(new Vector3(10, 0, -2)),
        grid.NodeFromWorldPoint(new Vector3(-4, 0, 8))
    };

        List<Node> caminoCompleto = solver.SolveTSP(start, destinos);

        StartCoroutine(MoveAlongPath(caminoCompleto));
    }

    /// <summary>
    /// Resuelve la ruta en orden aproximado estilo TSP usando:
    /// - Greedy (Next Nearest Node)
    /// - A* para cada tramo
    /// </summary>
    public List<Node> SolveTSP(Node start, List<Node> destinations)
    {
        List<Node> finalPath = new List<Node>();
        Node current = start;

        // Copia de trabajo
        List<Node> pending = new List<Node>(destinations);

        while (pending.Count > 0)
        {
            // 1. Elegir destino más cercano (según heurística)
            Node nextTarget = GetClosestNode(current, pending);

            // 2. Calcular ruta con A*
            List<Node> segmentPath = pathfinding.FindPath_Nodes(current.worldPosition, nextTarget.worldPosition);

            if (segmentPath == null || segmentPath.Count == 0)
            {
                Debug.LogWarning("No se encontró camino entre puntos en TSP");
                break;
            }

            // Evitar duplicar la posición inicial del tramo
            if (finalPath.Count > 0)
                segmentPath.RemoveAt(0);

            // 3. Añadir camino
            finalPath.AddRange(segmentPath);

            // 4. Pasamos al siguiente punto
            current = nextTarget;
            pending.Remove(nextTarget);
        }

        return finalPath;
    }

    /// <summary>
    /// Devuelve el nodo más cercano según la heurística A*
    /// </summary>
    private Node GetClosestNode(Node current, List<Node> nodes)
    {
        Node best = null;
        int bestDist = int.MaxValue;

        foreach (Node n in nodes)
        {
            int d = pathfinding.GetDistance(current, n);

            if (d < bestDist)
            {
                bestDist = d;
                best = n;
            }
        }

        return best;
    }

    IEnumerator MoveAlongPath(List<Node> path, float speed = 3f)
    {
        if (path == null || path.Count == 0)
            yield break;

        foreach (Node node in path)
        {
            Vector3 targetPos = node.worldPosition;

            // Mientras no llegamos al nodo
            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    speed * Time.deltaTime
                );

                Debug.Log("Moving");

                yield return null;
            }

            // Ajustar posición exacta al terminar cada paso
            transform.position = targetPos;

            yield return null;
        }
    }
}
