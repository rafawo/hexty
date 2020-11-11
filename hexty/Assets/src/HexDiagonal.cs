using UnityEngine;

/// <summary>
/// Represents the diagonal direction from the center of a pointy hex tile.
/// </summary>
public enum HexPointyDiagonal
{
    North,
    NorthEast,
    SouthEast,
    South,
    SouthWest,
    NorthWest,
}

/// <summary>
/// Represents the diagonal direction from the center of a flat hex tile.
/// </summary>
public enum HexFlatDiagonal
{
    NorthEast,
    East,
    SouthEast,
    SouthWest,
    West,
    NorthWest,
}

/// <summary>
/// Abstraction for an hexagonal diagonal.
/// The diagonal is determined by the orientation and the int diagonal
/// value modulo 6.
/// </summary>
public struct HexDiagonal
{
    public readonly HexOrientation Orientation;
    public readonly int Diagonal;

    public HexDiagonal(HexOrientation orientation, int diagonal)
    {
        Orientation = orientation;
        Diagonal = diagonal;
    }

    public HexPointyDiagonal AsHexPointy()
    {
        return (HexPointyDiagonal)(Diagonal % 6);
    }

    public HexFlatDiagonal AsHexFlat()
    {
        return (HexFlatDiagonal)(Diagonal % 6);
    }
}

public static class HexPointyDiagonalUtilities
{
    private static string[] _diagonalNames = new string[]
    {
        "North",
        "NorthEast",
        "SouthEast",
        "South",
        "SouthWest",
        "NorthWest",
    };

    public static string ToString(this HexPointyDiagonal diagonal)
        => _diagonalNames[(int)diagonal];

    private static HexPointyDiagonal[] _values = new HexPointyDiagonal[]
    {
        HexPointyDiagonal.North,
        HexPointyDiagonal.NorthEast,
        HexPointyDiagonal.SouthEast,
        HexPointyDiagonal.South,
        HexPointyDiagonal.SouthWest,
        HexPointyDiagonal.NorthWest,
    };

    public static HexPointyDiagonal[] Values()
        => _values;

    public static HexDiagonal AsDiagonal(this HexPointyDiagonal diagonal)
        => new HexDiagonal(HexOrientation.Pointy, (int)diagonal);

    public static Vector3 TranslationVector(this HexPointyDiagonal diagonal)
        => diagonal.AsDiagonal().TranslationVector();
}

public static class HexFlatDiagonalUtilities
{
    private static string[] _diagonalNames = new string[]
    {
        "NorthEast",
        "East",
        "SouthEast",
        "SouthWest",
        "West",
        "NorthWest",
    };

    public static string ToString(this HexFlatDiagonal diagonal)
        => _diagonalNames[(int)diagonal];

    private static HexFlatDiagonal[] _values = new HexFlatDiagonal[]
    {
        HexFlatDiagonal.NorthEast,
        HexFlatDiagonal.East,
        HexFlatDiagonal.SouthEast,
        HexFlatDiagonal.SouthWest,
        HexFlatDiagonal.West,
        HexFlatDiagonal.NorthWest,
    };

    public static HexFlatDiagonal[] Values()
        => _values;

    public static HexDiagonal AsDiagonal(this HexFlatDiagonal diagonal)
        => new HexDiagonal(HexOrientation.Flat, (int)diagonal);

    public static Vector3 TranslationVector(this HexFlatDiagonal diagonal)
        => diagonal.AsDiagonal().TranslationVector();
}

public static class HexDiagonalUtilities
{
    public static string ToString(this HexDiagonal diagonal)
        => diagonal.Orientation == HexOrientation.Pointy
            ? diagonal.AsHexPointy().ToString()
            : diagonal.AsHexFlat().ToString();

    private static Vector3[] _translationVectors = new Vector3[]
    {
        new Vector3(-1, -1,  2), // Flat-NorthEast  -  Pointy-North
        new Vector3( 1, -2,  1), // Flat-East  -  Pointy-NorthEast
        new Vector3( 2, -1, -1), // Flat-SouthEast  -  Pointy-SouthEast
        new Vector3( 1,  1, -2), // Flat-SouthWest  -  Pointy-South
        new Vector3(-1,  2, -1), // Flat-West  -  Pointy-SouthWest
        new Vector3(-2,  1,  1), // Flat-NorthWest  -  Pointy-NorthWest
    };

    public static Vector3 TranslationVector(this HexDiagonal diagonal)
        => _translationVectors[diagonal.Diagonal];

    public static Vector3 TranslationVector(int diagonal)
        => _translationVectors[diagonal % 6];
}
