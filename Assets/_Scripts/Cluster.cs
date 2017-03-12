/// <summary>
/// Creates a cluster of nodes, that is aware of its location within a larger grid
/// </summary>
public class Cluster
{
    public Node[,] Nodes { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public Cluster(int sideLength, int x, int y)
    {
        Nodes = new Node[sideLength, sideLength];
        X = x;
        Y = y;
    }
    /// <summary>
    /// Places a node at a desired index within the cluster's grid of nodes
    /// </summary>
    /// <param name="node">The node to be placed</param>
    /// <param name="x">The x index</param>
    /// <param name="y">The y index</param>
    public void Set(Node node, int x, int y)
    {
        Nodes[x, y] = node;
    }
}