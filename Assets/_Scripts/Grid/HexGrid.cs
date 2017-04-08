using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(AStar), typeof(PathRequestManager))]
public class HexGrid : MonoBehaviour
{
    public bool DrawHeatMap;
    public bool DrawRegions;
    public Vector2 Dimensions;
    public Canvas GridCanvas;
    public Text CellLabelPrefab;
    public Hexagon HexagonPrefab;
    public Agent CopPrefab;
    public Agent Cop;
    public Agent RobberPrefab;
    public Agent Robber;
    public Hexagon[,] Hexagons { get; private set; }
    public LayerMask UnwalkableMask;
    public LayerMask WalkableMask;
    public float InnerRadius { get; private set; }
    public float OuterRadius { get; private set; }
    public RegionType[] Regions;
    public Dictionary<int, int> RegionValueDictionary = new Dictionary<int, int>();
    public int MaxHeapSize
    {
        // The maximum possible heap size for creating an array big enough in the heap.
        get { return (int) Dimensions.x * (int) Dimensions.y; }
    }
    


    private void Awake()
    {
        // Calculation of inner and outer radius based on hex size given so that 
        // hexes are placed at the correct location with the correct size.
        OuterRadius = HexagonPrefab.Size;
        InnerRadius = Mathf.Sqrt(3) / 2 * OuterRadius;
        Hexagons = new Hexagon[(int) Dimensions.x,(int) Dimensions.y];

        // Detection of regions accross the map based on the layers of the colliders set
        foreach (var region in Regions)
        {
            WalkableMask.value |= region.RegionLayerMask.value;
            RegionValueDictionary.Add((int) Mathf.Log(region.RegionLayerMask.value, 2), region.RegionValue);
        }
        // Populating the grid
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                CreateCells(x, y);
            }
        }
        // Triangulating each hex in the grid with the respective mesh data set
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
        int regionValue = 0;
        // Raycasting for the region layers and detecting which region does each cell fall into
        if (hex.Walkable)
        {
            var ray = new Ray(hex.transform.position + Vector3.up * 50, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, WalkableMask))
            {
                RegionValueDictionary.TryGetValue(hit.collider.gameObject.layer, out regionValue);
            }
        }
        hex.RegionValue = regionValue;
        hex.X = x;
        hex.Y = z;
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

    // Places the cop at a random location within a specific range in the city.
    // If the cop did not exist in the scene before this, it will create it.
    public void PlaceCop()
    {
        if (Cop == null)
        {
            var coplocation = FindRandomCopLocation();
            Cop = Instantiate(CopPrefab);
            Cop.transform.position = coplocation.transform.position;
            var cameras = FindObjectsOfType<CameraFOV>();
            foreach (var cameraFov in cameras)
            {
                cameraFov.Cop = Cop;
            }
        }
        else
        {
            var coplocation = FindRandomCopLocation();
            Cop.transform.position = coplocation.transform.position;
        }
    }

    // Finds a random, walkable, hex for the cop to spawn to
    public Hexagon FindRandomCopLocation()
    {
        while (true)
        {
            int x = UnityEngine.Random.Range(45, 66);
            int y = UnityEngine.Random.Range(85, 116);
            if (Hexagons[x, y].Walkable) return Hexagons[x, y];
        }
    }

    // Places the robber at a random location within a specific range in the city.
    // If the robber did not exist in the scene before this, it will create it.
    public void PlaceRobber()
    {
        if (Robber == null)
        {
            var robberlocation = FindRandomRobberLocation();
            Robber = Instantiate(RobberPrefab);
            Robber.transform.position = robberlocation.transform.position;

            var cameras = FindObjectsOfType<CameraFOV>();
            foreach (var cameraFov in cameras)
            {
                cameraFov.Robber = Robber;
            }
        }
        else
        {
            var robberlocation = FindRandomRobberLocation();
            Robber.transform.position = robberlocation.transform.position;
        }
    }

    // Resets the cells in the grid, as well as the flags set for catching the robber
    // Needed for resetting the simulation, and sending a path finding request for the robber 
    // To find a path to the bank
    public void RobberToBank()
    {
        foreach (var hexagon in Hexagons)
        {
            hexagon.HCost = 0;
            hexagon.GCost = 0;
        }
        foreach (var agent in FindObjectsOfType<Agent>())
        {
            agent.IsCaught = false;
        }
        foreach (var cameraFov in FindObjectsOfType<CameraFOV>())
        {
            cameraFov.FoundCutOff = false;
            cameraFov.DetectedRobber = false;
        }
        var bank = GameObject.Find("Bank");
        Robber.RequestPath(bank.transform);
    }

    // Finds a random, walkable hex for the robber to spawn to
    public Hexagon FindRandomRobberLocation()
    {
        while (true)
        {
            int x = UnityEngine.Random.Range(0, 26);
            int y = UnityEngine.Random.Range(0, 26);
            if (Hexagons[x, y].Walkable) return Hexagons[x, y];
        }
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

    // Used for the heat map generation. Essentially calculates the F Costs of the
    // visited cells and calculates a color interpolation between the max and min (excluding 0 F Cost)
    // values of the F Costs of each hex, compares the hex's values in that range, and interpolate/displays
    // accordingly.
    private void OnDrawGizmos()
    {
        if (Hexagons == null) return;
        if (DrawRegions)
        {
            foreach (var hexagon in Hexagons)
            {
                if (hexagon.RegionValue == 0)
                {
                    Gizmos.color = Color.grey;
                }
                if (hexagon.RegionValue == 1)
                {
                    Gizmos.color = Color.green;
                }
                if (hexagon.RegionValue == 2)
                {
                    Gizmos.color = Color.blue;
                }
                if (hexagon.RegionValue == 3)
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawMesh(hexagon.GetComponent<MeshFilter>().mesh, hexagon.transform.position);
            }
        }
        if (DrawHeatMap)
        {
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
                }
                else if (cell.FCost >= q1 && cell.FCost < q2)
                {
                    Gizmos.color = Color.Lerp(Color.cyan, Color.green, value);
                }
                else if (cell.FCost >= q2 && cell.FCost < q3)
                {
                    Gizmos.color = Color.Lerp(Color.green, Color.yellow, value);
                }
                else if (cell.FCost >= q3 && cell.FCost <= q4)
                {
                    Gizmos.color = Color.Lerp(Color.yellow, Color.red, value);
                }
                else
                {
                    Gizmos.color = new Color(0.3f, 0.3f, 0.3f);
                }
                Gizmos.DrawMesh(cell.GetComponent<MeshFilter>().mesh, cell.transform.position);
            }
        

        }

    }


}