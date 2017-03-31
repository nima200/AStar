using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    public Vector2 Dimensions;
    public Canvas GridCanvas;
    public Text CellLabelPrefab;
    public Cell CellPrefab;
    public Cell[,] Cells { get; private set; }
    public List<Node> Path;
    public LayerMask UnwalkableMask;
    private void Awake()
    {
        Cells = new Cell[(int) Dimensions.x,(int) Dimensions.y];

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                CreateCells(x, y);
            }
        }
        Triangulate(Cells);
    }

    private void CreateCells(int x, int z)
    {
        // Resolve coordinates
        float xCoordinate = (x + z * 0.5f - z / 2) * (Metrics.InnerRadius * 2f);
        const float yCoordinate = 0f;
        float zCoordinate = z * (Metrics.OuterRadius * 1.5f);

        // Translate coordinate into world position, instantiate the prefab at that location
        var position = new Vector3(xCoordinate, yCoordinate, zCoordinate);
        var cell = Cells[x, z] = Instantiate(CellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.position = position;
        cell.Coordinates = Coordinates.FromOffsetCoordinates(x, z);

        // Instantiate a node for the cell
        var worldPoint = cell.transform.position;
        bool walkable = !(Physics.CheckSphere(worldPoint, Metrics.InnerRadius, UnwalkableMask));
        cell.InstantiateNode(walkable, worldPoint, x, z);

        // Set neighbors of the cell
        if (x > 0)
        {
            cell.SetNeigbor(CellDirection.W, Cells[x - 1, z]);
        }
        if (z > 0)
        {
            // EVEN ROWS
            if ((z & 1) == 0)
            {
                cell.SetNeigbor(CellDirection.SE, Cells[x, z - 1]);
                if (x > 0)
                {
                    cell.SetNeigbor(CellDirection.SW, Cells[x - 1, z - 1]);
                }
            }
            // ODD ROWS
            else
            {
                cell.SetNeigbor(CellDirection.SW, Cells[x, z - 1]);
                if (x < Dimensions.x - 1)
                {
                    cell.SetNeigbor(CellDirection.SE, Cells[x + 1, z - 1]);
                }
            }
        }
        // Display cell coordinates on top of it
        var label = cell.Label = Instantiate(CellLabelPrefab);
        label.rectTransform.SetParent(GridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.Coordinates.ToStringOnSeparateLines();
    }

    /// <summary>
    /// Cell from a given world position.
    /// If the cell is not found, throws an argument out of bounds
    /// </summary>
    /// <param name="position">The world position to find a cell</param>
    /// <returns>The cell at the given world position</returns>
    public Cell CellFromWorld(Vector3 position)
    {
        var coordinates = Coordinates.FromPosition(position);
        for (int x = 0; x < Cells.GetLength(0); x++)
        {
            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                if (Cells[x, y].Coordinates.Equals(coordinates))
                {
                    return Cells[x, y];
                }
            }
        }
        throw new ArgumentOutOfRangeException("The cell asked for does not exist");
    }

    private static void Triangulate(Cell[,] cells)
    {
        foreach (var cell in cells)
        {
            if (cell != null)
            {
                cell.Triangulate();
            }
        }
    }
    /// <summary>
    /// Returns a list of the neighbors of the cell requestsed
    /// </summary>
    /// <param name="cell">The cell to find neighbors of</param>
    /// <returns>The neighbors of the cell</returns>
    public List<Cell> GetNeighbors(Cell cell)
    {
        return cell.Neighbors.Where(neighbor => neighbor != null).ToList();
    }

    private void OnDrawGizmos()
    {
        if (Cells == null) return;
        foreach (var cell in Cells)
        {
            Gizmos.color = cell.Node.Walkable ? Color.white : Color.red;
            if (Path != null)
                if (Path.Contains(cell.Node))
                    Gizmos.color = Color.black;
            Gizmos.DrawMesh(cell.GetComponent<MeshFilter>().mesh, cell.transform.position);
        }
    }
}
