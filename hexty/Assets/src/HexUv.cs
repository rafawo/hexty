using UnityEngine;

/// <summary>
/// Static class with utility functions that help with
/// computing the UV coordinates of a texture with Hexagons.
/// </summary>
public static class HexUv
{
    /// <summary>
    /// Array of 2D vectors describing the six unitary uv coordinates of an hex tile with pointy orientation.
    /// </summary>
    public static Vector2[] PointyUvs = new Vector2[]
    {
        new Vector2(0.5f, 1f), // North corner - NorthEast wedge
        new Vector2(1f, 0.75f), // NorthEast corner - East wedge
        new Vector2(1f, 0.25f), // SouthEast corner - SouthEast wedge
        new Vector2(0.5f, 0f), // South corner - SouthWest wedge
        new Vector2(0f, 0.25f), // SouthWest - West wedge
        new Vector2(0f, 0.75f), // NorthWest - NorthWest wedge
    };

    /// <summary>
    /// Array of 2D vectors describing the six unitary uv coordinates of an hex tile with flat orientation.
    /// </summary>
    public static Vector2[] FlatUvs = new Vector2[]
    {
        new Vector2(0.75f, 1f), // NorthEast corner - NorthEast wedge
        new Vector2(1f, 0.5f), // East corner - SouthEast wedge
        new Vector2(0.75f, 0f), // SouthEast corner - South wedge
        new Vector2(0.25f, 0f), // SouthWest corner - SouthWest wedge
        new Vector2(0f, 0.5f), // West corner - West wedge
        new Vector2(0.25f, 1f), // NorthWest corner - North wedge
    };

    public static Vector2 GetFirstUv(HexPointyDirection direction)
        => GetFirstUv(direction, UvTexClip.Default);

    public static Vector2 GetSecondUv(HexPointyDirection direction)
        => GetSecondUv(direction, UvTexClip.Default);

    public static Vector2 GetFirstUv(HexFlatDirection direction)
        => GetFirstUv(direction, UvTexClip.Default);

    public static Vector2 GetSecondUv(HexFlatDirection direction)
        => GetSecondUv(direction, UvTexClip.Default);

    public static Vector2 GetFirstUv(HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? GetFirstUv(direction.AsHexPointy())
            : GetFirstUv(direction.AsHexFlat());

    public static Vector2 GetSecondUv(HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? GetSecondUv(direction.AsHexPointy())
            : GetSecondUv(direction.AsHexFlat());

    public static Vector2 GetCenterUv()
        => GetCenterUv(UvTexClip.Default);

    public static Vector2 GetFirstUv(HexPointyDirection direction, UvTexClip uvClip)
        => ComputeUv(PointyUvs, (int)direction, uvClip);

    public static Vector2 GetSecondUv(HexPointyDirection direction, UvTexClip uvClip)
        => ComputeUv(PointyUvs, ((int)direction + 1) % 6, uvClip);

    public static Vector2 GetFirstUv(HexFlatDirection direction, UvTexClip uvClip)
        => ComputeUv(FlatUvs, (int)direction, uvClip);

    public static Vector2 GetSecondUv(HexFlatDirection direction, UvTexClip uvClip)
        => ComputeUv(FlatUvs, ((int)direction + 1) % 6, uvClip);

    public static Vector2 GetFirstUv(HexDirection direction, UvTexClip uvClip)
        => direction.Orientation == HexOrientation.Pointy
            ? GetFirstUv(direction.AsHexPointy(), uvClip)
            : GetFirstUv(direction.AsHexFlat(), uvClip);

    public static Vector2 GetSecondUv(HexDirection direction, UvTexClip uvClip)
        => direction.Orientation == HexOrientation.Pointy
            ? GetSecondUv(direction.AsHexPointy(), uvClip)
            : GetSecondUv(direction.AsHexFlat(), uvClip);

    public static Vector2 GetCenterUv(UvTexClip uvClip)
        => uvClip.Compute(new Vector2(0.5f, 0.5f));

    private static Vector2 ComputeUv(Vector2[] unitaryUvs, int direction, UvTexClip uvClip)
        => uvClip.Compute(unitaryUvs[direction]);
}