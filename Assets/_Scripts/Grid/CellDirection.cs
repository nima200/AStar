public enum CellDirection
{
    NE, E, SE, SW, W, NW
}

public static class CellDirectionExtensions
{
    public static CellDirection Opposite(this CellDirection cellDirection)
    {
        return (int) cellDirection < 3 ? cellDirection + 3 : cellDirection - 3;
    }
}