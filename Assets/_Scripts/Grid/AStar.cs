using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

public class AStar : MonoBehaviour
{
    private HexGrid _grid;
    public Transform Source, Target;
    private PathRequestManager _requestManager;

    private void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<HexGrid>();
    }

    private void Update()
    {
    }

    private IEnumerator PathFind(Vector3 startPosition, Vector3 endPosition)
    {
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(startPosition);
        var endHex = _grid.HexFromPoint(endPosition);

        var openSet = new List<Hexagon>();
        var closedSet = new HashSet<Hexagon>();

        openSet.Add(startHex);

        while (openSet.Count > 0)
        {
            var currentHex = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost >= currentHex.FCost && openSet[i].FCost == currentHex.FCost) continue;
                if (openSet[i].HCost < currentHex.HCost)
                {
                    currentHex = openSet[i];
                }
            }
            openSet.Remove(currentHex);
            closedSet.Add(currentHex);

            if (currentHex == endHex)
            {
                foundPath = true;
                break;
            }
            var currentCell = _grid.HexFromPoint(currentHex.transform.position);
            foreach (var neighbor in _grid.GetNeighbors(currentCell))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor)) continue;
                int newCostToNeighbor = currentHex.GCost + GetDistance(currentHex, neighbor);
                if (newCostToNeighbor >= neighbor.GCost && openSet.Contains(neighbor)) continue;

                neighbor.GCost = newCostToNeighbor;
                neighbor.HCost = GetDistance(neighbor, endHex);
                neighbor.Parent = currentHex;
                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
            }
        }
        yield return null;
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        _requestManager.FinishedProcessingPath(path, foundPath);
    }

    private static Path RetracePath(Hexagon start, Hexagon end)
    {
        var path = new List<Hexagon>();
        var currentNode = end;
        do
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        } while (currentNode != start);
        var waypoints = HexToVec3(path);
        Array.Reverse(waypoints);
        return new Path(waypoints);
    }

    private static Vector3[] HexToVec3(IEnumerable<Hexagon> path)
    {
        return path.Select(hex => hex.transform.position).ToArray();
    }

    private static int GetDistance(Hexagon a, Hexagon b)
    {
        return Mathf.Max(Mathf.Abs(a.Coordinates.X - b.Coordinates.X),
            Mathf.Abs(a.Coordinates.Y - b.Coordinates.Y), Mathf.Abs(a.Coordinates.Z - b.Coordinates.Z));
    }

    public void StartFindPath(Vector3 pathStart, Vector3 pathEnd)
    {
        StartCoroutine(PathFind(pathStart, pathEnd));
    }
}
