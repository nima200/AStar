using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Policy;
using Priority_Queue;
public enum Optimization { List, Heap, PriorityQueue}
public class AStar : MonoBehaviour
{
    private HexGrid _grid;
    public Transform Source, Target;
    private PathRequestManager _requestManager;
    public Optimization Optimization;

    private void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<HexGrid>();
    }

    private void Update()
    {
    }

    private IEnumerator PathFind_LIST(Vector3 startPosition, Vector3 endPosition)
    {
        
        var sw = new Stopwatch();
        sw.Start();
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
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + "ms WITH LIST");
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

    private IEnumerator PathFind_HEAP(Vector3 startPosition, Vector3 endPosition)
    {
        
        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(startPosition);
        var endHex = _grid.HexFromPoint(endPosition);

        var openSet = new Heap<Hexagon>(_grid.MaxHeapSize);
        var closedSet = new HashSet<Hexagon>();

        openSet.Add(startHex);

        while (openSet.Count > 0)
        {
            var currentHex = openSet.RemoveFirst();
            closedSet.Add(currentHex);

            if (currentHex == endHex)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + "ms WITH HEAP");
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
                else
                    openSet.UpdateItem(neighbor);
            }
        }
        yield return null;
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        _requestManager.FinishedProcessingPath(path, foundPath);
    }
    private IEnumerator PathFind_PRIORITYQUEUE(Vector3 startPosition, Vector3 endPosition)
    {

        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(startPosition);
        var endHex = _grid.HexFromPoint(endPosition);

        var openSet = new SimplePriorityQueue<Hexagon, int>();
        var closedSet = new HashSet<Hexagon>();

        openSet.Enqueue(startHex, startHex.FCost);

        while (openSet.Count > 0)
        {
            var currentHex = openSet.Dequeue();
            closedSet.Add(currentHex);

            if (currentHex == endHex)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + "ms WITH PRIORITY QUEUE");
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
                    openSet.Enqueue(neighbor, neighbor.FCost);
                else
                {
                    openSet.UpdatePriority(neighbor, neighbor.FCost);
                }
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
        switch (Optimization)
        {
            case Optimization.List:
                StartCoroutine(PathFind_LIST(pathStart, pathEnd));
                break;
            case Optimization.Heap:
                StartCoroutine(PathFind_HEAP(pathStart, pathEnd));
                break;
            case Optimization.PriorityQueue:
                StartCoroutine(PathFind_PRIORITYQUEUE(pathStart, pathEnd));
                break;
        }
        
    }
}
