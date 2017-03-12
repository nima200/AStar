using UnityEngine;

public class Node {
    public bool Walkable;
    public Vector3 WorldPosition;
    public int GCost; // Distance from start node
    public int HCost; // Distance from destination node
    public int GridX; // For a node to keep track of where it is
    public int GridY; // For a node to keep track of where it is
    public int FCost // Total cost
    {
        get { return GCost + HCost; }
    }
    public Node Parent;

    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY)
    {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = gridX;
        GridY = gridY;
    }
}
