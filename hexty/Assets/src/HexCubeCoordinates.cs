using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines the type of offset system used to layout hex coordinates.
/// </summary>
public enum HexOffsetType
{
    /// <summary>
    /// Hex Pointy - shoves odd rows by +1/2 column.
    /// Hex Flat - shoves odd columns by +1/2 row.
    /// </summary>
    Odd,

    /// <summary>
    /// Hex Pointy - shoves even rows by +1/2 column.
    /// Hex Flat - shoves even columns by +1/2 row.
    /// </summary>
    Even,
}

public static class HexOffsetTypeUtilities
{
    /// <summary>
    /// Returns an HexCubeCoordinate that uses x axis as rows and z axis as columns.
    /// x (rows) grows positive to the right, z (columns) grows positive up.
    /// </summary>
    /// <param name="type">Type of offset.</param>
    /// <param name="orientation">Hex orientation.</param>
    /// <param name="row">Row offset</param>
    /// <param name="column">Column offset</param>
    /// <returns>HexCubeCoordinate corresponding to the offset coordinate with the appropriate system and hex orientation.</returns>
    public static HexCubeCoordinates ToHexCube(this HexOffsetType type, HexOrientation orientation, int x, int z)
    {
        if (orientation == HexOrientation.Pointy)
        {
            return HexCubeCoordinates.FromXZ(
                type == HexOffsetType.Odd
                    ? x - ((z - (z&1)) / 2)
                    : x - ((z + (z&1)) / 2),
                z);
        }
        else
        {
            return HexCubeCoordinates.FromXZ(
                x,
                type == HexOffsetType.Odd
                    ? z - ((x - (x&1)) / 2)
                    : z - ((x + (x&1)) / 2));
        }
    }

    /// <summary>
    /// Returns a Vector3 with x and z coordinates filled in equivalent to the row and column for a given HexCubeCoordinate.
    /// </summary>
    /// <param name="type">Type of offset</param>
    /// <param name="orientation">Hex orientation.</param>
    /// <param name="coordinates">Hex cube coordinates</param>
    /// <returns>Offset coordinates for a given HexCubeCoordinates.</returns>
    public static Vector3 FromHexCube(this HexOffsetType type, HexOrientation orientation, HexCubeCoordinates coordinates)
    {
        var x = coordinates.X;
        var z = coordinates.Z;

        if (orientation == HexOrientation.Pointy)
        {
            return new Vector3(
                type == HexOffsetType.Odd
                    ? x + ((z - (z&1)) / 2)
                    : x + ((z + (z&1)) / 2),
                0f,
                z);
        }
        else
        {
            return new Vector3(
                x,
                0f,
                type == HexOffsetType.Odd
                    ? z + ((x - (x&1)) / 2)
                    : z + ((x + (x&1)) / 2));
        }
    }
}

[System.Serializable]
public class HexCubeCoordinates
{
    private Vector3 vector;
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    [SerializeField]
    private int x;

    [SerializeField]
    private int z;

    public HexCubeCoordinates(int x, int y, int z)
        : this(new Vector3(x, y, z)) { }

    public HexCubeCoordinates(Vector3 vector)
    {
        if ((0 != (vector.x + vector.y + vector.z)) ||
            0 != ((vector.x % 1) + (vector.y % 1) + (vector.z % 1)))
        {
            throw new System.ArgumentException($"Vector ({vector}) is an invalid Hex coordinate");
        }

        X = (int)vector.x;
        Y = (int)vector.y;
        Z = (int)vector.z;

        x = X;
        z = Z;

        this.vector = vector;
    }

    public HexCubeCoordinates(HexCubeCoordinates other)
        : this (other.vector) { }

    public static HexCubeCoordinates FromXY(int x, int y)
        => new HexCubeCoordinates(x, y, - (x + y));

    public static HexCubeCoordinates FromXZ(int x, int z)
        => new HexCubeCoordinates(x, - (x + z), z);

    public static HexCubeCoordinates FromYZ(int y, int z)
        => new HexCubeCoordinates(- (y + z), y, z);

    public static readonly HexCubeCoordinates Origin
        = new HexCubeCoordinates(Vector3.zero);

    public static HexCubeCoordinates Zero
        { get { return Origin; } }

    public Vector3 AsVector()
        => new Vector3(X, Y, Z);

    public static HexCubeCoordinates FromOffsetCoordinates(int x, int z, HexOrientation orientation, HexOffsetType type)
        => type.ToHexCube(orientation, x, z);

    public Vector3 AsOffsetCoordinates(HexOrientation orientation, HexOffsetType type)
        => type.FromHexCube(orientation, this);

    /// <summary>
    /// Rounds a given x,y,z hex cube coordinate to the nearest integer based hex cube coordinate.
    /// </summary>
    /// <param name="point">Hex cube coordinate with floating point values.</param>
    /// <returns>Valid hex cube coordinate.</returns>
    public static HexCubeCoordinates Round(Vector3 point)
    {
        int iX = Mathf.RoundToInt(point.x);
        int iY = Mathf.RoundToInt(point.y);
        int iZ = Mathf.RoundToInt(point.z);

        if ((iX + iY + iZ) != 0)
        {
            // Near the edges between hexagons, the rounding of coordinates leads to trouble.
            // The further away you get from the center of a cell, the more rounding occurs.
            // Assume then the coordinate that got rounded the most is incorrect.
            // Based on the above, we discard the coordinate with the largest rounding delta,
            // and reconstruct it from the other two.

            float dX = Mathf.Abs(point.x - iX);
            float dY = Mathf.Abs(point.y - iY);
            float dZ = Mathf.Abs(-(point.x + point.y) - iZ);

            if ((dX > dY) && (dX > dZ))
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return HexCubeCoordinates.FromXZ(iX, iZ);
    }

    /// <summary>
    /// Creates an hex cube coordinate from a given position.
    /// This uses the X and Z coordinates as the 2D plane where the
    /// hexagonal grid is layed out.
    /// </summary>
    /// <param name="position">X and Z coordinates to be translated to hex cube coordinates.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <returns></returns>
    public static HexCubeCoordinates FromPosition(Vector3 position, HexMetrics metrics, HexOrientation orientation)
    {
        if (orientation == HexOrientation.Pointy)
        {
            float x = position.x / (metrics.InnerRadius * 2f);
            float y = -x;
            float offset = position.z / (metrics.OuterRadius * 3f);
            x -= offset;
            y -= offset;
            return Round(new Vector3(x, y, - (x + y)));
        }
        else
        {
            float z = position.z / (metrics.InnerRadius * 2f);
            float y = -z;
            float offset = position.x / (metrics.OuterRadius * 3f);
            z -= offset;
            y -= offset;
            return Round(new Vector3(- (y + z), y, z));
        }
    }

    /// <summary>
    /// Returns the center position of a given hex cube coordinate,
    /// using the X and Z coordinates as the 2D plane where the
    /// hexagonal grid is layed out.
    /// </summary>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    /// <returns>Center position of a given HexCubeCoordinate.</returns>
    public Vector3 ToPosition(HexMetrics metrics, HexOrientation orientation, HexOffsetType type)
    => orientation == HexOrientation.Pointy
        ? new Vector3(
            type == HexOffsetType.Odd
                ? (X + (Z * 0.5f) - ((Z&1) / 2)) * metrics.InnerRadius * 2f
                : (X + (Z * 0.5f) + ((Z&1) / 2)) * metrics.InnerRadius * 2f,
            0,
            Z * metrics.OuterRadius * 1.5f)
        : new Vector3(
            X * metrics.OuterRadius * 1.5f,
            0,
            type == HexOffsetType.Odd
                ? (Z + (X * 0.5f) - ((Z&1) / 2)) * metrics.InnerRadius * 2f
                : (Z + (X * 0.5f) + ((Z&1) / 2)) * metrics.InnerRadius * 2f);

    /// <summary>
    /// Returns the center position of a given hex cube coordinate,
    /// using the X and Z coordinates as the 2D plane where the
    /// hexagonal grid is layed out.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinate whose center position is returned.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    /// <returns>Center position of a given HexCubeCoordinate.</returns>
    public static Vector3 ToPosition(HexCubeCoordinates coordinates, HexMetrics metrics, HexOrientation orientation, HexOffsetType type)
        => coordinates.ToPosition(metrics, orientation, type);

    public HexCubeCoordinates GetNeighbor(HexPointyDirection direction, int scale = 1)
        => this + (scale * direction.UnitVector());

    public HexCubeCoordinates GetNeighbor(HexFlatDirection direction, int scale = 1)
        => this + (scale * direction.UnitVector());

    public HexCubeCoordinates GetNeighbor(HexDirection direction, int scale = 1)
        => this + (scale * direction.UnitVector());

    public HexCubeCoordinates GetNeighbor(int direction, int scale = 1)
        => this + (scale * HexDirectionUtilities.UnitVector(direction));

    public HexCubeCoordinates[] GetNeighbors(int scale = 1)
        => new HexCubeCoordinates[]
        {
            GetNeighbor(0, scale),
            GetNeighbor(1, scale),
            GetNeighbor(2, scale),
            GetNeighbor(3, scale),
            GetNeighbor(4, scale),
            GetNeighbor(5, scale),
        };

    public static HexCubeCoordinates[] GetNeighbors(HexCubeCoordinates coordinates, int scale = 1)
        => coordinates.GetNeighbors(scale);

    /// <summary>
    /// Returns the three vertices that create a mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="type">Odd or Even.</param>
    public Vector3[] GetMeshTriangle(HexDirection direction, HexMetrics metrics, HexOffsetType type)
    {
        var center = ToPosition(metrics, direction.Orientation, type);
        var v1 = center + metrics.GetFirstCorner(direction);
        var v2 = center + metrics.GetSecondCorner(direction);
        return new Vector3[3] { center, v1, v2 };
    }

    /// <summary>
    /// Returns the three vertices that create a mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="type">Odd or Even.</param>
    public Vector3[] GetMeshTriangle(HexPointyDirection direction, HexMetrics metrics, HexOffsetType type)
        => GetMeshTriangle(new HexDirection(HexOrientation.Pointy, (int)direction), metrics, type);

    /// <summary>
    /// Returns the three vertices that create a mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="type">Odd or Even.</param>
    public Vector3[] GetMeshTriangle(HexFlatDirection direction, HexMetrics metrics, HexOffsetType type)
        => GetMeshTriangle(new HexDirection(HexOrientation.Flat, (int)direction), metrics, type);

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    public static Vector2[] GetMeshUvs(HexDirection direction)
    {
        var uv1 = HexUv.GetFirstUv(direction);
        var uv2 = HexUv.GetSecondUv(direction);
        return new Vector2[3] { HexUv.GetCenterUv(), uv1, uv2 };
    }

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    public static Vector2[] GetMeshUvs(HexPointyDirection direction)
        => GetMeshUvs(new HexDirection(HexOrientation.Pointy, (int)direction));

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    public static Vector2[] GetMeshUvs(HexFlatDirection direction)
        => GetMeshUvs(new HexDirection(HexOrientation.Flat, (int)direction));

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="uvClip">Supplies an UV clip to use to compute UV coordinates.</param>
    public static Vector2[] GetMeshUvs(HexDirection direction, UvTexClip uvClip)
    {
        var uv1 = HexUv.GetFirstUv(direction, uvClip);
        var uv2 = HexUv.GetSecondUv(direction, uvClip);
        return new Vector2[3] { HexUv.GetCenterUv(uvClip), uv1, uv2 };
    }

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="uvClip">Supplies an UV clip to use to compute UV coordinates.</param>
    public static Vector2[] GetMeshUvs(HexPointyDirection direction, UvTexClip uvClip)
        => GetMeshUvs(new HexDirection(HexOrientation.Pointy, (int)direction), uvClip);

    /// <summary>
    /// Returns the three uv coordinates that correspond to the mesh triangle for a given wedge on the specified direction.
    /// </summary>
    /// <param name="direction">Direction within the hex that determines the wedge triangle.</param>
    /// <param name="uvClip">Supplies an UV clip to use to compute UV coordinates.</param>
    public static Vector2[] GetMeshUvs(HexFlatDirection direction, UvTexClip uvClip)
        => GetMeshUvs(new HexDirection(HexOrientation.Flat, (int)direction), uvClip);

    /// <summary>
    /// Given a point in space, determines the direction within the hex where it is contained.
    /// </summary>
    /// <param name="point">Point in space.</param>
    /// <param name="metrics">Hex metrics used for spacing.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    public HexDirection GetRelativeMeshTriangleDirection(Vector3 point, HexMetrics metrics, HexOrientation orientation, HexOffsetType type)
    {
        var position = ToPosition(metrics, orientation, type);
        return orientation == HexOrientation.Pointy
            ? HexPointyDirectionUtilities.GetPointDirection(position, point).AsDirection()
            : HexFlatDirectionUtilities.GetPointDirection(position, point).AsDirection();
    }

    public override string ToString()
        => $"({X}, {Y}, {Z})";

    public string ToStringOnSeparateLines()
        => $"{X}\n{Y}\n{Z}";

    public override int GetHashCode()
        => this.ToString().GetHashCode();

    public override bool Equals(object obj)
        => obj is HexCubeCoordinates coordinates && coordinates.ToString().Equals(this.ToString());

    // Operator overloads

    public static HexCubeCoordinates operator +(HexCubeCoordinates a, HexCubeCoordinates b)
        => new HexCubeCoordinates(a.vector + b.vector);

    public static HexCubeCoordinates operator +(HexCubeCoordinates a, Vector3 b)
        => new HexCubeCoordinates(a.vector + b);

    public static HexCubeCoordinates operator -(HexCubeCoordinates a, HexCubeCoordinates b)
        => new HexCubeCoordinates(a.vector - b.vector);

    public static HexCubeCoordinates operator -(HexCubeCoordinates a, Vector3 b)
        => new HexCubeCoordinates(a.vector - b);

    public static HexCubeCoordinates operator -(HexCubeCoordinates a)
        => new HexCubeCoordinates(-a.vector);

    public static HexCubeCoordinates operator *(HexCubeCoordinates a, float d)
        => new HexCubeCoordinates(a.vector * d);

    public static HexCubeCoordinates operator *(float d, HexCubeCoordinates a)
        => new HexCubeCoordinates(a.vector * d);

    public static HexCubeCoordinates operator /(HexCubeCoordinates a, float d)
        => new HexCubeCoordinates(a.vector / d);

    public static bool operator ==(HexCubeCoordinates lhs, HexCubeCoordinates rhs)
        => lhs.vector == rhs.vector;

    public static bool operator !=(HexCubeCoordinates lhs, HexCubeCoordinates rhs)
        => lhs.vector != rhs.vector;
}

/// <summary>
/// Simple abstraction of a translation unit for an hex cube coordinate.
/// Essentially, how many steps to take on a given direction.
/// </summary>
public struct HexTranslationUnit
{
    /// <summary>
    /// Direction within the hex to move towards to.
    /// </summary>
    public readonly HexDirection Direction;

    /// <summary>
    /// How many steps to take in a given direction.
    /// </summary>
    public readonly int Steps;

    /// <summary>
    /// Vector representation of this translation unit.
    /// </summary>
    public Vector3 TranslationVector
    {
        get
        {
            return Direction.UnitVector() * Steps;
        }
    }

    public HexTranslationUnit(HexDirection direction, int steps)
    {
        Direction = direction;
        Steps = steps;
    }

    public void Apply(ref HexCubeCoordinates coordinates)
    {
        coordinates += TranslationVector;
    }
}

/// <summary>
/// All valid hex angles in degrees, used for rotation of an hex cube coordinate.
/// </summary>
public enum HexAngleDegree
{
    d60 = 60,
    d120 = 120,
    d180 = 180,
    d240 = 240,
    d300 = 300,
    d360 = 360,
}

/// <summary>
/// Determines the movement/rotation from a given coordinate.
/// </summary>
public enum HexAngleRotation
{
    ClockWise,
    CounterClockWise,
}

/// <summary>
/// Determines if an spiral movement is done inwards or outwards.
/// </summary>
public enum HexSpiralDirection
{
    Inwards,
    Outwards,
}

public static class HexCubeCoordinatesUtilities
{
    /// <summary>
    /// Translates a given hex cube coordinate with the supplied translation units.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinates.</param>
    /// <param name="units">Translation units.</param>
    /// <returns></returns>
    public static HexCubeCoordinates Translate(
        this HexCubeCoordinates coordinates,
        IEnumerable<HexTranslationUnit> units
        )
    {
        var translatedCoordinates = new HexCubeCoordinates(coordinates);

        foreach (var unit in units)
        {
            unit.Apply(ref translatedCoordinates);
        }

        return translatedCoordinates;
    }

    /// <summary>
    /// Returns the magnitude of an hex cube coordinate.
    /// Essentially, the distance of a given hex cube coordinate respect the origin.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinate to get the magnitude from.</param>
    /// <returns>Magnitude of a given hex cube coordinate.</returns>
    public static int Magnitude(this HexCubeCoordinates coordinates)
        => (Mathf.Abs(coordinates.X) + Mathf.Abs(coordinates.Y) + Mathf.Abs(coordinates.Z)) / 2;

    /// <summary>
    /// Returns the distance between two hex cube coordinates.
    /// </summary>
    /// <param name="a">Hex cube coordinate a.</param>
    /// <param name="b">Hex cube coordinate b.</param>
    /// <returns>Distance between two hex cube coordinates.</returns>
    public static int Distance(this HexCubeCoordinates a, HexCubeCoordinates b)
        => Magnitude(a - b);

    /// <summary>
    /// Rotates an hex cube coordinate clockwise/counterclockwise by the specified degrees.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinates to rotate.</param>
    /// <param name="center">Rotation point.</param>
    /// <param name="d">Degrees to rotate.</param>
    /// <param name="rotation">ClockWise or CounterClockWise.</param>
    /// <returns>Rotated hex cube coordinate.</returns>
    public static HexCubeCoordinates Rotate(
        this HexCubeCoordinates coordinates,
        HexCubeCoordinates center,
        HexAngleDegree d = HexAngleDegree.d60,
        HexAngleRotation rotation = HexAngleRotation.ClockWise
        )
    {
        var rotatee = coordinates - center;

        for (int i = 0; i < ((int)d / (int)HexAngleDegree.d60); ++i)
        {
            if (rotation == HexAngleRotation.ClockWise)
            {
                rotatee = new HexCubeCoordinates(-rotatee.Z, -rotatee.X, -rotatee.Y);
            }
            else
            {
                rotatee = new HexCubeCoordinates(-rotatee.Y, -rotatee.Z, -rotatee.X);
            }
        }

        return rotatee + center;
    }

    /// <summary>
    /// Reflects a given hex cube coordinate within the specified hex line.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinate to reflect.</param>
    /// <param name="line">Hex line that determines the reflection point.</param>
    /// <returns>Reflected hex cube coordinates.</returns>
    public static HexCubeCoordinates Reflect(this HexCubeCoordinates coordinates, HexGeometry.HexLine line)
    {
        var reflectee = coordinates.AsVector();

        switch (line.Axis)
        {
        case HexGeometry.HexAxis.X:
            reflectee = HexCubeCoordinates.FromXZ((int)(reflectee.x - line.K), (int)reflectee.z).AsVector();
            reflectee = new Vector3(-reflectee.x, -reflectee.z, -reflectee.y);
            reflectee = HexCubeCoordinates.FromXZ((int)(reflectee.x + line.K), (int)reflectee.z).AsVector();
            break;
        case HexGeometry.HexAxis.Y:
            reflectee = HexCubeCoordinates.FromYZ((int)(reflectee.y - line.K), (int)reflectee.z).AsVector();
            reflectee = new Vector3(-reflectee.z, -reflectee.y, -reflectee.x);
            reflectee = HexCubeCoordinates.FromYZ((int)(reflectee.y + line.K), (int)reflectee.z).AsVector();
            break;
        case HexGeometry.HexAxis.Z:
            reflectee = HexCubeCoordinates.FromYZ((int)reflectee.y, (int)(reflectee.z - line.K)).AsVector();
            reflectee = new Vector3(-reflectee.y, -reflectee.x, - reflectee.z);
            reflectee = HexCubeCoordinates.FromYZ((int)reflectee.y, (int)(reflectee.z + line.K)).AsVector();
            break;
        }

        return new HexCubeCoordinates(reflectee);
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates that satisfy a ring movement over a specific radius.
    /// The ring movement starts at the specified neighbor direction as an integer,
    /// regardless of hex orientation, since it produces valid movement by iterating over the neighbor values.
    ///
    /// The first and the last hex cube coordinates in the enumerable list are the starting point at the specified
    /// scaled neighbor.
    ///
    /// For example, choosing neighbor 4...
    ///
    /// In Pointy orientation:
    /// 4 is West neighbor, and iterates through
    ///      NorthEast
    ///      East
    ///      SouthEast
    ///      SouthWest
    ///      West
    ///      NorthWest
    ///
    /// In Flat:
    /// 4 is NorthWest neighbor, and iterates through
    ///      NorthEast
    ///      SouthEast
    ///      South
    ///      SouthWest
    ///      NorthWest
    ///      North
    /// </summary>
    /// <param name="coordinates">Center hex cube coordinate from where the ring will be done.</param>
    /// <param name="radius">Ring radius.</param>
    /// <param name="direction">Hex direction as an integer from 0 to 5.</param>
    /// <param name="rotation">Hex angle rotation (ClockWise vs CounterClockWise).</param>
    /// <param name="exists">Predicate that determines if a given hex cube coordinate exists and should be added to the result.</param>
    /// <returns>IEnumerable of all valid hex cube coordinates for a given ring.</returns>
    public static IEnumerable<HexCubeCoordinates> Ring(
        this HexCubeCoordinates coordinates,
        int radius,
        int direction = 0,
        HexAngleRotation rotation = HexAngleRotation.ClockWise,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        var coords = coordinates.GetNeighbor(direction, radius);
        var d = rotation == HexAngleRotation.ClockWise
            ? direction + 2
            : direction + 4;

        for (int i = 0; i < 6; ++i)
        {
            for (int j = 0; j < radius; ++j)
            {
                if (exists == null || exists.Invoke(coords))
                {
                    yield return coords;
                }

                coords = coords.GetNeighbor(d);
            }

            d = rotation == HexAngleRotation.ClockWise
                ? d + 1
                : d - 1;
        }
    }

    /// <summary>
    /// Spiral through all of the rings contained in the specified radius.
    /// This will spiral clockwise/counterclockwise, inwards/outwards and from the specified
    /// scaled neighbor on each ring.
    /// </summary>
    /// <param name="coordinates">Supplies the center of the spiral.</param>
    /// <param name="radius">Supplies the spiral's radius.</param>
    /// <param name="inwards">Determines if the spiral is done inwards, starting in the outermost ring. Otherwise spirals outwards starting from the center and innermost ring.</param>
    /// <param name="direction"></param>
    /// <param name="rotation"></param>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists. If it does, it'll be included int he result</param>
    /// <returns></returns>
    public static IEnumerable<HexCubeCoordinates> Spiral(
        this HexCubeCoordinates coordinates,
        int radius,
        HexSpiralDirection spiralDirection = HexSpiralDirection.Outwards,
        int direction = 0,
        HexAngleRotation rotation = HexAngleRotation.ClockWise,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        if (spiralDirection == HexSpiralDirection.Outwards)
        {
            yield return coordinates;
        }

        for (
            int r = spiralDirection == HexSpiralDirection.Inwards
                ? radius
                : 1;
            spiralDirection == HexSpiralDirection.Inwards
                ? r >= 1
                : r <= radius;
            r = spiralDirection == HexSpiralDirection.Inwards
                ? r - 1
                : r + 1
            )
        {
            foreach (var coord in coordinates.Ring(r, direction, rotation, exists))
            {
                yield return coord;
            }
        }

        if (spiralDirection == HexSpiralDirection.Inwards)
        {
            yield return coordinates;
        }
    }

    /// <summary>
    /// Floods from a given hex cube coordinate the specified movement radius.
    /// This is essentially a breadth-first traversal from a given coordinate.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinate from where the flood starts.</param>
    /// <param name="movement">The amount of rings to flood to.</param>
    /// <param name="exists">Predicate that determines if a given hex cube coordinate exists. If it does, includes it in the result.</param>
    /// <returns></returns>
    public static HashSet<HexCubeCoordinates> Flood(
        this HexCubeCoordinates coordinates,
        int movement,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        var visited = new HashSet<HexCubeCoordinates>();
        visited.Add(coordinates);
        var fringes = new List<List<HexCubeCoordinates>>();
        fringes.Add(new List<HexCubeCoordinates>());
        fringes[0].Add(coordinates);

        for (int i = 1; i <= movement; ++i)
        {
            fringes.Add(new List<HexCubeCoordinates>());

            foreach (var coords in fringes[i - 1])
            {
                for (int d = 0; d < 6; ++d)
                {
                    var neighbor = coords.GetNeighbor(d);

                    if (!visited.Contains(neighbor) && (exists == null || exists.Invoke(neighbor)))
                    {
                        visited.Add(neighbor);
                        fringes[i].Add(neighbor);
                    }
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// Creates an interpolated hex cube line, and if any of the hex cube coordinates of such line
    /// does not exist the target hex cube coordinate is treated as not visible.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinate from where the other is tried to be seen.</param>
    /// <param name="other">Target hex cube coordinates to see.</param>
    /// <param name="metrics">Hex metrics used to determine spacing when creating the interpolated line.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    /// <param name="exists">Predicate that determines if a given hex coordinate exists.</param>
    /// <returns>True if the target hex cube coordinate is visible from a given hex cube coordinate.</returns>
    public static bool Visible(
        this HexCubeCoordinates coordinates,
        HexCubeCoordinates other,
        HexMetrics metrics,
        HexOrientation orientation,
        HexOffsetType type,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        if (exists == null || coordinates == other)
        {
            return true;
        }
        else if (!exists.Invoke(coordinates) || !exists.Invoke(other))
        {
            return false;
        }

        foreach (var coord in HexGeometry.HexInterpolatedLine.Range(coordinates, other, metrics, orientation, type))
        {
            if (!exists.Invoke(coord))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns an IEnumerable with all the visible hex cube coordinates from a given one on a given radius.
    /// Uses an outward, clockwise spiral fashion to iterate over the visible hex cube coordinates.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinates from where the field of view is calculated.</param>
    /// <param name="radius">Radius that constraints the limit of the field of view.</param>
    /// <param name="metrics">Hex metrics used to create an interpolated hex line when determining visibility.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    /// <param name="exists">Predicate used to determine if a given hex cube coordinate exists. Non existing hex cube coordinates block visibility.</param>
    /// <returns></returns>
    public static IEnumerable<HexCubeCoordinates> FieldOfView(
        this HexCubeCoordinates coordinates,
        int radius,
        HexMetrics metrics,
        HexOrientation orientation,
        HexOffsetType type,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        foreach (var coord in coordinates.Spiral(
            radius, HexSpiralDirection.Outwards, 0, HexAngleRotation.ClockWise, exists))
        {
            if (coord.Visible(coordinates, metrics, orientation, type, exists))
            {
                yield return coord;
            }
        }
    }

    /// <summary>
    /// Using A* path finding algorithm, with the distance between two hex cube coordinates as the heuristic,
    /// returns the shortest path between two hex coordinates.
    /// </summary>
    /// <param name="coordinates">Hex cube coordinates from where the path starts.</param>
    /// <param name="destination">Hex cube coordinates where the path ends.</param>
    /// <param name="exists">Predicate used to determine if a given hex coordinate exists. Helps with reducing the search space.</param>
    /// <returns></returns>
    public static Queue<HexCubeCoordinates> FindPath(
        this HexCubeCoordinates coordinates,
        HexCubeCoordinates destination,
        Predicate<HexCubeCoordinates> exists = null
        )
        => AStar<HexCubeCoordinates>.Search(
            coordinates, destination, x => x.GetNeighbors(), null, exists, (x, y) => x.Distance(y));
}

