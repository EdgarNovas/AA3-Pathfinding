using UnityEngine;

public class AlgorithmSelector : MonoBehaviour
{
 public PathFinding pf;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            pf.currentAlgorithm = AlgorithmType.BFS;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            pf.currentAlgorithm = AlgorithmType.Dijkstra;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            pf.currentAlgorithm = AlgorithmType.GreedyBFS;

        if (Input.GetKeyDown(KeyCode.Alpha4))
            pf.currentAlgorithm = AlgorithmType.AStar;
    }
}
