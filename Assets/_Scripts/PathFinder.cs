using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

public class PathFinder : MonoBehaviour
{
    private Grid _grid;
    public Transform Source, Target;

    [UsedImplicitly]
    private void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    [UsedImplicitly]
    private void Update()
    {
        // Constantly check for A* Path between the start and end
        FindPath(Source.position, Target.position);
    }

    // A*
    private void FindPath(Vector3 startPosition, Vector3 endPosition)
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

    private void PreProcess(int maxLevel)
    {
        
    }

    private void AbstractMaze()
    {
        var clusters = BuildClusters(1);
        var clusterPairs = (from c1 in clusters
            from c2 in clusters
            select new Pair<Cluster,Cluster>(c1, c2)).ToList();
        foreach (var pair in clusterPairs)
        {
            // If the pair's elements are adjacent
            if ((pair.B.X == pair.A.X + 1 && pair.B.Y == pair.A.Y) ||
                (pair.B.X == pair.A.X - 1 && pair.B.Y == pair.A.Y) ||
                (pair.B.Y == pair.A.Y + 1 && pair.B.X == pair.A.X) ||
                (pair.B.Y == pair.A.Y - 1 && pair.B.X == pair.A.X))
            {

            }
        }

    }
    /// <summary>
    /// Builds an entrance between the two clusters, given the following rules
    /// 1) Border Limitation Condition: The entrance is defined along and cannot exceed the border between two adjacent clusters
    /// 2) Symmetry Condition: t is part of the transition between c1 and c2, if and only if symT is also a part of the transition and on the opposite cluster that t was in
    /// 3) Obstacle Free Condition: An entrance contains no obstacle tiles
    /// 4) Maximality Condition: An entrance is extended in both directions as long as the previous conditions remain true
    /// </summary>
    /// <param name="c1">The first cluster</param>
    /// <param name="c2">The second cluster</param>
    private List<Entrance> BuildEntrances(Cluster c1, Cluster c2)
    {
        var direction = CheckDirection(c1, c2);
        var entrances = new List<Entrance>();
        switch (direction)
        {
            case Direction.North:
                var l1 = new Node[c1.Nodes.Length]; 
                var l2 = new Node[c2.Nodes.Length]; 
                for (int i = 0; i < l1.Length; i++)
                {
                    l1[i] = c1.Nodes[i, 0]; // The edge shared by c1 in the entrance
                    l2[i] = c2.Nodes[i, c2.Nodes.Length - 1]; // The edge shared by c2 in the entrance
                }
                
                break;
            case Direction.East:
                break;
            case Direction.South:
                break;
            case Direction.West:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return null;
    }

    public void FindEntrance(Node[] l1, Node[] l2)
    {
        bool onStreak = false;
        var entrances = new List<List<Node>>();
        var entrance = new List<Node>();
        for (int i = 0; i < l1.Length; i++)
        {
            if (l1[i].Walkable)
            {
                if (l2[i].Walkable)
                {
                    if (onStreak)
                    {
                        entrance.Add(l1[i]);
                        entrance.Add(l2[i]);
                    }
                    else
                    {
                        onStreak = true;
                        entrance = new List<Node> {l1[i], l2[i]};
                    }
                }
                else
                {
                    onStreak = false;
                    if (entrance.Count > 0) entrances.Add(entrance);
                    entrance = new List<Node>();
                }
            }
        }
    }

    private static Direction CheckDirection(Cluster c1, Cluster c2)
    {
        if (c2.X == c1.X + 1 && c2.Y == c1.Y)
        {
            return Direction.East;
        }
        if (c2.X == c1.X - 1 && c2.Y == c1.Y)
        {
            return Direction.West;
        }
        if (c2.Y == c1.Y + 1 && c2.X == c1.X)
        {
            return Direction.North;
        }
        if (c2.Y == c1.Y - 1 && c2.X == c1.X)
        {
            return Direction.South;    
        }
        throw new ArgumentException("Invalid clusters! Possible not adjacent.");
    }

    private List<Cluster> BuildClusters(int level)
    {
        int scaleFactor = 5;
        // Assuming grid is square for now
        int sideLength = Mathf.RoundToInt(_grid.GridWorldSize.x / scaleFactor);
        var clusters = new List<Cluster>();

        for (int x = 0; x < scaleFactor; x++)
        {
            for (int y = 0; y < scaleFactor; y++)
            {
                var cluster = new Cluster(sideLength, x, y);
                for (int i = 0; i < sideLength; i++)
                {
                    for (int j = 0; j < sideLength; j++)
                    {
                        cluster.Set(_grid.Nodes[i + (x * sideLength), j + (y * sideLength)], i, j);
                    }
                }
                clusters.Add(cluster);
            }
        }
        return clusters;
    }
    public class Entrance
    {
        public List<Node[]> Lines { get; private set; }
        public Pair<Node, Node> Transition { get; private set; }
        public Node[] L1 { get; private set; }
        public Node[] L2 { get; private set; }
        public Entrance(Node[] l1, Node[] l2)
        {
            L1 = l1;
            L2 = l2;
            Lines = new List<Node[]>(2) {L1, L2};
            // Assuming the right L1, L2 was passed, having the same length.
            int midpointIndex = L1.Length / 2;
            var t = L1[midpointIndex];
            var symT = L2[midpointIndex];
            Transition = new Pair<Node, Node>(t, symT);
        }
    }
}
