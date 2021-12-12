using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode : MonoBehaviour
{
    public LayerMask unwalkableMask;
    public Unit BlueBat;
    public Unit RedBat;
    public Node[,] grid;
    public List<Vector3> path;
    public bool showNodes;
    public float nodeRadius;
    public Vector3 gridWorldSize;
    int gridSizeX, gridSizeY;
    Vector3 worldBottomLeft;
    float nodeDiameter;

    private void Awake()
    {
        CreateGrid();
    }
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) == Mathf.Abs(y))
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
    public Node NodeFromWorldPosition(Vector3 worldPosition)
    {

        float percentX = ((worldPosition.x - transform.position.x) + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = ((worldPosition.y - transform.position.y) + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY-1) * percentY);

        return grid[x, y];
    }

    public void CreateGrid()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPosition = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable;

                if(GM.instance != null && GM.instance.selectedUnit != null && (GM.instance.selectedUnit.fly))
                    walkable = Physics2D.OverlapBox(worldPosition, new Vector2(nodeDiameter - .1f, nodeDiameter - .1f), 0);
                else
                    walkable = !Physics2D.OverlapBox(worldPosition, new Vector2(nodeDiameter - .1f, nodeDiameter - .1f),0, unwalkableMask);
                grid[x, y] = new Node(walkable, worldPosition, x, y);
            }
        }
    }
   
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));
        if(grid != null && showNodes)
        {
            foreach (Node n in grid)
            {
                if(path == null)
                {
                    if (n.walkable)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                    }
                }
                else
                {
                    if (path.Contains(n.worldPosition))
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                    }
                    else
                    {
                        if (n.walkable)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                        }
                        else
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                        }
                    }
                }
            }
        }
    }  
}
