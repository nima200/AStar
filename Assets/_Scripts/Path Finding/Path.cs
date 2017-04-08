using System;
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

    public int IndexOf(Vector3 other)
    {
        return Array.IndexOf(Waypoints, other);
    }

    public int DistanceOf(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(IndexOf(b) - IndexOf(a));
    }
}