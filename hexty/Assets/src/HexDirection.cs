// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System;
using UnityEngine;

/// <summary>
/// All possible directions from a pointy hex tile that connect to neighbor cells.
/// </summary>
public enum HexPointyDirection
{
    NorthEast,
    East,
    SouthEast,
    SouthWest,
    West,
    NorthWest,
}

/// <summary>
/// All possible directions from a flat hex tile that connect to neighbor cells.
/// </summary>
public enum HexFlatDirection
{
    North,
    NorthEast,
    SouthEast,
    South,
    SouthWest,
    NorthWest,
}

/// <summary>
/// Abstraction for an hexagonal direction.
/// The direction is determined by the orientation and the int Direction
/// value modulo 6.
/// </summary>
public struct HexDirection
{
    public readonly HexOrientation Orientation;
    public readonly int Direction;

    public HexDirection(HexOrientation orientation, int direction)
    {
        Orientation = orientation;
        Direction = direction;
    }

    public HexPointyDirection AsHexPointy()
    {
        return (HexPointyDirection)(Direction % 6);
    }

    public HexFlatDirection AsHexFlat()
    {
        return (HexFlatDirection)(Direction % 6);
    }
}

public static class HexPointyDirectionUtilities
{
    public static HexDirection AsDirection(this HexPointyDirection direction)
        => new HexDirection(HexOrientation.Pointy, (int)direction);

    private readonly static HexPointyDirection[] _values = new HexPointyDirection[]
    {
        HexPointyDirection.NorthEast,
        HexPointyDirection.East,
        HexPointyDirection.SouthEast,
        HexPointyDirection.SouthWest,
        HexPointyDirection.West,
        HexPointyDirection.NorthWest,
    };

    public static HexPointyDirection[] Values()
        => _values;

    public readonly static string[] _directionNames = new string[]
    {
        "NorthEast",
        "East",
        "SouthEast",
        "SouthWest",
        "West",
        "NorthWest",
    };

    public static string ToString(this HexPointyDirection direction)
        => _directionNames[(int)direction];

    public static HexPointyDirection Opposite(this HexPointyDirection direction)
        => (int)direction < 3 ? (direction + 3) : (direction - 3);

    public static HexPointyDirection Previous(this HexPointyDirection direction)
        => direction == HexPointyDirection.NorthEast ? HexPointyDirection.NorthWest : (direction - 1);

    public static HexPointyDirection Next(this HexPointyDirection direction)
        => direction == HexPointyDirection.NorthWest ? HexPointyDirection.NorthEast : (direction + 1);

    public static Vector3 UnitVector(this HexPointyDirection direction)
        => new HexDirection(HexOrientation.Pointy, (int)direction).UnitVector();

    private readonly static Predicate<float>[] _pointAsAnglePredicates = new Predicate<float>[]
    {
        x => x > 30 && x <= 90, // NorthEast
        x => x >= -30 && x <= 30, // East
        x => x < -30 && x > -90, // SouthEast
        x => x <= -90 && x > -150, // SouthWest
        x => (x > 150 && x <= 180) || (x <= -150 && x >= -180), // West
        x => x > 90 && x <= 150, // NorthWest
    };

    public static bool IsPointContained(this HexPointyDirection direction, Vector3 origin, Vector3 point)
        => _pointAsAnglePredicates[(int)direction]
            .Invoke(Mathf.Atan2(point.z - origin.z, point.x - origin.x) * Mathf.Rad2Deg);

    public static HexPointyDirection GetPointDirection(Vector3 origin, Vector3 point)
    {
        var angle = Mathf.Atan2(point.z - origin.z, point.x - origin.x) * Mathf.Rad2Deg;
        foreach (var direction in Values())
        {
            if (_pointAsAnglePredicates[(int)direction].Invoke(angle))
            {
                return direction;
            }
        }
        throw new System.InvalidOperationException($"Failed to get triangle for origin {origin} and point {point}");
    }

    public static HexSection[] Sections(this HexPointyDirection direction)
        => HexSectionUtilities.GetDirectionSections(direction);
}

public static class HexFlatDirectionUtilities
{
    public static HexDirection AsDirection(this HexFlatDirection direction)
        => new HexDirection(HexOrientation.Flat, (int)direction);

    private readonly static HexFlatDirection[] _values = new HexFlatDirection[]
    {
        HexFlatDirection.NorthEast,
        HexFlatDirection.SouthEast,
        HexFlatDirection.South,
        HexFlatDirection.SouthWest,
        HexFlatDirection.NorthWest,
        HexFlatDirection.North,
    };

    public static HexFlatDirection[] Values()
        => _values;

    public readonly static string[] _directionNames = new string[]
    {
        "NorthEast",
        "SouthEast",
        "South",
        "SouthWest",
        "NorthWest",
        "North",
    };

    public static string ToString(this HexFlatDirection direction)
        => _directionNames[(int)direction];

    public static HexFlatDirection Opposite(this HexFlatDirection direction)
        => (int)direction < 3 ? (direction + 3) : (direction - 3);

    public static HexFlatDirection Previous(this HexFlatDirection direction)
        => direction == HexFlatDirection.North ? HexFlatDirection.NorthWest : (direction - 1);

    public static HexFlatDirection Next(this HexFlatDirection direction)
        => direction == HexFlatDirection.NorthWest ? HexFlatDirection.North : (direction + 1);

    public static Vector3 UnitVector(this HexFlatDirection direction)
        => new HexDirection(HexOrientation.Flat, (int)direction).UnitVector();

    private readonly static Predicate<float>[] _pointAsAnglePredicates = new Predicate<float>[]
    {
        x => x >= 0 && x <= 60, // NorthEast
        x => x <= 0 && x > -60, // SouthEast
        x => x <= -60 && x > -120, // South
        x => x < -120 && x >= -180, // SouthWest
        x => x > 120 && x <= 180, // NorthWest
        x => x > 60 && x <= 120, // North
    };

    public static bool IsPointContained(this HexFlatDirection direction, Vector3 origin, Vector3 point)
        => _pointAsAnglePredicates[(int)direction]
            .Invoke(Mathf.Atan2(point.z - origin.z, point.x - origin.x) * Mathf.Rad2Deg);

    public static HexFlatDirection GetPointDirection(Vector3 origin, Vector3 point)
    {
        var angle = Mathf.Atan2(point.z - origin.z, point.x - origin.x) * Mathf.Rad2Deg;
        foreach (var direction in Values())
        {
            if (_pointAsAnglePredicates[(int)direction].Invoke(angle))
            {
                return direction;
            }
        }
        throw new System.InvalidOperationException($"Failed to get triangle for origin {origin} and point {point}");
    }

    public static HexSection[] Sections(this HexFlatDirection direction)
        => HexSectionUtilities.GetDirectionSections(direction);
}

public static class HexDirectionUtilities
{
    public static HexDirection[] Values(HexOrientation orientation)
        => new HexDirection[]
        {
            new HexDirection(orientation, 0),
            new HexDirection(orientation, 1),
            new HexDirection(orientation, 2),
            new HexDirection(orientation, 3),
            new HexDirection(orientation, 4),
            new HexDirection(orientation, 5),
        };

    public static int[] Directions()
        => new int[] { 0, 1, 2, 3, 4, 5 };

    public static string ToString(this HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? direction.AsHexPointy().ToString()
            : direction.AsHexFlat().ToString();

    public static HexDirection Opposite(this HexDirection direction)
        => new HexDirection(direction.Orientation,
            direction.Orientation == HexOrientation.Pointy
                ? (int)direction.AsHexPointy().Opposite()
                : (int)direction.AsHexFlat().Opposite());

    public static HexDirection Previous(this HexDirection direction)
        => new HexDirection(direction.Orientation,
            direction.Orientation == HexOrientation.Pointy
                ? (int)direction.AsHexPointy().Previous()
                : (int)direction.AsHexFlat().Previous());

    public static HexDirection Next(this HexDirection direction)
        => new HexDirection(direction.Orientation,
            direction.Orientation == HexOrientation.Pointy
                ? (int)direction.AsHexPointy().Next()
                : (int)direction.AsHexFlat().Next());

    public readonly static Vector3[] _unitVectors = new Vector3[]
    {
        new Vector3( 0, -1,  1), // Pointy-NorthEast == Flat-North
        new Vector3( 1, -1,  0), // Pointy-East == Flat-NorthEast
        new Vector3( 1,  0, -1), // Pointy-SouthEast == Flat-SouthEast
        new Vector3( 0,  1, -1), // Pointy-SouthWest == Flat-South
        new Vector3(-1,  1,  0), // Pointy-West == Flat-SouthWest
        new Vector3( -1, 0,  1), // Pointy-NorthEast == Flat-NorthWest
    };

    public static Vector3 UnitVector(int direction)
        => _unitVectors[direction % 6];

    public static Vector3 UnitVector(this HexDirection direction)
        => UnitVector(direction.Direction);

    public static bool IsPointContained(this HexDirection direction, Vector3 origin, Vector3 point)
        => direction.Orientation == HexOrientation.Pointy
            ? direction.AsHexPointy().IsPointContained(origin, point)
            : direction.AsHexFlat().IsPointContained(origin, point);

    public static HexSection[] Sections(this HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
            ? direction.AsHexPointy().Sections()
            : direction.AsHexFlat().Sections();
}
