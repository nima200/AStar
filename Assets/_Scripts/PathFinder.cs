using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

public class PathFinder : MonoBehaviour
{
    private Grid _grid;
    public Transform Source, Target;

    private void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    [UsedImplicitly]
    private void Update()
    {
        // Constantly check for A* Path between the start and end
        AStar(Source.position, Target.position);
    }

    // A*
    private void AStar(Vector3 startPosition, Vector3 endPosition)
    {
        var startNode = _grid.NodeFromWorld(startPosition);
        var endNode = _grid.NodeFromWorld(endPosition);

        // The set of nodes to be evaluated
        var openSet = new List<Node>();
        // The set of nodes already evaluated
        var closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var currentNode = openSet[0];
            // Current node = node in Open Set with the lowest FCost.
            for (int i = 1; i < openSet.Count; i++)
            {
                // Also take into account when two nodes have equal FCost: Take the one closer to destination
                if (openSet[i].FCost >= currentNode.FCost && openSet[i].FCost != currentNode.FCost) continue;
                if (openSet[i].HCost < currentNode.HCost)
                {
                    currentNode = openSet[i];
                }
            }
            // Essentially remove the node we decided to work with from openset because we have 'evaluated' it
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // If we have reached the destination, retrace the path back to start and end
            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }

            /*
             * Actual Algorithm core:
             * for each neighbor of current node, if the neighbor is not traversable or it's already evaluated, 
             * then skip to the next neighbor. If not, if the path from current node to the neighbor node is cheaper
             * than the neighbor's cost of traversal, or if the neighbor is evaluated already, 
             * then you can calculate the new FCost through H and G cost, and if it is not considered open for evaluation already,
             * then you can add it to the list of nodes to be evaluated. 
             * Note that stuff previously removed from open for evaluation set will not get added back to it as the optimal distance
             * was chosen anyways.
             */
            foreach (var neighbor in _grid.GetNeighbors(currentNode))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor)) continue;
                int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor >= neighbor.GCost && openSet.Contains(neighbor)) continue;
                neighbor.GCost = newCostToNeighbor;
                neighbor.HCost = GetDistance(neighbor, endNode);
                neighbor.Parent = currentNode;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
            }
        }
    }

    /* Basic helper method to retrace the path taken through tracing the node parent repetitively until start is reached. */
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
    /*First count on the x axis how many nodes away you are from the destination, then do the same on the y axis
     * Take the lowst number and that gives us the number of diagonal nodes we need to take to be inline with the end node
     * To calculate howmany inline moves you need, subtract lower number from higher number, and that's the number of inline moves
     * 
     * Diagonal distance = 14 (sqrt(2) * 10)
     * Inline distance = 10 (1 * 10)
     */
    private static int GetDistance(Node a, Node b)
    {
         

        int distanceX = Mathf.Abs(a.GridX - b.GridX);
        int distanceY = Mathf.Abs(a.GridY - b.GridY);

        if (distanceX > distanceY)
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
}
