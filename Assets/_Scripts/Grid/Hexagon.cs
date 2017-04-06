using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Priority_Queue;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Hexagon : MonoBehaviour, IHeapItem<Hexagon>
{
    // PUBLIC
    public float Size;
    public bool Walkable;
    public int GCost;
    public int HCost;
    public Coordinates Coordinates;
    public Text Label;
    // PRIVATE
    private Mesh _cellMesh;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private Hexagon[] _neighbors;
    // PROPERTY
    public int FCost { get { return GCost + HCost; } }
    public Renderer Renderer { get; private set; }
    public Hexagon Parent { get; set; }
    public List<Hexagon> Neighbors
    {
        get { return _neighbors.Where(neighbor => neighbor != null).ToList(); }
    }

    public int HeapIndex { get; set; }

    /// <summary>
    /// Initialize all undeclared attributes created above.
    /// </summary>
    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = _cellMesh = new Mesh();
        _cellMesh.name = "Hexagon Mesh";
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uvs = new List<Vector2>();
        Renderer = GetComponent<Renderer>();
        _neighbors = new Hexagon[6];
        gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshCollider>().sharedMesh = _cellMesh;
    }

    /// <summary>
    /// Used for initializing/creating the mesh for the hex.
    /// </summary>
    public void Triangulate()
    {
        /* Clear any old info in the arrays. */
        _cellMesh.Clear();
        _vertices.Clear();
        _uvs.Clear();
        _triangles.Clear();
        /* Center vertex of each hexagon aligned to the center of the game object in the scene. */
        var center = gameObject.transform.parent.localPosition;
        /* For each corner: */
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(HexCorner(center, Size, i + 1), HexCorner(center, Size, i), center);
            _cellMesh.vertices = _vertices.ToArray();
            _cellMesh.triangles = _triangles.ToArray();
            _cellMesh.uv = _uvs.ToArray();
        }
        _cellMesh.RecalculateNormals();
    }
    /// <summary>
    /// Given three vertices, it creates a triangle, by adding the vertices and uvs to 
    /// the data structures of the mesh.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        /* Keep track of where we left off the last time this method was called. Prevents vertices 
         * overlapping previous vertices. */
        int vertexIndex = _vertices.Count;
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
        _uvs.Add(new Vector2(v1.x, v1.z));
        _uvs.Add(new Vector2(v2.x, v2.z));
        _uvs.Add(new Vector2(v3.x, v3.z));
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }
    /// <summary>
    /// Returns the neighbor at a given direction.
    /// </summary>
    /// <param name="direction">The direction : CellDirection units.</param>
    /// <returns>Neighbor at given direction.</returns>
    public Hexagon GetNeighbor(CellDirection direction)
    {
        return _neighbors[(int) direction];
    }
    /// <summary>
    /// Returns the neighbor at a given direction.
    /// </summary>
    /// <param name="direction">The direction : integer units.</param>
    /// <returns>Neighbor at given direction.</returns>
    public Hexagon GetNeighbor(int direction)
    {
        return _neighbors[direction];
    }
    /// <summary>
    /// Returns the neighbor at an oposite direction of what was provided.
    /// </summary>
    /// <param name="direction">The direction : CellDirection units.</param>
    /// <returns>The neighbor at opposite direction.</returns>
    public Hexagon GetNeighbor_Opposite(CellDirection direction)
    {
        return (int) direction < 3 ? _neighbors[(int) direction + 3] : _neighbors[(int) direction - 3];
    }
    /// <summary>
    /// Returns the neighbor at an oposite direction of what was provided.
    /// </summary>
    /// <param name="direction">The direction : integer units.</param>
    /// <returns>The neighbor at opposite direction.</returns>
    public Hexagon GetNeighbor_Opposite(int direction)
    {
        return direction < 3 ? _neighbors[direction + 3] : _neighbors[direction - 3];
    }
    /// <summary>
    /// Setter for neighbors. It automatically finds the opposite direction.
    /// I.e. if A is at east of B, then B is at west of A.
    /// </summary>
    /// <param name="cellDirection">The direction of the neighboring hexagon in terms of this hexagon.</param>
    /// <param name="hexagon">The neighboring hexagon.</param>
    public void SetNeigbor(CellDirection cellDirection, Hexagon hexagon)
    {
        _neighbors[(int) cellDirection] = hexagon;
        hexagon._neighbors[(int) cellDirection.Opposite()] = this;
    }

    public void Hide()
    {
        Renderer.enabled = !Renderer.enabled;
    }

    private static Vector3 HexCorner(Vector3 center, float size, int i)
    {
        float angleDeg = 60f * i + 30f;
        float angleRad = Mathf.PI / 180f * angleDeg;
        return new Vector3(center.x + size * Mathf.Cos(angleRad), 0, center.z + size * Mathf.Sin(angleRad));
    }

    public int CompareTo(Hexagon other)
    {
        int compare = FCost.CompareTo(other.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(other.HCost);
        }
        return -compare;
    }
}