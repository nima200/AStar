using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
    private HexGrid _grid;
    public Transform Source, Target;

    private void Awake()
    {
        _grid = GetComponent<HexGrid>();
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
                var neighborNode = neighbor.Node;
                if (!neighborNode.Walkable || closedSet.Contains(neighborNode)) continue;
                int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighborNode);
                if ()
            }
        }
    }
}
