using UnityEngine;

/// <summary>
/// Possible orientations for an hexagon.
/// </summary>
public enum HexOrientation
{
    /// <summary>
    /// Pointy orientation hexagons have their first corner
    /// directly North from the center.
    ///
    /// <para>
    ///      /\ -x
    ///    /    \
    ///  /        \ +x
    /// |          |+z
    /// |          |
    /// |          |-z
    ///  \        / -y
    ///    \    /
    ///      \/ +y
    /// </para>
    /// </summary>
    Pointy,

    /// <summary>
    /// Flat orientation hexagons have their first two corners
    /// horizontally parallel directly North from the center.
    ///
    /// <para>
    ///    +y    -y
    ///     ------
    ///   /        \ -x
    ///  /          \
    /// /            \ +x
    /// \            / +z
    ///  \          /
    ///   \        / -z
    ///     -------
    /// </para>
    /// </summary>
    Flat,
}

/// <summary>
/// Class that determines various hexagonal grid metrics.
/// All of the settings are based on the outer radius.
/// </summary>
public class HexMetrics
{
    /// <summary>
    /// Outer radius of an Hex tile.
    /// The outer radius is the distance from the hexagon's center point to any vertex.
    /// </summary>
    public readonly float OuterRadius;

    /// <summary>
    /// Inner radius of an Hex tile.
    /// The inner radius times two is the distance from the center of a given hex tile
    /// to the center of a neighbor's hex tile.
    /// Cannot be set to a value, since this will always correspond to the square root
    /// of 3 divided by 2 times the outer radius; based on the Pythagorean theorem.
    /// </summary>
    public readonly float InnerRadius;

    /// <summary>
    /// Array of 3D vectors describing the six corners of an hex tile with pointy orientation.
    /// These vectors treat axis X and Z as the plane where the hexagon tiles are layed out.
    /// </summary>
    public readonly Vector3[] PointyCorners;

    /// <summary>
    /// Array of 3D vectors describing the six corners of an hex tile with flat orientation.
    /// These vectors treat axis X and Z as the plane where the hexagon tiles are layed out.
    /// </summary>
    public readonly Vector3[] FlatCorners;

    /// <summary>
    /// Constructor that calculates once the inner radius and corners of a pointy oriented
    /// hex tile, based on the size in float units of the outer radius.
    /// </summary>
    /// <param name="outerRadius">Float units for the outer radius of a pointy oriented hex tile.</param>
    public HexMetrics(float outerRadius)
    {
        OuterRadius = outerRadius;
        InnerRadius = OuterRadius * (float)(System.Math.Sqrt(3) / 2);
        PointyCorners = new Vector3[]
        {
            new Vector3(0f, 0f, OuterRadius), // North corner - NorthEast wedge
            new Vector3(InnerRadius, 0f, 0.5f * OuterRadius), // NorthEast corner - East wedge
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius), // SouthEast corner - SouthEast wedge
            new Vector3(0f, 0f, -OuterRadius), // South corner - SouthWest wedge
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius), // SouthWest - West wedge
            new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius), // NorthWest - NorthWest wedge
        };
        FlatCorners = new Vector3[]
        {
            new Vector3(0.5f * OuterRadius, 0f, InnerRadius), // NorthEast corner - NorthEast wedge
            new Vector3(OuterRadius, 0f, 0f), // East corner - SouthEast wedge
            new Vector3(0.5f * OuterRadius, 0f, -InnerRadius), // SouthEast corner - South wedge
            new Vector3(-0.5f * OuterRadius, 0f, -InnerRadius), // SouthWest corner - SouthWest wedge
            new Vector3(-OuterRadius, 0f, 0f), // West corner - West wedge
            new Vector3(-0.5f * OuterRadius, 0f, InnerRadius), // NorthWest corner - North wedge
        };
    }

    public Vector3 GetFirstCorner(HexPointyDirection direction)
        => PointyCorners[(int)direction];

    public Vector3 GetSecondCorner(HexPointyDirection direction)
        => PointyCorners[((int)direction + 1) % 6];

    public Vector3 GetFirstCorner(HexFlatDirection direction)
        => FlatCorners[(int)direction];

    public Vector3 GetSecondCorner(HexFlatDirection direction)
        => FlatCorners[((int)direction + 1) % 6];

    public Vector3 GetFirstCorner(HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? GetFirstCorner(direction.AsHexPointy())
            : GetFirstCorner(direction.AsHexFlat());

    public Vector3 GetSecondCorner(HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? GetSecondCorner(direction.AsHexPointy())
            : GetSecondCorner(direction.AsHexFlat());
}
