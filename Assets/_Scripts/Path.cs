using UnityEngine;

public class Path
{
    public Vector3[] Waypoints;

    public Path()
    {
        Waypoints = new Vector3[0];
    }

    public Path(Vector3[] waypoints)
    {
        Waypoints = waypoints;
    }
}