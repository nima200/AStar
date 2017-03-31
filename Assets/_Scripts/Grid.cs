using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask UnwalkableMask;
    public Vector2 GridWorldSize;
    public float NodeRadius;
    public Node[,] Nodes { get; private set; }
    private float _nodeDiameter;
    private int _gridSizeX, _gridSizeY;
    public List<Node> Path;

    [UsedImplicitly]
    private void Start()
    {
        _nodeDiameter = NodeRadius * 2;
        _gridSizeX = Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter);
        _gridSizeY = Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        Nodes = new Node[_gridSizeX, _gridSizeY];
        // Left edge of the world
        var worldBottomLeft = transform.position - Vector3.right * _gridSizeX / 2 - Vector3.forward * GridWorldSize.y / 2; 

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                var worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + NodeRadius) +
                                     Vector3.forward * (y * _nodeDiameter + NodeRadius);
                // True if we don't collide with anything in the unwalkable mask
                bool walkable = !(Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask));
                Nodes[x,y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorld(Vector3 worldPosition)
    {
        // Trying to get a percentage of the position of the node, given left = 0 %,  bottom = 0 %
        // If in left: 0, if middle: 0.5, if right: 1.0
        float percentX = (worldPosition.x + GridWorldSize.x / 2) / GridWorldSize.x;
        float percentY = (worldPosition.z + GridWorldSize.y / 2) / GridWorldSize.y;
        // Clamp between 0 and 1: If player is outside of grid, do not attempt to find node that player is on
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        // Getting actual index of node based on percentages calculated above
        int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX); // - 1 on gsX to avoid falling out of array
        int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY); // - 1 on gsY to avoid falling out of array

        return Nodes[x, y];
    }

    [UsedImplicitly]
    /*private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(GridWorldSize.x, 1, GridWorldSize.y));
        if (Nodes == null) return;
        foreach (var node in Nodes)
        {
            Gizmos.color = (node.Walkable) ? Color.white : Color.red;
            if (Path != null)
            {
                if (Path.Contains(node))
                {
                    Gizmos.color = Color.black;
                }
            }
            Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.1f));
            
        }
    }*/

    public List<Node> GetNeighbors(Node node)
    {
        // We need to first know where the node is in the grid
        var neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Skip the neighbor at location x = 0, y = 0 cause it's the cell itself.
                if (x == 0 && y == 0) continue;

                int neighborX = node.GridX + x;
                int neighborY = node.GridY + y;
                // Making sure to not return a neighbor that doesnt exist because it's a location out of the grid
                if (neighborX >= 0 && neighborX < _gridSizeX && neighborY >= 0 && neighborY < _gridSizeY)
                {
                    neighbors.Add(Nodes[neighborX, neighborY]);
                }
            }
        }
        return neighbors;
    }
}
