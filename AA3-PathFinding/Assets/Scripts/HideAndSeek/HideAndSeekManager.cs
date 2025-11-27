using UnityEngine;
using Edgar;
public class HideAndSeekManager : MonoBehaviour
{
    [Header("Configuración")]
    public Transform sentinel;   // El que busca
    public Transform hider;      // El que se esconde
    public LayerMask obstacleMask; // Capa de los muros para bloquear la visión
    public bool drawGizmos = true;

    [Header("Referencias")]
    public Grid3D grid3D;

    private void Update()
    {
        if (grid3D == null || grid3D.GridArray == null) return;

        // CALCULO VISIBILIDAD (Exposure Map)
        // Esto actualiza la variable 'isVisible' de cada nodo
        UpdateExposureMap();

      
        // Comprobar si el Hider está actualmente visible
        Node hiderNode = grid3D.NodeFromWorldPoint(hider.position);

        if (hiderNode.isVisible)
        {
            // Si nos ven, buscamos el sitio escondido más cercano
            Vector3 bestHidingSpot;
            if (GetNearestHidingSpot(hiderNode, out bestHidingSpot))
            {
                // Solo pedimos camino si no estamos ya yendo hacia allá
                // Cosa a poner en el trabajo escrito(Para evitar spam de peticiones, podriamos añadir un control de distancia extra)
                PathRequestManager.RequestPath(hider.position, bestHidingSpot, OnPathFound);
            }
        }
    }

    void OnPathFound(Vector3[] newPath, bool success)
    {
        if (success)
        {
            hider.GetComponent<Unit3D>().OnPathFound(newPath, success);
        }
    }


    void UpdateExposureMap()
    {
        Vector3 eyePos = sentinel.position;
        // Elevamos un poco el ojo para que no choque con el suelo inmediatamente
        Vector3 eyePosElevated = eyePos + Vector3.up * 0.5f;

        Node[,] grid = grid3D.GridArray;

        foreach (Node node in grid)
        {
            // Resetear estado
            node.isVisible = false;

            // NO HAY UN OBSTACULO ENCIMA
            if (!node.walkable) continue;

            // Dirección desde el ojo hasta el nodo
            // Elevamos un poco el node también
            Vector3 nodePosElevated = node.worldPosition + Vector3.up * 0.5f;
            Vector3 dirToNode = nodePosElevated - eyePosElevated;
            float dist = dirToNode.magnitude;

            // Lanzamos Raycast. Si NO choca con nada (obstacleMask) antes de llegar, es visible.
            // Usao dist - 0.1f para no chocar con el propio centro del nodo si hay un collider cerca.
            if (!Physics.Raycast(eyePosElevated, dirToNode.normalized, out RaycastHit hit, dist, obstacleMask))
            {
                node.isVisible = true;
            }
        }
    }

    bool GetNearestHidingSpot(Node startNode, out Vector3 hidingPosition)
    {
        Node[,] grid = grid3D.GridArray;
        Node bestNode = null;
        int minDistance = int.MaxValue;

        foreach (Node node in grid)
        {
            // Solo nos interesan nodos caminables y QUE NO SEAN VISIBLES
            if (!node.walkable || node.isVisible) continue;

            // Calculamos distancia (usamos tu heurística simple para ser rápidos)
            int dist = GetDistance(startNode, node);

            if (dist < minDistance)
            {
                minDistance = dist;
                bestNode = node;
            }
        }

        if (bestNode != null)
        {
            hidingPosition = bestNode.worldPosition;
            return true;
        }

        hidingPosition = Vector3.zero;
        return false;
    }

    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid3D == null || grid3D.GridArray == null) return;

        foreach (Node n in grid3D.GridArray)
        {
            if (!n.walkable) continue; // No dibujamos muros

            // ROJO =Peligro
            // VERDE =Seguro
            Gizmos.color = n.isVisible ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);

            // Dibujar cubo un poco más pequeño que el nodo para que lo veas bien
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (grid3D.nodeRadius * 1.8f));
        }

        // Dibujar linea desde el centinela al jugador para depurar visualmente
        if (sentinel != null && hider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(sentinel.position, hider.position);
        }
    }
}
