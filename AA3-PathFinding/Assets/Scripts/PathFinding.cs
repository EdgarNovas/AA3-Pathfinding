using Edgar;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Diagnostics;
using System;


public enum AlgorithmType
{
    AStar,
    Dijkstra,
    GreedyBFS,
    BFS
}

public class PathFinding : MonoBehaviour
{
    public AlgorithmType currentAlgorithm = AlgorithmType.AStar;
    PathRequestManager requestManager;
    TwoDAPath grid2D; // Renombrado para mayor claridad
    Grid3D grid3D;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid2D = GetComponent<TwoDAPath>();
        grid3D = GetComponent<Grid3D>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        // 1. DETECTAR QUÉ GRID USAR Y EVITAR NULOS
        Node startNode = null;
        Node targetNode = null;
        int maxSize = 0;
        bool use3D = false;

        if (grid3D != null)
        {
            use3D = true;
            startNode = grid3D.NodeFromWorldPoint(startPos);
            targetNode = grid3D.NodeFromWorldPoint(targetPos);
            maxSize = grid3D.MaxSize;
        }
        else if (grid2D != null)
        {
            use3D = false;
            startNode = grid2D.NodeFromWorldPoint(startPos);
            targetNode = grid2D.NodeFromWorldPoint(targetPos);
            maxSize = grid2D.MaxSize;
        }
        else
        {
            UnityEngine.Debug.LogError("PathFinding: No se encontró ningún componente Grid (ni 2D ni 3D).");
            yield break; // Salimos si no hay mapa
        }

        // 2. COMPROBAR SI LOS NODOS SON VÁLIDOS Y CAMINABLES
        if (startNode.walkable && targetNode.walkable)
        {
            // Usamos maxSize dinámico
            Heap<Node> openSet = new Heap<Node>(maxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path Found " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                // 3. OBTENER VECINOS SEGÚN EL GRID ACTIVO
                List<Node> neighbours;
                if (use3D)
                    neighbours = grid3D.GetNeighbours(currentNode);
                else
                    neighbours = grid2D.GetNeighbours(currentNode);

                foreach (Node neighbour in neighbours)
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        // Invertimos primero para simplificar en el orden correcto (inicio -> fin)
        path.Reverse();
        Vector3[] waypoints = SimplifyPath(path);

        // SimplifyPath devuelve array, no necesitamos invertir de nuevo si lo hacemos bien
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count; i++)
        {
            // Nota: En 3D es mejor devolver todos los nodos al principio para probar
            // o calcular la dirección basándose en gridX/gridY aunque sea 3D
            waypoints.Add(path[i].worldPosition);
        }

        // He simplificado esta función temporalmente para asegurar que el movimiento 
        // funcione suave en 3D antes de optimizar vértices.
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        // Esto funciona igual para 2D y 3D porque gridY en 3D representa la Z
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);

        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
}
