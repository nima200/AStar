using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Policy;
using Priority_Queue;
public enum Optimization { AS_List, AS_Heap, AS_PriorityQueue, JPS_List, JPS_Heap}
public class AStar : MonoBehaviour
{
    private HexGrid _grid;
    public Transform Source, Target;
    public Optimization Optimization;
    private List<Hexagon> _oList = new List<Hexagon>();
    private HashSet<Hexagon> _cList = new HashSet<Hexagon>();
    private Heap<Hexagon> _oListHeap;

    private void Awake()
    {
        _grid = GetComponent<HexGrid>();
        _oListHeap = new Heap<Hexagon>(_grid.MaxHeapSize);
    }

    private void Update()
    {

    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        switch (Optimization)
        {
            case Optimization.AS_List:
                PathFind_LIST(request, callback);
                break;
            case Optimization.AS_Heap:
                PathFind_HEAP(request, callback);
                break;
            case Optimization.AS_PriorityQueue:
                PathFind_PRIORITYQUEUE(request, callback);
                break;
            case Optimization.JPS_List:
                PathFind_JPS_LIST(request, callback);
                break;
            case Optimization.JPS_Heap:
                PathFind_JPS_HEAP(request, callback);
                break;
        }
    }

    public void PathFind_LIST(PathRequest request, Action<PathResult> callback)
    {
        
        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(request.PathStart);
        var endHex = _grid.HexFromPoint(request.PathEnd);

        var openSet = new List<Hexagon>();
        var closedSet = new HashSet<Hexagon>();

        openSet.Add(startHex);

        while (openSet.Count > 0)
        {
            var currentHex = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost >= currentHex.FCost && openSet[i].FCost != currentHex.FCost) continue;
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
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        callback(new PathResult(path, foundPath, request.Callback));
    }

    public void PathFind_HEAP(PathRequest request, Action<PathResult> callback)
    {
        
        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(request.PathStart);
        var endHex = _grid.HexFromPoint(request.PathEnd);

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
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        callback(new PathResult(path, foundPath, request.Callback));
    }
    public void PathFind_PRIORITYQUEUE(PathRequest request, Action<PathResult> callback)
    {

        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(request.PathStart);
        var endHex = _grid.HexFromPoint(request.PathEnd);

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
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        callback(new PathResult(path, foundPath, request.Callback));
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

    public void PathFind_JPS_LIST(PathRequest request, Action<PathResult> callback)
    {
        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(request.PathStart);
        var endHex = _grid.HexFromPoint(request.PathEnd);

        _oList.Add(startHex);
        while (_oList.Count > 0)
        {
            var currentHex = _oList[0];
            for (int i = 1; i < _oList.Count; i++)
            {
                if (_oList[i].FCost < currentHex.FCost && _oList[i].FCost != currentHex.FCost) continue;
                if (_oList[i].HCost < currentHex.HCost)
                {
                    currentHex = _oList[i];
                }
            }

            if (currentHex == endHex)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + "ms WITH JPS LIST");
                foundPath = true;
                break;
            }

            IdentifySuccessors(currentHex, endHex);

            _cList.Add(currentHex);
        }
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        callback(new PathResult(path, foundPath, request.Callback));
    }

    public void PathFind_JPS_HEAP(PathRequest request, Action<PathResult> callback)
    {
        var sw = new Stopwatch();
        sw.Start();
        var path = new Path();

        bool foundPath = false;

        var startHex = _grid.HexFromPoint(request.PathStart);
        var endHex = _grid.HexFromPoint(request.PathEnd);

        _oListHeap.Add(startHex);
        while (_oListHeap.Count > 0)
        {
            var currentHex = _oListHeap.RemoveFirst();
            _cList.Add(currentHex);
            if (currentHex == endHex)
            {
                sw.Stop();
                print("Path found: " + sw.ElapsedMilliseconds + "ms WITH JPS HEAP");
                foundPath = true;
                break;
            }

            IdentifySuccessors(currentHex, endHex);

            
        }
        if (foundPath)
        {
            path = RetracePath(startHex, endHex);
        }
        callback(new PathResult(path, foundPath, request.Callback));
    }

    public void IdentifySuccessors(Hexagon current, Hexagon end)
    {
        // DIRECTIONS :                             NE, E, SE, SW,  W, NW
        // HEX NEIGHBOR ARRAY COORDINATES:          0,  1,  2,  3,  4,  5
        // CARDINALS:                               ^       ^       ^       [NE, SE,  W]
        // DIAGONALS:                                   ^       ^       ^   [E,  SW, NW]
        
        for (int i = 0; i < 6; i++)
        {
            // if i is 0, 2, 4 we are cardinal
            // if i is 1, 3, 5 we are diagonal
            var neighbor = current.GetNeighbor(i);
            // Do not consider null neighbors as it means current is at some edge of the grid.
            if (IsValidNeighbor(current, neighbor))
            {
                var jumpHex = Jump(current, neighbor, i, end);

                if (jumpHex != null)
                {
                    if (!_oListHeap.Contains(jumpHex) && !_cList.Contains(jumpHex))
                    {
                        jumpHex.Parent = current;
                        int newCostToJumpHex = current.GCost + GetDistance(current, jumpHex);
                        jumpHex.GCost = newCostToJumpHex;
                        jumpHex.HCost = GetDistance(jumpHex, end);
                        _oListHeap.Add(jumpHex);
                    }
                    else
                    {
                        _oListHeap.UpdateItem(jumpHex);
                    }
                }
            }
        }
    }

    public Hexagon Jump(Hexagon current, Hexagon next, int direction, Hexagon end)    
    {
        // If neighbor is blocked, we can't jump there. If neighbor doesn't exist 
        // since current was on the edge of the grid, we can't jump to neighbor.
        if (next == null || !next.Walkable) return null;

        // If the next node is the end node, we have found the target so return it.
        if (next.Equals(end)) return next;

        // Diagonal case
        if (direction == 1 || direction == 3 || direction == 5)
        {
            if (current.GetNeighbor((direction + 5) % 6) != null && next.GetNeighbor((direction + 5) % 6) != null)
                if (!current.GetNeighbor((direction + 5) % 6).Walkable && next.GetNeighbor((direction + 5) % 6).Walkable)
                    return next;
            if (current.GetNeighbor((direction + 1) % 6) != null && next.GetNeighbor((direction + 1) % 6) != null)
                if (!current.GetNeighbor((direction + 1) % 6).Walkable && next.GetNeighbor((direction + 1) % 6).Walkable)
                    return next;
            var nextCardinal_1 = next.GetNeighbor((direction + 5) % 6);
            var nextCardinal_2 = next.GetNeighbor((direction + 1) % 6);
            if (Jump(next, nextCardinal_1, (direction + 5) % 6, end) != null ||
                Jump(next, nextCardinal_2, (direction + 1) % 6, end) != null)
                return next;
        }
        // Cardinal case
        else
        {
            var possibleDirections = new List<int>() {0, 2, 4};
            possibleDirections.Remove((int) ((CellDirection) direction).Opposite());

            if (direction == possibleDirections[0])
            {
                if (next.GetNeighbor(direction) != null && current.GetNeighbor((direction + 5) % 6) != null)
                    if (next.GetNeighbor(direction).Walkable && !current.GetNeighbor((direction + 5) % 6).Walkable)
                        if (next.GetNeighbor((direction + 5) % 6).Walkable)
                            return next;
                if (next.GetNeighbor(direction) != null && current.GetNeighbor((direction + 1) % 6) != null)
                    if (next.GetNeighbor(direction).Walkable && !current.GetNeighbor((direction + 1) % 6).Walkable)
                        if (next.GetNeighbor((direction + 1) % 6).Walkable)
                            return next;
            }
            else
            {
                if (next.GetNeighbor(direction) != null && next.GetNeighbor((direction + 1) % 6) != null)
                    if (next.GetNeighbor(direction).Walkable && !current.GetNeighbor((direction + 1) % 6).Walkable)
                        if (next.GetNeighbor((direction + 1) % 6).Walkable)
                            return next;
                if (next.GetNeighbor(direction) != null && next.GetNeighbor((direction + 5) % 6) != null)
                    if (next.GetNeighbor(direction).Walkable && !current.GetNeighbor((direction + 5) % 6).Walkable)
                        if (next.GetNeighbor((direction + 5) % 6).Walkable)
                            return next;
            }
        }
        // No forced neighbors, so continue in the same direction
        return Jump(next, next.GetNeighbor(direction), direction, end);
    }

    public bool IsValidNeighbor(Hexagon hex, Hexagon neighbor)
    {
        return neighbor != null && neighbor.Walkable && !_cList.Contains(neighbor) && !neighbor.Equals(hex);
    }

    private static int GetDistance(Hexagon a, Hexagon b)
    {
        return Mathf.Max(Mathf.Abs(a.Coordinates.X - b.Coordinates.X),
            Mathf.Abs(a.Coordinates.Y - b.Coordinates.Y), Mathf.Abs(a.Coordinates.Z - b.Coordinates.Z));
    }
}
