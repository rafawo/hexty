/// <summary>
/// All possible sections from within an hex cell.
/// </summary>
public enum HexSection
{
    Up,
    Right,
    Down,
    Left,
}

public static class HexSectionUtilities
{
    private readonly static HexSection[] _values = new HexSection[]
    {
        HexSection.Up,
        HexSection.Right,
        HexSection.Down,
        HexSection.Left,
    };

    public static HexSection[] Values()
        => _values;

    private readonly static HexSection[][][] _sectionsByDirection = new HexSection[][][]
    {
        new HexSection[][]
        {
            new HexSection[]
            { // NorthEast
                HexSection.Up,
                HexSection.Right,
            },
            new HexSection[]
            { // East
                HexSection.Right,
            },
            new HexSection[]
            { // SouthEast
                HexSection.Right,
                HexSection.Down,
            },
            new HexSection[]
            { // SouthWest
                HexSection.Down,
                HexSection.Left,
            },
            new HexSection[]
            { // West
                HexSection.Left,
            },
            new HexSection[]
            { // NorthWest
                HexSection.Left,
                HexSection.Up,
            },
        },
        new HexSection[][]
        {
            new HexSection[]
            { // NorthEast
                HexSection.Up,
                HexSection.Right,
            },
            new HexSection[]
            { // SouthEast
                HexSection.Right,
                HexSection.Down,
            },
            new HexSection[]
            { // South
                HexSection.Down,
            },
            new HexSection[]
            { // SouthWest
                HexSection.Left,
                HexSection.Down,
            },
            new HexSection[]
            { // NorthWest
                HexSection.Left,
                HexSection.Up,
            },
            new HexSection[]
            { // North
                HexSection.Up,
            },
        },
    };

    public static HexSection[] GetDirectionSections(HexPointyDirection direction)
        => _sectionsByDirection[(int)HexOrientation.Pointy][(int)direction];

    public static HexSection[] GetDirectionSections(HexFlatDirection direction)
        => _sectionsByDirection[(int)HexOrientation.Flat][(int)direction];

    public static HexSection[] GetDirectionSections(HexDirection direction)
        => _sectionsByDirection[(int)direction.Orientation][direction.Direction];

    private readonly static string[] _sectionNames = new string[]
    {
        "Up",
        "Right",
        "Down",
        "Left",
    };

    public static string ToString(this HexSection section)
        => _sectionNames[(int)section];

    private readonly static HexPointyDirection[][] _pointyDirectionsBySection = new HexPointyDirection[][]
    {
        new HexPointyDirection[]
        { // Up
            HexPointyDirection.NorthEast,
            HexPointyDirection.NorthWest,
        },
        new HexPointyDirection[]
        { // Right
            HexPointyDirection.NorthEast,
            HexPointyDirection.East,
            HexPointyDirection.SouthEast,
        },
        new HexPointyDirection[]
        { // Down
            HexPointyDirection.SouthEast,
            HexPointyDirection.SouthWest,
        },
        new HexPointyDirection[]
        { // Left
            HexPointyDirection.NorthWest,
            HexPointyDirection.West,
            HexPointyDirection.SouthWest,
        },
    };

    private readonly static HexFlatDirection[][] _flatDirectionsBySection = new HexFlatDirection[][]
    {
        new HexFlatDirection[]
        { // Up
            HexFlatDirection.North,
        },
        new HexFlatDirection[]
        { // Right
            HexFlatDirection.NorthEast,
            HexFlatDirection.SouthEast,
        },
        new HexFlatDirection[]
        { // Down
            HexFlatDirection.South,
        },
        new HexFlatDirection[]
        { // Left
            HexFlatDirection.NorthWest,
            HexFlatDirection.SouthWest,
        },
    };

    public static HexPointyDirection[] PointyDirections(this HexSection section)
        => _pointyDirectionsBySection[(int)section];

    public static HexFlatDirection[] FlatDirections(this HexSection section)
        => _flatDirectionsBySection[(int)section];

    public static bool IsDirectionContained(this HexSection section, HexPointyDirection direction)
        => System.Array.Exists(section.PointyDirections(), x => x == direction);

    public static bool IsDirectionContained(this HexSection section, HexFlatDirection direction)
        => System.Array.Exists(section.FlatDirections(), x => x == direction);

    public static bool IsDirectionContained(this HexSection section, HexDirection direction)
        => direction.Orientation == HexOrientation.Pointy
                ? System.Array.Exists(section.PointyDirections(), x => x == direction.AsHexPointy())
                : System.Array.Exists(section.FlatDirections(), x => x == direction.AsHexFlat());
}
