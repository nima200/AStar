using UnityEngine;

public class Metrics
{
    /// <summary>
    /// Hexes have inner and outer radius. The inner is derived from the outer. 
    /// Using pythagorean theorem, you can calculate the triangles Needed to see 
    /// that if the outer radius is X, the inner radius becomes X * (Sqrt(3)/2).
    /// </summary>
    public const float OuterRadius = 10f;
    public const float InnerRadius = OuterRadius * 0.866025404f;
    /* Generic locations of the corners according to the inner and outer radius set. 
     * The order in which these vertices are placed in this array determine whether 
     * the pointy edge of the are on top when generated or a flat edge. */
    public static Vector3[] Corners =
    {
        new Vector3(0f, 0f, OuterRadius),
        new Vector3(InnerRadius, 0f, 0.5f * OuterRadius),
        new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
        new Vector3(0f, 0f, -OuterRadius),
        new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius),
        new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius)
    };
}