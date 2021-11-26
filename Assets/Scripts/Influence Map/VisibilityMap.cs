using System.Collections.Generic;
using UnityEngine;

public class VisibilityMap
{
    public Vector3 gridWorldSize;
    public VisibilityNode[,] grid;
    public bool showGrid;
    public float nodeRadius;
    int columns, rows;
    Vector3 worldBottomLeft;
    Vector3 centerWorldPosition;
    float nodeDiameter;
    [HideInInspector]
    public List<Unit> viewPoints;

    public VisibilityMap(Vector3 _gridWorldSize, bool _showGrid, float _nodeRadius, Vector3 _worldBottomLeft)
    {
        gridWorldSize = _gridWorldSize;
        showGrid = _showGrid;
        nodeRadius = _nodeRadius;
        worldBottomLeft = _worldBottomLeft;
        centerWorldPosition = worldBottomLeft + gridWorldSize / 2;
        nodeDiameter = nodeRadius * 2;
        columns = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        rows = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        viewPoints = new List<Unit>();
        CreateGrid();
    }

    public void CreateGrid()
    {

        grid = new VisibilityNode[rows, columns];
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector3 worldPosition = worldBottomLeft + 
                                        Vector3.right * (y * nodeDiameter + nodeRadius) + 
                                        Vector3.up * (x * nodeDiameter + nodeRadius);
                grid[x, y] = new VisibilityNode(worldPosition, false, x, y);
            }
        }
    }

    private VisibilityNode GetNodeFromWorldPosition(Vector3 worldPos)
    {
        float percentY = ((worldPos.x - centerWorldPosition.x) + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentX = ((worldPos.y - centerWorldPosition.y) + gridWorldSize.y / 2) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((rows - 1) * percentX);
        int y = Mathf.RoundToInt((columns - 1) * percentY);

        return grid[x, y];
    }

    public void InsertViewPoint(Unit _viewPoint)
    {
        viewPoints.Add(_viewPoint);
    }

    public void DeleteViewPoint(Unit _viewPoint)
    {
        viewPoints.Remove(_viewPoint);
    }

    private List<VisibilityNode> GetNeighbourNodes(VisibilityNode node)
    {
        List<VisibilityNode> neighbours = new List<VisibilityNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < rows && checkY >= 0 && checkY < columns)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public void UpdateVisibilityMap()
    {

        foreach (VisibilityNode node in grid)
        {
            node.visible = false;
        }

        foreach (Unit viewPoint in viewPoints)
        {
            List<VisibilityNode> neighbours = GetNeighbourNodes(GetNodeFromWorldPosition(viewPoint.transform.position));
            foreach (VisibilityNode neighbour in neighbours)
            {
                neighbour.visible = true;
            }
        }
    }
}
