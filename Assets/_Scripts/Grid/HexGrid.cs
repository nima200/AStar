using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    public bool DrawHeatMap;
    public bool DrawPath;
    public Vector2 Dimensions;
    public Canvas GridCanvas;
    public Text CellLabelPrefab;
    public Hexagon HexagonPrefab;
    public Hexagon[,] Hexagons { get; private set; }
    public List<Hexagon> Path;
    public LayerMask UnwalkableMask;
    public float InnerRadius { get; private set; }
    public float OuterRadius { get; private set; }
    

    private void Awake()
    {
        OuterRadius = HexagonPrefab.Size;
        InnerRadius = Mathf.Sqrt(3) / 2 * OuterRadius;
        Hexagons = new Hexagon[(int) Dimensions.x,(int) Dimensions.y];

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                CreateCells(x, y);
            }
        }
        Triangulate(Hexagons);
    }

    private void CreateCells(int x, int z)
    {
        // Resolve coordinates
        float xCoordinate = (x + z * 0.5f - z / 2) * (InnerRadius * 2f);
        const float yCoordinate = 0f;
        float zCoordinate = z * (OuterRadius * 1.5f);

        // Translate coordinate into world position, instantiate the prefab at that location
        var position = new Vector3(xCoordinate, yCoordinate, zCoordinate);
        var hex = Hexagons[x, z] = Instantiate(HexagonPrefab);
        hex.transform.SetParent(transform, false);
        hex.transform.position = position;
        hex.Coordinates = Coordinates.FromOffsetCoordinates(x, z);
        hex.name = hex.Coordinates.ToString();
        // Instantiate a node for the hexagon
        hex.Walkable = !(Physics.CheckSphere(hex.transform.position, InnerRadius, UnwalkableMask));

        // Set neighbors of the hexagon
        if (x > 0)
        {
            hex.SetNeigbor(CellDirection.W, Hexagons[x - 1, z]);
        }
        if (z > 0)
        {
            // EVEN ROWS
            if ((z & 1) == 0)
            {
                hex.SetNeigbor(CellDirection.SE, Hexagons[x, z - 1]);
                if (x > 0)
                {
                    hex.SetNeigbor(CellDirection.SW, Hexagons[x - 1, z - 1]);
                }
            }
            // ODD ROWS
            else
            {
                hex.SetNeigbor(CellDirection.SW, Hexagons[x, z - 1]);
                if (x < Dimensions.x - 1)
                {
                    hex.SetNeigbor(CellDirection.SE, Hexagons[x + 1, z - 1]);
                }
            }
        }
        // Display hexagon coordinates on top of it
        var label = hex.Label = Instantiate(CellLabelPrefab);
        label.rectTransform.SetParent(GridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = hex.Coordinates.ToStringOnSeparateLines();
    }

    /// <summary>
    /// Hexagon from a given world position.
    /// If the hexagon is not found, throws an argument out of bounds
    /// </summary>
    /// <param name="position">The world position to find a hexagon</param>
    /// <returns>The hexagon at the given world position</returns>
    public Hexagon HexFromPoint(Vector3 position)
    {
        var coordinates = Coordinates.FromPosition(position, this);
        for (int x = 0; x < Hexagons.GetLength(0); x++)
        {
            for (int y = 0; y < Hexagons.GetLength(1); y++)
            {
                if (Hexagons[x, y].Coordinates.Equals(coordinates))
                {
                    return Hexagons[x, y];
                }
            }
        }
        throw new ArgumentOutOfRangeException("There is no hexagon at world position " + position);
    }
    /// <summary>
    /// Given a 2D array of hexagons, iteratively triangulates the individual hexagons in the array.
    /// </summary>
    /// <param name="hexagons">2D array of hexagons.</param>
    private static void Triangulate(Hexagon[,] hexagons)
    {
        foreach (var cell in hexagons)
        {
            if (cell != null)
            {
                cell.Triangulate();
            }
        }
    }
    /// <summary>
    /// Returns a list of the neighbors of the hexagon requestsed
    /// </summary>
    /// <param name="hexagon">The hexagon to find neighbors of</param>
    /// <returns>The neighbors of the hexagon</returns>
    public List<Hexagon> GetNeighbors(Hexagon hexagon)
    {
        return hexagon.Neighbors;
    }

    private void OnDrawGizmos()
    {
        if (!DrawHeatMap) return;
        if (Hexagons == null) return;
        var fCosts = (from Hexagon hexagon in Hexagons select hexagon.FCost).ToList();
        fCosts.RemoveAll(value => value == 0);
        float maxFCost = fCosts.Max(t => t);
        float minFCost = fCosts.Min(t => t);
        float q = (maxFCost - minFCost) / 4;
        float q1 = minFCost + q;
        float q2 = minFCost + 2 * q;
        float q3 = minFCost + 3 * q;
        float q4 = minFCost + 4 * q;
        foreach (var cell in Hexagons)
        {
            float value = cell.FCost / (maxFCost - minFCost);
            if (cell.FCost >= minFCost && cell.FCost < q1)
            {
                Gizmos.color = Color.Lerp(Color.blue, Color.cyan, value);
            } else if (cell.FCost >= q1 && cell.FCost < q2)
            {
                Gizmos.color = Color.Lerp(Color.cyan, Color.green, value);
            } else if (cell.FCost >= q2 && cell.FCost < q3)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.yellow, value);
            } else if (cell.FCost >= q3 && cell.FCost <= q4)
            {
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, value);
            }
            else
            {
                Gizmos.color = new Color(0.3f, 0.3f, 0.3f);
            }
            if (Path != null && DrawPath)
                if (Path.Contains(cell))
                    Gizmos.color = Color.black;
            Gizmos.DrawMesh(cell.GetComponent<MeshFilter>().mesh, cell.transform.position);
        }
    }


}
