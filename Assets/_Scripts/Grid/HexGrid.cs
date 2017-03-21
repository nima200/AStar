using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public Vector2 Dimensions;
    public Canvas GridCanvas;
    public Text CellLabelPrefab;
    public Cell CellPrefab;
    public Cell[,] Cells { get; private set; }

    private void Awake()
    {
        Cells = new Cell[(int) Dimensions.x,(int) Dimensions.y];
        for (int z = 0, i = 0; z < Dimensions.x; z++)
        {
            for (int x = 0; x < Dimensions.y; x++)
            {
                CreateCells(x, z);
            }
        }
        Triangulate(Cells);
    }

    private void CreateCells(int x, int z)
    {
        float xCoordinate = (x + z * 0.5f - z / 2) * (Metrics.InnerRadius * 2f);
        const float yCoordinate = 0f;
        float zCoordinate = z * (Metrics.OuterRadius * 1.5f);

        var position = new Vector3(xCoordinate, yCoordinate, zCoordinate);
        var cell = Cells[x, z] = Instantiate(CellPrefab);

        cell.transform.SetParent(transform, false);
        cell.transform.position = position;
        cell.Coordinates = Coordinates.FromOffsetCoordinates(x, z);

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
        var label = cell.Label = Instantiate(CellLabelPrefab);
        label.rectTransform.SetParent(GridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.Coordinates.ToStringOnSeparateLines();
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

    private void Start () {
		
	}

	private void Update () {
		
	}
}
