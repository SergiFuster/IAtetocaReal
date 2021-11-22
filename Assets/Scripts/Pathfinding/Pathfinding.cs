using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;

    GridNode grid;
    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<GridNode>(); 
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPost)
    {
        StartCoroutine(FindPath(startPos, targetPost));
    }
    IEnumerator FindPath(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPosition(startPoint);
        Node endNode = grid.NodeFromWorldPosition(endPoint);

        List<Node> openNodes = new List<Node>();
        HashSet<Node> closedNodes = new HashSet<Node>();
        openNodes.Add(startNode);

        if(startNode.walkable && endNode.walkable)
        {
            while(openNodes.Count > 0)
            {
                Node currentNode = openNodes[0];

                //Change current node with the node with lowest fCost, if same, change by the lowest hCost
                for(int i = 1; i < openNodes.Count; i++)
                {
                    if(openNodes[i].fCost < currentNode.fCost || openNodes[i].fCost == currentNode.fCost)
                    {
                        if(openNodes[i].hCost < currentNode.hCost)
                            currentNode = openNodes[i];
                    }
                }
                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                if (currentNode == endNode)
                {
                    pathSuccess = true;
                    break;
                }

                //Search neighbours and calculate hCost and gCost
                foreach(Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedNodes.Contains(neighbour))
                        continue;
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if(newMovementCostToNeighbour < neighbour.gCost || !openNodes.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, endNode);
                        neighbour.parent = currentNode;

                        if (!openNodes.Contains(neighbour))
                            openNodes.Add(neighbour);
                    }
                }
            }
        }

        yield return null;

        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, endNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> newPath = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for(int i = 0; i < path.Count-1; i++)
        {
            Vector2 directionNew = new Vector2(path[i].gridX - path[i+1].gridX, path[i].gridY - path[i+1].gridY);
            if(directionNew != directionOld)
            {
                newPath.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return newPath.ToArray(); 
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY) 
            return (14 * dstY + 10 * (dstX - dstY));

        return (14 * dstX + 10 * (dstY - dstX));
    }
}
