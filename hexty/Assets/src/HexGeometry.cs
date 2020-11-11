using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collection of definitions and utilities to work with Hexagonal grid geometry.
/// </summary>
namespace HexGeometry
{

/// <summary>
/// Represents all possibles axis on an hexagonal grid.
/// </summary>
public enum HexAxis
{
    /// <summary>
    /// Vertical axis with "forwards slash" direction.
    /// </summary>
    X,

    /// <summary>
    /// Vertical axis with "backwards slash" direction.
    /// </summary>
    Y,

    /// <summary>
    /// Horizontal axis.
    /// </summary>
    Z,

    /// <summary>
    /// Used to represent failure to resolve an axis when dealing with hex geometry.
    /// </summary>
    None,
}

public static class HexAxisUtilities
{
    /// <summary>
    /// Given two coordinates, figures out the axis on which they are parallel.
    /// This can be used to quickly check if they have a same coordinate value
    /// on a given axis.
    /// </summary>
    public static HexAxis ResolveParallelAxis(HexCubeCoordinates a, HexCubeCoordinates b)
    {
        if (a.X == b.X)
        {
            return HexAxis.X;
        }
        else if (a.Y == b.Y)
        {
            return HexAxis.Y;
        }
        else if (a.Z == b.Z)
        {
            return HexAxis.Z;
        }
        else
        {
            return HexAxis.None;
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates that walks an hex line by moving
    /// 1...n and -1...-n hex cube coordinates within the hex line's axis from a given
    /// hex cube coordinate. The supplied hex cube coordinate determines the X, Y or Z value
    /// as 'K' for the hex line at the specified axis.
    /// </summary>
    /// <param name="start">Hex cube coordinate from where the range starts.</param>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    /// <param name="stopOnNonExistingBounds">Determines if the range should stop when both boundaries of the axis reach non existing coordinates.</param>
    public static IEnumerable<HexCubeCoordinates> Range(
        this HexAxis axis,
        HexCubeCoordinates start,
        Predicate<HexCubeCoordinates> exists = null,
        bool stopOnNonExistingBounds = true
        )
    {
        if (exists == null)
        {
            exists = x => x != null;
        }

        if (exists.Invoke(start))
        {
            yield return start;
        }

        if (axis != HexAxis.None)
        {
            bool higherBoundFound = false;
            bool lowerBoundFound = false;
            var coords = new HexCubeCoordinates[] { start, start };

            for (int i = 1; i != int.MaxValue; ++i)
            {
                switch (axis)
                {
                case HexAxis.X:
                    coords[0] = HexCubeCoordinates.FromXY(start.X, start.Y + i);
                    coords[1] = HexCubeCoordinates.FromXY(start.X, start.Y - i);
                    break;
                case HexAxis.Y:
                    coords[0] = HexCubeCoordinates.FromXY(start.X + i, start.Y);
                    coords[1] = HexCubeCoordinates.FromXY(start.X - i, start.Y);
                    break;
                case HexAxis.Z:
                    coords[0] = HexCubeCoordinates.FromXZ(start.X + i, start.Z);
                    coords[1] = HexCubeCoordinates.FromXZ(start.X - i, start.Z);
                    break;
                }

                if (exists.Invoke(coords[0]) && !higherBoundFound)
                {
                    yield return coords[0];
                }
                else
                {
                    higherBoundFound = true;
                }

                if (exists.Invoke(coords[1]) && !lowerBoundFound)
                {
                    yield return coords[1];
                }
                else
                {
                    lowerBoundFound = true;
                }

                if (higherBoundFound && lowerBoundFound && stopOnNonExistingBounds)
                {
                    break;
                }
            }
        }
    }
}

/// <summary>
/// Describes an hexagonal grid line, that moves consistently on a given axis.
/// Hex lines are defined by equations:
///     x = k;
///     y = k;
///     z = k;
/// </summary>
public struct HexLine
{
    public readonly HexAxis Axis;
    public readonly int K;

    public HexLine(HexAxis axis, int k)
    {
        Axis = axis;
        K = k;
    }

    public HexLine(HexLine line)
    {
        Axis = line.Axis;
        K = line.K;
    }

    /// <summary>
    /// Resolves the hex line at which two hex cube coordinates intersect and creates it.
    /// </summary>
    /// <param name="a">Hex cube coordinates a.</param>
    /// <param name="a">Hex cube coordinates b.</param>
    public HexLine(HexCubeCoordinates a, HexCubeCoordinates b) : this(ResolveHexLine(a, b)) { }

    /// <summary>
    /// Resolves the hex line at which two hex cube coordinates intersect.
    /// </summary>
    /// <param name="a">Hex cube coordinates a.</param>
    /// <param name="a">Hex cube coordinates b.</param>
    /// <returns>HexLine at which two hex cube coordinates intersect.</returns>
    public static HexLine ResolveHexLine(HexCubeCoordinates a, HexCubeCoordinates b)
    {
        var axis = HexAxisUtilities.ResolveParallelAxis(a, b);
        switch (axis)
        {
        case HexAxis.X:
            return new HexLine(axis, a.X);
        case HexAxis.Y:
            return new HexLine(axis, a.Y);
        case HexAxis.Z:
            return new HexLine(axis, a.Z);
        default:
            return new HexLine(axis, 0);
        }
    }

    /// <summary>
    /// Returns the hex line's representation as a scaled unit vector on the axis it's defined on.
    /// </summary>
    public Vector3 AsVector()
    {
        switch (Axis)
        {
        case HexAxis.X:
            return K * Vector3.right;
        case HexAxis.Y:
            return K * Vector3.up;
        case HexAxis.Z:
            return K * Vector3.forward;
        default:
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the hex line's representation as a scaled unit vector on the axis it's defined on.
    /// </summary>
    public static Vector3 AsVector(HexAxis axis, int k)
        => new HexLine(axis, k).AsVector();

    public static bool operator ==(HexLine lhs, HexLine rhs)
        => lhs.Axis == rhs.Axis && lhs.K == rhs.K;

    public static bool operator !=(HexLine lhs, HexLine rhs)
        => lhs.Axis != rhs.Axis || lhs.K != rhs.K;

    public static implicit operator bool(HexLine line)
        => line.Axis != HexAxis.None;

    /// <summary>
    /// Determines if two hex lines are parallel.
    /// </summary>
    /// <param name="line">HexLine used to check if parallel with another HexLine.</param>
    /// <returns>True if two given hex lines are parallel.</returns>
    public bool Parallel(HexLine line)
        => line.Axis == Axis;

    /// <summary>
    /// Returns whether two hex lines intersects, and if it does, returns the intersecting
    /// hex cube coordinates through out paramater "intersection".
    /// </summary>
    /// <param name="line">HexLine to check if intersects with a given HexLine.</param>
    /// <param name="intersection">If the hex lines intersect, returns the intersecting hex cube coordinates.</param>
    /// <returns>True if two given hex lines intersect.</returns>
    public bool Intersects(HexLine line, out HexCubeCoordinates intersection)
    {
        //
        // Well known invalid intersections
        //

        if (Parallel(line) || !this || !line)
        {
            intersection = HexCubeCoordinates.Origin;
            return false;
        }

        //
        // Intersection when one line is parallel to X and other to Y
        //
        else if (Axis == HexAxis.X && line.Axis == HexAxis.Y)
        {
            intersection = new HexCubeCoordinates(new Vector3(K, line.K, - (K + line.K)));
            return true;
        }
        else if (Axis == HexAxis.Y && line.Axis == HexAxis.X)
        {
            intersection = new HexCubeCoordinates(new Vector3(line.K, K, - (K + line.K)));
            return true;
        }

        //
        // Intersection when one line is parallel to X and other to Z
        //
        else if (Axis == HexAxis.X && line.Axis == HexAxis.Z)
        {
            intersection = new HexCubeCoordinates(new Vector3(K, - (K + line.K), line.K));
            return true;
        }
        else if (Axis == HexAxis.Z && line.Axis == HexAxis.X)
        {
            intersection = new HexCubeCoordinates(new Vector3(line.K, - (K + line.K), K));
            return true;
        }

        //
        // Intersection when one line is parallel to Y and the other on Z
        //
        else if (Axis == HexAxis.Y && line.Axis == HexAxis.Z)
        {
            intersection = new HexCubeCoordinates(new Vector3(- (K + line.K), K, line.K));
            return true;
        }
        else if (Axis == HexAxis.Z && line.Axis == HexAxis.Y)
        {
            intersection = new HexCubeCoordinates(new Vector3(- (K + line.K), line.K, K));
            return true;
        }

        //
        // In theory this shouldn't happen, but if it does treat it as invalid
        //
        else
        {
            intersection = HexCubeCoordinates.Origin;
            return false;
        }
    }

    public bool Contains(HexCubeCoordinates coordinates)
    {
        switch (Axis)
        {
        case HexAxis.X:
            return coordinates.X == K;
        case HexAxis.Y:
            return coordinates.Y == K;
        case HexAxis.Z:
            return coordinates.Z == K;
        default:
            return false;
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates that walks the range on the hex axis at which
    /// a given hex line is defined from, starting at the intersecting hex cube coordinates with the specified
    /// hex line.
    /// </summary>
    /// <param name="line">HexLine that determines the intersecting hex cube coordinates.</param>
    /// <param name="exists">Predicate used to determine if hex cube coordinates is included in the result.</param>
    /// /// <param name="stopOnNonExistingBounds">Determines if the range should stop when both boundaries of the axis reach non existing coordinates.</param>
    public IEnumerable<HexCubeCoordinates> Range(
        HexLine line,
        Predicate<HexCubeCoordinates> exists = null,
        bool stopOnNonExistingBounds = true
        )
    {
        var intersection = HexCubeCoordinates.Origin;
        if (Intersects(line, out intersection))
        {
            foreach (var coords in Range(intersection, exists, stopOnNonExistingBounds))
            {
                yield return coords;
            }
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates that walks the range on the hex axis at which
    /// a given hex line is defined from, starting at the supplied intersecting hex cube coordinates.
    /// </summary>
    /// <param name="intersection">Hex cube coordinates intersecting the HexLine.</param>
    /// <param name="exists">Predicate used to determine if hex cube coordinates is included in the result.</param>
    /// <param name="stopOnNonExistingBounds">Determines if the range should stop when both boundaries of the axis reach non existing coordinates.</param>
    public IEnumerable<HexCubeCoordinates> Range(
        HexCubeCoordinates intersection,
        Predicate<HexCubeCoordinates> exists = null,
        bool stopOnNonExistingBounds = true
        )
    {
        if (Contains(intersection))
        {
            foreach (var coords in HexAxisUtilities.Range(Axis, intersection, exists, stopOnNonExistingBounds))
            {
                yield return coords;
            }
        }
    }

    public override bool Equals(object obj)
        => obj is HexLine line && this == line;

    public override int GetHashCode()
    {
        int hashCode = -204490818;
        hashCode = hashCode * -1521134295 + Axis.GetHashCode();
        hashCode = hashCode * -1521134295 + K.GetHashCode();
        return hashCode;
    }
}

public static class HexInterpolatedLine
{

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates that represent an interpolated hex line
    /// from two given hex cube coordinates.
    /// </summary>
    /// <param name="a">Hex cube coordinates where the hex interpolated line starts at.</param>
    /// <param name="b">Hex cube coordinates where the hex interpolated line ends at.</param>
    /// <param name="metrics">Hex metrics used to determine spacing.</param>
    /// <param name="orientation">Pointy or Flat.</param>
    /// <param name="type">Odd or Even.</param>
    /// <param name="exists">Determines if a given hex cube coordinate exists, if it does it's included in the result.</param>
    public static IEnumerable<HexCubeCoordinates> Range(
        HexCubeCoordinates a,
        HexCubeCoordinates b,
        HexMetrics metrics,
        HexOrientation orientation,
        HexOffsetType type,
        Predicate<HexCubeCoordinates> exists = null
        )
    {
        var distance = a.Distance(b);
        var apos = a.ToPosition(metrics, orientation, type);
        var bpos = b.ToPosition(metrics, orientation, type);
        var k = (bpos - apos) / distance;

        for (int i = 0; i <= distance; ++i)
        {
            var coord = HexCubeCoordinates.FromPosition(apos + (k * i), metrics, orientation);
            if (exists == null || exists.Invoke(coord))
            {
                yield return coord;
            }
        }
    }

}

/// <summary>
/// Represents the type of half plane.
/// </summary>
public enum HexHalfPlaneType
{
    /// <summary>
    /// All coordinates that within an axis are values greater than or equal to the half plane's K.
    /// </summary>
    High,

    /// <summary>
    /// All coordinates that within an axis are values lower than or equal to the half plane's K.
    /// </summary>
    Low,
}

/// <summary>
/// Describes an hexagonal grid half plane.
/// An hex half plane is essentially an hex line and whether values are on one side or the other
/// within the hex line's axis.
/// </summary>
public struct HexHalfPlane
{
    public readonly HexAxis Axis;
    public readonly HexHalfPlaneType Type;
    public readonly int K;

    public HexLine Line
    {
        get
        {
            return new HexLine(Axis, K);
        }
    }

    public HexHalfPlane(HexAxis axis, HexHalfPlaneType type, int k)
    {
        Axis = axis;
        Type = type;
        K = k;
    }

    private bool CheckByType(int k)
    {
        switch (Type)
        {
        case HexHalfPlaneType.High:
            return k >= K;
        case HexHalfPlaneType.Low:
            return k <= K;
        default:
            return false;
        }
    }

    public bool Contains(HexCubeCoordinates coordinates)
    {
        switch (Axis)
        {
        case HexAxis.X:
            return CheckByType(coordinates.X);
        case HexAxis.Y:
            return CheckByType(coordinates.Y);
        case HexAxis.Z:
            return CheckByType(coordinates.Z);
        default:
            return false;
        }
    }

    /// <summary>
    /// Returns whether two hex half planes intersect at their hex line, and if it does, returns the intersecting
    /// hex cube coordinates through out paramater "intersection".
    /// </summary>
    /// <param name="halfPlane">HexHalfPlane to check if intersects with a given HexHalfPlane.</param>
    /// <param name="intersection">If the hex half planes intersect, returns the intersecting hex cube coordinates.</param>
    /// <returns>True if two given hex half planes intersect.</returns>
    public bool Intersects(HexHalfPlane halfPlane, out HexCubeCoordinates intersection)
        => Line.Intersects(halfPlane.Line, out intersection);
}

/// <summary>
/// Hex triangle types.
/// </summary>
public enum HexTriangleType
{
    /// <summary>
    /// <para>
    ///  /\
    /// /  \
    /// ----
    /// </para>
    /// </summary>
    Up,

    /// <summary>
    /// <para>
    /// ____
    /// \  /
    ///  \/
    /// </para>
    /// </summary>
    Down,
}

/// <summary>
/// Triangle in an hex grid.
/// All triangles in an hexagonal grid are equilateral.
/// Described by three hex lines:
///     x = K; y = M; z = N;
/// </summary>
public class HexTriangle
{
    public readonly int K;
    public readonly int M;
    public readonly int N;
    public readonly HexTriangleType Type;
    public readonly HexCubeCoordinates[] Vertices;

    private HexTriangle() { }

    public HexTriangle(int k, int m, int n)
    {
        K = k;
        M = m;
        N = n;

        Type = - (K + N) > M ? HexTriangleType.Up : HexTriangleType.Down;
        Vertices = new HexCubeCoordinates[]
        {
            new HexCubeCoordinates(K, M, - (K + M)),
            new HexCubeCoordinates(K, - (K + N), N),
            new HexCubeCoordinates(- (M + N), M, N),
        };

        _poly = null;
    }

    public HexTriangle(HexCubeCoordinates[] vertices)
    {
        if (vertices.Length != 3)
        {
            throw new System.ArgumentException("Hex triangles can only be described with three vertices");
        }

        //
        // Regardles of where X, Y, Z axis are resolved to in the vertices array,
        // by making sure that each combination includes at least one index
        // that matches the index at the resolved axis we can directly access the value.
        //
        // For example, if the Z axis is resolved at combination v0 and v2, because the
        // resolved axis would correspond to the 3rd combination, we would get the Z
        // property from vertices[2].
        //

        var resolvedAxis = new List<HexAxis>();
        resolvedAxis.Add(HexAxisUtilities.ResolveParallelAxis(vertices[0], vertices[1]));
        resolvedAxis.Add(HexAxisUtilities.ResolveParallelAxis(vertices[1], vertices[2]));
        resolvedAxis.Add(HexAxisUtilities.ResolveParallelAxis(vertices[0], vertices[2]));

        if (resolvedAxis.Contains(HexAxis.None) ||
            resolvedAxis.FindAll(x => x == HexAxis.X).Count != 1 ||
            resolvedAxis.FindAll(x => x == HexAxis.Y).Count != 1 ||
            resolvedAxis.FindAll(x => x == HexAxis.Z).Count != 1)
        {
            new System.ArgumentException($"Hex triangle vertices must have a unique axis in common for each pair");
        }

        K = vertices[resolvedAxis.FindIndex(x => x == HexAxis.X)].X;
        M = vertices[resolvedAxis.FindIndex(x => x == HexAxis.Y)].Y;
        N = vertices[resolvedAxis.FindIndex(x => x == HexAxis.Z)].Z;

        Type = - (K + N) > M ? HexTriangleType.Up : HexTriangleType.Down;
        Vertices = vertices;

        _poly = null;
    }

    /// <summary>
    /// Returns the length of the hex triangle.
    /// </summary>
    public int Length()
        => Vertices[0].Distance(Vertices[1]) + 1;

    public bool Contains(HexCubeCoordinates coordinates)
    {
        switch (Type)
        {
        case HexTriangleType.Up:
            return coordinates.X >= K && coordinates.Y >= M && coordinates.Z >= N;
        case HexTriangleType.Down:
            return coordinates.X <= K && coordinates.Y <= M && coordinates.Z <= N;
        default:
            return false;
        }
    }

    private HexConvexPolygon _poly;

    /// <summary>
    /// HexConvexPolygon representation for an HexTriangle.
    /// </summary>
    public HexConvexPolygon ConvexPolygon
    {
        get
        {
            if (_poly == null)
            {
                _poly = new HexConvexPolygon(
                    new HexHalfPlane[] {
                        new HexHalfPlane(
                            HexAxis.X,
                            Type == HexTriangleType.Up
                                ? HexHalfPlaneType.High
                                : HexHalfPlaneType.Low,
                            K
                        ),
                        new HexHalfPlane(
                            HexAxis.Y,
                            Type == HexTriangleType.Up
                                ? HexHalfPlaneType.High
                                : HexHalfPlaneType.Low,
                            M
                        ),
                        new HexHalfPlane(
                            HexAxis.Z,
                            Type == HexTriangleType.Up
                                ? HexHalfPlaneType.High
                                : HexHalfPlaneType.Low,
                            N
                        ),
                    }
                );
            }

            return _poly;
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates with all those
    /// that are contained within this hex geometry figure.
    /// </summary>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    public IEnumerable<HexCubeCoordinates> Range(Predicate<HexCubeCoordinates> exists = null)
    {
        return ConvexPolygon.Range(exists);
    }

    /// <summary>
    /// Returns the count of hex cube coordinates contained within this hex geometry figure.
    /// </summary>
    public int Area()
    {
        var edgeLength = Vertices[0].Distance(Vertices[1]) + 1;
        return (edgeLength * (edgeLength + 1)) / 2;
    }

    /// <summary>
    /// Spawns an hex triangle based on a given hex cube coordinate, length, type and hex orientation.
    /// </summary>
    public static HexTriangle Spawn(
        HexCubeCoordinates a,
        int length,
        HexTriangleType type,
        HexOrientation orientation,
        HexAxis axis,
        bool spawnUpwards = true
        )
    {
        if (type == HexTriangleType.Up)
        {
            var vertices = new HexCubeCoordinates[]
            {
                a,
                a.GetVertex(length, HexVertexType.LowerRight, orientation, axis),
                a.GetVertex(length, HexVertexType.LowerLeft, orientation, axis),
            };

            if (spawnUpwards)
            {
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = vertices[i].GetVertex(length, HexVertexType.UpperRight, orientation, axis);
                }
            }

            return new HexTriangle(vertices);
        }
        else
        {
            var vertices = new HexCubeCoordinates[]
            {
                a,
                a.GetVertex(length, HexVertexType.UpperRight, orientation, axis),
                a.GetVertex(length, HexVertexType.UpperLeft, orientation, axis),
            };

            if (!spawnUpwards)
            {
                for (int i = 0; i < vertices.Length; ++i)
                {
                    vertices[i] = vertices[i].GetVertex(length, HexVertexType.LowerRight, orientation, axis);
                }
            }

            return new HexTriangle(vertices);
        }
    }
}

/// <summary>
/// Represents all quadrangles that can be constructed in an hexagonal grid.
/// All quadrangles in an hexagonal grid are trapezoids.
/// </summary>
public enum HexQuadrangleType
{
    /// <summary>
    /// <para>
    ///   ______
    ///  /      \
    /// /        \
    /// ----------
    /// </para>
    /// </summary>
    Up,

    /// <summary>
    /// <para>
    /// __________
    /// \        /
    ///  \      /
    ///   ------
    /// </para>
    /// </summary>
    Down,

    /// <summary>
    /// <para>
    ///   ____
    ///  /   /
    /// /   /
    /// \  /
    ///  \/
    /// </para>
    /// </summary>
    UpLeft,

    /// <summary>
    /// <para>
    /// ____
    /// \   \
    ///  \   \
    ///   \  /
    ///    \/
    /// </para>
    /// </summary>
    UpRight,

    /// <summary>
    /// <para>
    ///    /\
    ///   /  \
    ///   \   \
    ///    \   \
    ///     ----
    /// </para>
    /// </summary>
    DownLeft,

    /// <summary>
    /// <para>
    ///    /\
    ///   /  \
    ///  /   /
    /// /   /
    /// ----
    /// </para>
    /// </summary>
    DownRight,

    /// <summary>
    /// <para>
    ///   __________
    ///  /         /
    /// /         /
    /// ----------
    /// </para>
    /// </summary>
    Forward,

    /// <summary>
    /// <para>
    /// _________
    /// \        \
    ///  \        \
    ///   ----------
    /// </para>
    /// </summary>
    Backward,

    /// <summary>
    /// <para>
    ///   /\
    ///  /  \
    ///  \  /
    ///   \/
    /// </para>
    /// </summary>
    Rhomboid,

    /// <summary>
    /// Used as a placeholder quadrangle type that is not valid.
    /// </summary>
    None,
}

/// <summary>
/// Quadrangle in an hex grid.
/// All quadrangles in hexagonal grids are trapezoids strictly speaking,
/// still, we try to represent a parallelogram.
/// </summary>
public class HexQuadrangle
{
    /// <summary>
    /// The quadrangle type respect a given axis.
    /// </summary>
    public readonly Dictionary<HexAxis, HexQuadrangleType> Type;

    /// <summary>
    /// Stores the half planes that describe the quadrangle.
    /// The first two half planes will always be the high and low Z axis respectively.
    /// The remaining two will either be two X, two Z, ZX or XZ half planes.
    /// </summary>
    public readonly HexHalfPlane[] HalfPlanes;

    private HexQuadrangle() { }

    public HexQuadrangle(HexLine[] lines)
    {
        Construct(ref HalfPlanes, ref Type, lines);
    }

    public HexQuadrangle(HexCubeCoordinates[] vertices)
    {
        if (vertices.Length != 4)
        {
            throw new System.ArgumentException("Hex quadrangles can only be described with four vertices");
        }

        var xLines = new List<HexLine>();
        var yLines = new List<HexLine>();
        var zLines = new List<HexLine>();

        for (int i = 0; i < vertices.Length; ++i)
        {
            for (int j = i + 1; j < vertices.Length; ++j)
            {
                if (vertices[i] == vertices[j])
                {
                    throw new System.ArgumentException("Hex quadrangles can only be described with four distinct vertices");
                }

                if (HexAxis.X == HexAxisUtilities.ResolveParallelAxis(vertices[i], vertices[j]))
                {
                    xLines.Add(new HexLine(HexAxis.X, vertices[i].X));
                }
                else if (HexAxis.Y == HexAxisUtilities.ResolveParallelAxis(vertices[i], vertices[j]))
                {
                    yLines.Add(new HexLine(HexAxis.Y, vertices[i].Y));
                }
                else if (HexAxis.Z == HexAxisUtilities.ResolveParallelAxis(vertices[i], vertices[j]))
                {
                    zLines.Add(new HexLine(HexAxis.Z, vertices[i].Z));
                }
            }
        }

        if (xLines.Count + yLines.Count + zLines.Count != 4)
        {
            throw new System.ArgumentException("Hex quadrangles can only be described with exactly four lines");
        }

        xLines.AddRange(yLines);
        xLines.AddRange(zLines);
        Construct(ref HalfPlanes, ref Type, xLines.ToArray());
    }

    private void Construct(ref HexHalfPlane[] HalfPlanes, ref Dictionary<HexAxis, HexQuadrangleType> Type, HexLine[] lines)
    {
        if (lines.Length != 4)
        {
            throw new System.ArgumentException("Hex quadrangles must be described with four hex lines");
        }

        if (Array.FindAll(lines, x => x.Axis == HexAxis.None).Length != 0)
        {
            throw new System.ArgumentException("Hex quadrangles must be described by valid hex lines");
        }

        var xLines = Array.FindAll(lines, x => x.Axis == HexAxis.X);
        var yLines = Array.FindAll(lines, x => x.Axis == HexAxis.Y);
        var zLines = Array.FindAll(lines, x => x.Axis == HexAxis.Z);

        if (xLines.Length > 3 || yLines.Length > 3 || zLines.Length > 3)
        {
            throw new System.ArgumentException("Hex quadrangles can not be described with three or more hex lines in the same axis");
        }

        if ((xLines.Length == 2 && xLines[0].K == xLines[1].K) ||
            (yLines.Length == 2 && yLines[0].K == yLines[1].K) ||
            (zLines.Length == 2 && zLines[0].K == zLines[1].K))
        {
            throw new System.ArgumentException("Hex quadrangles can not be described without distinct hex lines in any given axis");
        }

        Type = new Dictionary<HexAxis, HexQuadrangleType>()
        {
            { HexAxis.X, HexQuadrangleType.None },
            { HexAxis.Y, HexQuadrangleType.None },
            { HexAxis.Z, HexQuadrangleType.None },
            { HexAxis.None, HexQuadrangleType.None },
        };

        var halfPlanes = new List<HexHalfPlane>();
        var baseLines = new List<HexLine>();
        var otherLines = new List<HexLine>();

        if (xLines.Length == 2)
        {
            baseLines.AddRange(xLines);
            otherLines.AddRange(yLines);
            otherLines.AddRange(zLines);
        }
        else if (yLines.Length == 2)
        {
            baseLines.AddRange(yLines);
            otherLines.AddRange(xLines);
            otherLines.AddRange(zLines);
        }
        else if (zLines.Length == 2)
        {
            baseLines.AddRange(zLines);
            otherLines.AddRange(xLines);
            otherLines.AddRange(yLines);
        }
        else
        {
            throw new System.ArgumentException("Hex quadrangle lines must have at least two in the same axis");
        }

        halfPlanes.Add(new HexHalfPlane(
            baseLines[0].Axis,
            HexHalfPlaneType.High,
            Mathf.Min(baseLines[0].K,baseLines[1].K)));
        halfPlanes.Add(new HexHalfPlane(
            baseLines[1].Axis,
            HexHalfPlaneType.Low,
            Mathf.Max(baseLines[0].K, baseLines[1].K)));

        var intersection = HexCubeCoordinates.Origin;
        if (!otherLines[0].Intersects(otherLines[1], out intersection))
        {
            halfPlanes.Add(new HexHalfPlane(
                otherLines[0].Axis,
                HexHalfPlaneType.High,
                Mathf.Min(otherLines[0].K, otherLines[1].K)));
            halfPlanes.Add(new HexHalfPlane(
                otherLines[0].Axis,
                HexHalfPlaneType.Low,
                Mathf.Max(otherLines[0].K, otherLines[1].K)));

            if ((baseLines[0].Axis == HexAxis.Z && otherLines[0].Axis == HexAxis.X) ||
                (baseLines[0].Axis == HexAxis.X && otherLines[0].Axis == HexAxis.Z))
            {
                Type[HexAxis.X] = HexQuadrangleType.Backward;
                Type[HexAxis.Y] = HexQuadrangleType.Rhomboid;
                Type[HexAxis.Z] = HexQuadrangleType.Forward;
            }
            else if ((baseLines[0].Axis == HexAxis.Z && otherLines[0].Axis == HexAxis.Y) ||
                     (baseLines[0].Axis == HexAxis.Y && otherLines[0].Axis == HexAxis.Z))
            {
                Type[HexAxis.X] = HexQuadrangleType.Forward;
                Type[HexAxis.Y] = HexQuadrangleType.Rhomboid;
                Type[HexAxis.Z] = HexQuadrangleType.Backward;
            }
            else if ((baseLines[0].Axis == HexAxis.X && otherLines[0].Axis == HexAxis.Y) ||
                     (baseLines[0].Axis == HexAxis.Y && otherLines[0].Axis == HexAxis.X))
            {
                Type[HexAxis.X] = HexQuadrangleType.Backward;
                Type[HexAxis.Y] = HexQuadrangleType.Forward;
                Type[HexAxis.Z] = HexQuadrangleType.Rhomboid;
            }
        }
        else
        {
            var k = baseLines[0].Axis == HexAxis.X
                ? intersection.X
                : baseLines[1].Axis == HexAxis.Y
                    ? intersection.Y
                    : intersection.Z;

            if (Mathf.Max(baseLines[0].K, baseLines[1].K) == k ||
                Mathf.Min(baseLines[0].K, baseLines[1].K) == k)
            {
                throw new System.ArgumentException($"Hex quadrangle parallel lines must be at least two units lower/higher than the intersection of the other lines ({zLines[0].K}, {zLines[1].K}) => {intersection.Z}");
            }

            bool highIntersection = false;

            if (Mathf.Max(k, baseLines[0].K, baseLines[1].K) == k)
            {
                halfPlanes.Add(new HexHalfPlane(
                    otherLines[0].Axis,
                    HexHalfPlaneType.High,
                    otherLines[0].K));
                halfPlanes.Add(new HexHalfPlane(
                    otherLines[1].Axis,
                    HexHalfPlaneType.High,
                    otherLines[1].K));

                highIntersection = true;
            }
            else if (Mathf.Min(k, baseLines[0].K, baseLines[1].K) == k)
            {
                halfPlanes.Add(new HexHalfPlane(
                    otherLines[0].Axis,
                    HexHalfPlaneType.Low,
                    otherLines[0].K));
                halfPlanes.Add(new HexHalfPlane(
                    otherLines[1].Axis,
                    HexHalfPlaneType.Low,
                    otherLines[1].K));
            }
            else
            {
                throw new System.ArgumentException("Hex quadrangles cannot have intersected lines in between two parallel lines");
            }

            if (highIntersection)
            {
                if (baseLines[0].Axis == HexAxis.Z)
                {
                    Type[HexAxis.X] = HexQuadrangleType.DownLeft;
                    Type[HexAxis.Y] = HexQuadrangleType.DownRight;
                    Type[HexAxis.Z] = HexQuadrangleType.Up;
                }
                else if (baseLines[0].Axis == HexAxis.X)
                {
                    Type[HexAxis.X] = HexQuadrangleType.Up;
                    Type[HexAxis.Y] = HexQuadrangleType.DownLeft;
                    Type[HexAxis.Z] = HexQuadrangleType.DownRight;
                }
                else if (baseLines[0].Axis == HexAxis.Y)
                {
                    Type[HexAxis.X] = HexQuadrangleType.DownRight;
                    Type[HexAxis.Y] = HexQuadrangleType.Up;
                    Type[HexAxis.Z] = HexQuadrangleType.DownLeft;
                }
            }
            else
            {
                if (baseLines[0].Axis == HexAxis.Z)
                {
                    Type[HexAxis.X] = HexQuadrangleType.UpRight;
                    Type[HexAxis.Y] = HexQuadrangleType.UpLeft;
                    Type[HexAxis.Z] = HexQuadrangleType.Down;
                }
                else if (baseLines[0].Axis == HexAxis.X)
                {
                    Type[HexAxis.X] = HexQuadrangleType.Down;
                    Type[HexAxis.Y] = HexQuadrangleType.UpRight;
                    Type[HexAxis.Z] = HexQuadrangleType.UpLeft;
                }
                else if (baseLines[0].Axis == HexAxis.Y)
                {
                    Type[HexAxis.X] = HexQuadrangleType.UpLeft;
                    Type[HexAxis.Y] = HexQuadrangleType.Down;
                    Type[HexAxis.Z] = HexQuadrangleType.UpRight;
                }
            }
        }

        if (halfPlanes.Count != 4)
        {
            throw new System.ArgumentException("Something went wrong creating half planes describing an hexagonal quadrangle");
        }

        HalfPlanes = halfPlanes.ToArray();
    }

    public bool Contains(HexCubeCoordinates coordinates)
    {
        foreach (var halfPlane in HalfPlanes)
        {
            if (!halfPlane.Contains(coordinates))
            {
                return false;
            }
        }

        return true;
    }

    private HexConvexPolygon _poly;

    /// <summary>
    /// HexConvexPolygon representation for an HexQuadrangle.
    /// </summary>
    public HexConvexPolygon ConvexPolygon
    {
        get
        {
            if (_poly == null)
            {
                _poly = new HexConvexPolygon(HalfPlanes);
            }

            return _poly;
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates with all those
    /// that are contained within this hex geometry figure.
    /// </summary>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    public IEnumerable<HexCubeCoordinates> Range(Predicate<HexCubeCoordinates> exists = null)
    {
        return ConvexPolygon.Range(exists);
    }

    private int _area = 0;

    /// <summary>
    /// Returns the count of hex cube coordinates contained within this hex geometry figure.
    /// </summary>
    public int Area()
    {
        if (_area == 0)
        {
            var vertices = new HexCubeCoordinates[4];
            HalfPlanes[0].Intersects(HalfPlanes[2], out vertices[0]);
            HalfPlanes[0].Intersects(HalfPlanes[3], out vertices[0]);
            HalfPlanes[1].Intersects(HalfPlanes[2], out vertices[0]);
            HalfPlanes[1].Intersects(HalfPlanes[3], out vertices[0]);

            var a = vertices[0].Distance(vertices[1]) + 1;
            var b = vertices[2].Distance(vertices[3]) + 1;

            if (a == b)
            {
                _area = a * (Mathf.Abs(HalfPlanes[0].K - HalfPlanes[1].K) + 1);
            }
            else if (a > b)
            {
                _area = ((a * (a + 1)) - (b * (b - 1))) / 2;
            }
            else
            {
                _area = ((b * (b + 1)) - (a * (a - 1))) / 2;
            }
        }

        return _area;
    }

    /// <summary>
    /// Spawns an hex quadrangle based on a given hex cube coordinate, width, length, type and hex orientation.
    /// The width corresponds to the number of hex cube coordinates in the upper edge of the quadrangle.
    /// </summary>
    public static HexQuadrangle Spawn(
        HexCubeCoordinates a,
        int width,
        int height,
        HexQuadrangleType type,
        HexOrientation orientation,
        HexAxis axis,
        bool spawnUpwards = true
        )
    {
        if (width < 2 || height < 2)
        {
            throw new System.ArgumentException($"Invalid combination of width ${width} and height ${height} to create an hex quadrangle.");
        }

        // Precompute vertices as if it were a spawn upwards up quadrangle.
        // This so that we can know ahead of time the smaller hex distance and bigger hex distance
        int smallerHexDistance = width;
        var topRightCorner = a.GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis);
        var lowerLeftCorner = a.GetVertex(height, HexVertexType.LowerLeft, orientation, axis);
        var lowerRightCorner = topRightCorner.GetVertex(height, HexVertexType.LowerRight, orientation, axis);
        var biggerHexDistance = lowerLeftCorner.Distance(lowerRightCorner) + 1;

        var vertices = new List<HexCubeCoordinates>();
        vertices.Add(a);

        switch (type)
        {
        case HexQuadrangleType.Up:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(biggerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperRight, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
            }
            break;

        case HexQuadrangleType.Down:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(biggerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
            }
            break;

        case HexQuadrangleType.UpLeft:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(biggerHexDistance, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.Left, orientation, axis));
                vertices.Add(vertices[2].GetVertex(smallerHexDistance, HexVertexType.LowerLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(biggerHexDistance, HexVertexType.LowerLeft, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
            }
            break;

        case HexQuadrangleType.UpRight:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(smallerHexDistance, HexVertexType.UpperLeft, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.Left, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(smallerHexDistance, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
            }
            break;

        case HexQuadrangleType.DownLeft:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(biggerHexDistance, HexVertexType.UpperLeft, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(biggerHexDistance, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.Left, orientation, axis));
                vertices.Add(vertices[2].GetVertex(smallerHexDistance, HexVertexType.UpperLeft, orientation, axis));
            }
            break;

        case HexQuadrangleType.DownRight:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(smallerHexDistance, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(smallerHexDistance, HexVertexType.LowerLeft, orientation, axis));
                vertices.Add(vertices[2].GetVertex(height, HexVertexType.Left, orientation, axis));
            }
            break;

        case HexQuadrangleType.Forward:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperRight, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerLeft, orientation, axis));
            }
            break;

        case HexQuadrangleType.Backward:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(smallerHexDistance, HexVertexType.Right, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
            }
            break;

        case HexQuadrangleType.Rhomboid:
            if (spawnUpwards)
            {
                vertices.Add(vertices[0].GetVertex(width, HexVertexType.UpperRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.UpperLeft, orientation, axis));
            }
            else
            {
                vertices.Add(vertices[0].GetVertex(height, HexVertexType.LowerRight, orientation, axis));
                vertices.Add(vertices[1].GetVertex(width, HexVertexType.LowerLeft, orientation, axis));
                vertices.Add(vertices[0].GetVertex(width, HexVertexType.LowerLeft, orientation, axis));
            }
            break;
        }

        return new HexQuadrangle(vertices.ToArray());
    }
}

/// <summary>
/// Describes an hexagonal grid regular hexagon.
/// Defined by a center hex cube coordinate and a radius.
/// </summary>
public struct HexRegularHexagon
{
    public readonly HexCubeCoordinates Center;
    public readonly int Radius;

    public HexRegularHexagon(HexCubeCoordinates center, int radius)
    {
        Center = center;
        Radius = Mathf.Abs(radius);
        _poly = null;
    }

    public bool Contains(HexCubeCoordinates coordinates)
        => Center.Distance(coordinates) <= Radius;

    private HexConvexPolygon _poly;

    /// <summary>
    /// HexConvexPolygon representation for an HexRegularHexagon.
    /// </summary>
    public HexConvexPolygon ConvexPolygon
    {
        get
        {
            if (_poly == null)
            {
                _poly = new HexConvexPolygon(
                    new HexHalfPlane[] {
                        new HexHalfPlane(HexAxis.X, HexHalfPlaneType.High, Center.X -Radius),
                        new HexHalfPlane(HexAxis.X, HexHalfPlaneType.Low, Center.X + Radius),
                        new HexHalfPlane(HexAxis.Y, HexHalfPlaneType.High, Center.Y -Radius),
                        new HexHalfPlane(HexAxis.Y, HexHalfPlaneType.Low, Center.Y + Radius),
                        new HexHalfPlane(HexAxis.Z, HexHalfPlaneType.High, Center.Z -Radius),
                        new HexHalfPlane(HexAxis.Z, HexHalfPlaneType.Low, Center.Z + Radius),
                    }
                );
            }

            return _poly;
        }
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates with all those
    /// that are contained within this hex geometry figure.
    /// </summary>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    public IEnumerable<HexCubeCoordinates> Range(Predicate<HexCubeCoordinates> exists = null)
    {
        return ConvexPolygon.Range(exists);
    }

    /// <summary>
    /// Returns the count of hex cube coordinates contained within this hex geometry figure.
    /// </summary>
    public int Area()
    {
        return (3 * Radius * Radius) + (3 * Radius) + 1;
    }
}

/// <summary>
/// Describes an hexagonal grid convex polygon.
/// Defined by multiple hex half planes.
/// </summary>
public class HexConvexPolygon
{
    public readonly HexHalfPlane[] HalfPlanes;
    public readonly HexCubeCoordinates[] Vertices;

    public struct MaxMinCoords
    {
        public readonly int Max_X;
        public readonly int Min_X;

        public readonly int Max_Y;
        public readonly int Min_Y;

        public readonly int Max_Z;
        public readonly int Min_Z;

        public MaxMinCoords(int maX, int miX, int maY, int miY, int maZ, int miZ)
        {
            Max_X = maX;
            Min_X = miX;
            Max_Y = maY;
            Min_Y = miY;
            Max_Z = maZ;
            Min_Z = miZ;
        }
    }

    public readonly MaxMinCoords RangeCoords;

    private HexConvexPolygon() { }

    public HexConvexPolygon(int highX, int lowX, int highY, int lowY, int highZ, int lowZ)
    {
        var halfPlanes = new List<HexHalfPlane>();
        halfPlanes.Add(new HexHalfPlane(HexAxis.X, HexHalfPlaneType.Low, highX));
        halfPlanes.Add(new HexHalfPlane(HexAxis.X, HexHalfPlaneType.High, lowX));
        halfPlanes.Add(new HexHalfPlane(HexAxis.Y, HexHalfPlaneType.Low, highY));
        halfPlanes.Add(new HexHalfPlane(HexAxis.Y, HexHalfPlaneType.High, lowY));
        halfPlanes.Add(new HexHalfPlane(HexAxis.Z, HexHalfPlaneType.Low, highZ));
        halfPlanes.Add(new HexHalfPlane(HexAxis.Z, HexHalfPlaneType.High, lowZ));
        HalfPlanes = halfPlanes.ToArray();
        ValidateHalfPlanes(ref RangeCoords, ref Vertices);
    }

    public HexConvexPolygon(HexHalfPlane[] halfPlanes)
    {
        HalfPlanes = halfPlanes;
        ValidateHalfPlanes(ref RangeCoords, ref Vertices);
    }

    private void ValidateHalfPlanes(ref MaxMinCoords rangeCoords, ref HexCubeCoordinates[] Vertices)
    {
        if (Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.X && x.Type == HexHalfPlaneType.High).Length > 1 ||
            Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.X && x.Type == HexHalfPlaneType.Low).Length > 1 ||
            Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.Y && x.Type == HexHalfPlaneType.High).Length > 1 ||
            Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.Y && x.Type == HexHalfPlaneType.Low).Length > 1 ||
            Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.Z && x.Type == HexHalfPlaneType.High).Length > 1 ||
            Array.FindAll(HalfPlanes,
                x => x.Axis == HexAxis.Z && x.Type == HexHalfPlaneType.Low).Length > 1)
        {
            throw new System.ArgumentException("Hex convex polygons cannot have multiple half planes with the same axis and type");
        }

        var intersections = new List<HexCubeCoordinates>();

        for (int i = 0; i < HalfPlanes.Length; ++i)
        {
            for (int j = i + 1; j < HalfPlanes.Length; ++j)
            {
                var intersection = HexCubeCoordinates.Zero;

                if (HalfPlanes[i].Intersects(HalfPlanes[j], out intersection) &&
                    this.Contains(intersection))
                {
                    intersections.Add(intersection);
                }
            }
        }

        if (intersections.Count < HalfPlanes.Length)
        {
            throw new System.ArgumentException("Hex convex polygons must have at least the same amount of intersections as half planes describing it");
        }

        Vertices = intersections.ToArray();
        PrecomputeMaxMinCoords(ref rangeCoords);
    }

    private void PrecomputeMaxMinCoords(ref MaxMinCoords rangeCoords)
    {
        var xs = Vertices.Select(n => n.X);
        var ys = Vertices.Select(n => n.Y);
        var zs = Vertices.Select(n => n.Z);

        rangeCoords = new MaxMinCoords(
            (int)Mathf.Max(xs.ToArray()),
            (int)Mathf.Min(xs.ToArray()),
            (int)Mathf.Max(ys.ToArray()),
            (int)Mathf.Min(ys.ToArray()),
            (int)Mathf.Max(zs.ToArray()),
            (int)Mathf.Min(zs.ToArray())
        );

        if ((rangeCoords.Max_X == rangeCoords.Min_X) ||
            (rangeCoords.Max_Y == rangeCoords.Min_Y) ||
            (rangeCoords.Max_Z == rangeCoords.Min_Z))
        {
            throw new System.ArgumentException("No max and min axis values can be the same when constructing an hex convex polygon");
        }
    }

    public bool Contains(HexCubeCoordinates coordinates)
    {
        foreach (var halfPlane in HalfPlanes)
        {
            if (!halfPlane.Contains(coordinates))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates with all those
    /// that are contained within this hex geometry figure.
    /// </summary>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    public IEnumerable<HexCubeCoordinates> Range(Predicate<HexCubeCoordinates> exists = null)
    {
        for (int x = RangeCoords.Min_X; x <= RangeCoords.Max_X; ++x)
        {
            var max = Mathf.Max(RangeCoords.Min_Y, -x - RangeCoords.Max_Z);
            var min = Mathf.Min(RangeCoords.Max_Y, -x - RangeCoords.Min_Z);
            for (int y = max; y <= min; ++y)
            {
                var coord = HexCubeCoordinates.FromXY(x, y);
                if (exists == null || exists.Invoke(coord))
                {
                    yield return coord;
                }
            }
        }
    }

    private int _area = 0;

    /// <summary>
    /// Returns the count of hex cube coordinates contained within this hex geometry figure.
    /// </summary>
    public int Area()
    {
        if (_area == 0)
        {
            _area = Range().Count();
        }

        return _area;
    }

    /// <summary>
    /// Returns the intersection of hex cube coordinates between two hex convex polygons.
    /// </summary>
    public IEnumerable<HexCubeCoordinates> Intersection(HexConvexPolygon polygon, Predicate<HexCubeCoordinates> exists = null)
    {
        return Intersection(new HexConvexPolygon[] { this, polygon }, exists);
    }

    /// <summary>
    /// Returns the intersection of hex cube coordinates between multiple hex convex polygons.
    /// </summary>
    public static IEnumerable<HexCubeCoordinates> Intersection(HexConvexPolygon[] polygons, Predicate<HexCubeCoordinates> exists = null)
    {
        try
        {
            return new HexConvexPolygon(
                (int)Mathf.Min(polygons.Select(p => (float)p.RangeCoords.Max_X).ToArray()),
                (int)Mathf.Max(polygons.Select(p => (float)p.RangeCoords.Min_X).ToArray()),
                (int)Mathf.Min(polygons.Select(p => (float)p.RangeCoords.Max_Y).ToArray()),
                (int)Mathf.Max(polygons.Select(p => (float)p.RangeCoords.Min_Y).ToArray()),
                (int)Mathf.Min(polygons.Select(p => (float)p.RangeCoords.Max_Z).ToArray()),
                (int)Mathf.Max(polygons.Select(p => (float)p.RangeCoords.Min_Z).ToArray())
            ).Range(exists);
        }
        catch
        {
            // Ignore failure to create polygon and treat it as if there was no intersection
            return new List<HexCubeCoordinates>();
        }

    }
}

public static class HexConvexPolygonExtensions
{
    /// <summary>
    /// Returns the intersection of hex cube coordinates between multiple hex convex polygons.
    /// </summary>
    public static IEnumerable<HexCubeCoordinates> Intersection(this HexConvexPolygon poly, HexConvexPolygon[] polygons, Predicate<HexCubeCoordinates> exists = null)
    {
        var list = new List<HexConvexPolygon>();
        list.Add(poly);
        list.AddRange(polygons);
        return HexConvexPolygon.Intersection(list.ToArray(), exists);
    }

}

/// <summary>
/// Describes an hexagonal grid arbitrary polygon.
/// Defined by multiple non-intersecting hex convex polygons.
/// </summary>
public class HexPolygon
{
    public readonly HexConvexPolygon[] ConvexPolygons;

    private HexPolygon() { }

    public HexPolygon(HexConvexPolygon[] polygons)
    {
        if (HexConvexPolygon.Intersection(polygons).Count() != 0)
        {
            throw new System.ArgumentException("Hex polygons created with convex polygons must not intersect");
        }

        ConvexPolygons = polygons;
    }

    public bool Contains(HexCubeCoordinates coordinates)
    {
        foreach (var polygon in ConvexPolygons)
        {
            if (polygon.Contains(coordinates))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns an IEnumerable of hex cube coordinates with all those
    /// that are contained within this hex geometry figure.
    /// </summary>
    /// <param name="exists">Predicate that determines if an hex cube coordinate exists, if it does then it's included in the results.</param>
    public IEnumerable<HexCubeCoordinates> Range(Predicate<HexCubeCoordinates> exists = null)
    {
        foreach (var polygon in ConvexPolygons)
        {
            foreach (var coordinate in polygon.Range(exists))
            {
                yield return coordinate;
            }
        }
    }

    private int _area = 0;

    /// <summary>
    /// Returns the count of hex cube coordinates contained within this hex geometry figure.
    /// </summary>
    public int Area()
    {
        if (_area == 0)
        {
            foreach (var polygon in ConvexPolygons)
            {
                _area += polygon.Area();
            }
        }

        return _area;
    }
}

}
