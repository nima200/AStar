using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

public class AStar : MonoBehaviour
{
    private HexGrid _grid;
    public Transform Source, Target;
    public Coordinates PlayerCoordinates;
    public List<Cell> PlayerNeighbors;

    private void Awake()
    {
        _grid = GetComponent<HexGrid>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PathFind(Source.position, Target.position);
        }
        PlayerCoordinates = _grid.CellFromWorld(Source.position).Coordinates;
        PlayerNeighbors = _grid.CellFromWorld(Source.position).Neighbors.ToList();
    }

    private void PathFind(Vector3 startPosition, Vector3 endPosition)
    {
        var startNode = _grid.CellFromWorld(startPosition).Node;
        var endNode = _grid.CellFromWorld(endPosition).Node;

        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost >= currentNode.FCost && openSet[i].FCost == currentNode.FCost) continue;
                if (openSet[i].HCost < currentNode.HCost)
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }
            var currentCell = _grid.CellFromWorld(currentNode.WorldPosition);
            foreach (var neighbor in _grid.GetNeighbors(currentCell))
            {
                if (!neighbor.Node.Walkable || closedSet.Contains(neighbor.Node)) continue;
                int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor.Node);
                if (newCostToNeighbor >= neighbor.Node.GCost && openSet.Contains(neighbor.Node)) continue;
                neighbor.Node.GCost = newCostToNeighbor;
                neighbor.Node.HCost = GetDistance(neighbor.Node, endNode);
                neighbor.Node.Parent = currentNode;
                if (!openSet.Contains(neighbor.Node))
                    openSet.Add(neighbor.Node);
            }
        }
    }

    private void RetracePath(Node start, Node end)
    {
        var path = new List<Node>();
        var currentNode = end;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        _grid.Path = path;
    }

    private static int GetDistance(Node a, Node b)
    {
        int distanceX = Mathf.Abs(a.GridX - b.GridY);
        int distanceY = Mathf.Abs(a.GridY - b.GridY);
        return distanceX > distanceY
            ? 10 * distanceY + 10 * (distanceX - distanceY)
            : 10 * distanceX + 10 * (distanceY - distanceX);
    }
}
