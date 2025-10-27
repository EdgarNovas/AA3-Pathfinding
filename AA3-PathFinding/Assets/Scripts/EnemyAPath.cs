using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Edgar;

public class EnemyAPath : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Edgar.Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start()
    {
        
        nodeDiameter = nodeRadius * 2;
        
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        
        CreateGrid();
        
    }


    private void CreateGrid()
    {
        grid = new Edgar.Node[gridSizeX, gridSizeY];
        
        Vector2 worldBottomLeft = TransformV2() - Vector2.right * gridWorldSize.x / 2 -
            Vector2.up * gridWorldSize.y / 2; //En 3d Cambiar up por Forward

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint,x,y);
                
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Edgar.Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.red;

                Gizmos.DrawCube(n.worldPosition, Vector2.one * (nodeDiameter - .1f));
            }
        }
    }

    Vector2 TransformV2()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }

}
