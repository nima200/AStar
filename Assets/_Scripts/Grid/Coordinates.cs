using System;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Local

[System.Serializable]
public struct Coordinates
{
    [SerializeField]
    private int _x, _z;

    public int X { get { return _x; } }

    public int Z { get { return _z; } }

    public int Y { get { return -X - Z; } }

    public Coordinates(int x, int z) : this()
    {
        _x = x;
        _z = z;
    }

    public static Coordinates FromOffsetCoordinates(int x, int z)
    {
        return new Coordinates(x - (z / 2), z);
    }

    public static Coordinates FromPosition(Vector3 position)
    {
        float x = position.x / (Metrics.InnerRadius * 2f);
        float y = -x;
        float offset = position.z / (Metrics.OuterRadius * 3f);
        x -= offset;
        y -= offset;
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);
        if (iX + iY + iZ == 0) return new Coordinates(iX, iZ);
        float dX = Mathf.Abs(x - iX);
        float dY = Mathf.Abs(y - iY);
        float dZ = Mathf.Abs(-x - y - iZ);
        if (dX > dY && dX > dZ)
        {
            iX = -iY - iZ;
        } else if (dZ > dY)
        {
            iZ = -iX - iY;
        }
        return new Coordinates(iX, iZ);
    }

    public bool Equals(Coordinates other)
    {
        return other.X == X && other.Z == Z;
    }

    public override string ToString()
    {
        return "( " + X + ", " + Y + ", " + Z + " )";
    }

    public string ToStringOnSeparateLines()
    {
        return X + "\n" + Y + "\n" + Z;
    }
}