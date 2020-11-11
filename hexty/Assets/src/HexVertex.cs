/// <summary>
/// Types of vertex directions from within an hex cube coordinate
/// on a given axis.
/// </summary>
public enum HexVertexType
{
    Left,
    UpperLeft,
    LowerLeft,
    Right,
    UpperRight,
    LowerRight,
}

public static class HexVertexTypeUtilities
{
    /// <summary>
    /// Returns the hex cube coordinate that corresponds to the neighbor
    /// at length distance described by the supplied vertex type and on
    /// respect to a given axis.
    /// </summary>
    public static HexCubeCoordinates GetVertex(
        this HexCubeCoordinates a,
        int length,
        HexVertexType type,
        HexOrientation orientation,
        HexGeometry.HexAxis axis
        )
    {
        HexCubeCoordinates coordinates = null;

        switch (type)
        {
        case HexVertexType.Left:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthEast
                        : (int)HexFlatDirection.North,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthEast
                        : (int)HexFlatDirection.SouthEast,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.West
                        : (int)HexFlatDirection.SouthWest,
                    length - 1);
                break;

            }
            break;

        case HexVertexType.UpperLeft:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.East
                        : (int)HexFlatDirection.NorthEast,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthWest
                        : (int)HexFlatDirection.South,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthWest
                        : (int)HexFlatDirection.NorthWest,
                    length - 1);
                break;

            }
            break;

        case HexVertexType.LowerLeft:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthWest
                        : (int)HexFlatDirection.NorthWest,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.East
                        : (int)HexFlatDirection.NorthEast,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthWest
                        : (int)HexFlatDirection.South,
                    length - 1);
                break;

            }
            break;

        case HexVertexType.Right:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthWest
                        : (int)HexFlatDirection.South,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthWest
                        : (int)HexFlatDirection.NorthWest,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.East
                        : (int)HexFlatDirection.NorthEast,
                    length - 1);
                break;

            }
            break;

        case HexVertexType.UpperRight:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthEast
                        : (int)HexFlatDirection.SouthEast,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.West
                        : (int)HexFlatDirection.SouthWest,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthEast
                        : (int)HexFlatDirection.North,
                    length - 1);
                break;

            }
            break;

        case HexVertexType.LowerRight:
            switch (axis)
            {
            case HexGeometry.HexAxis.X:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.West
                        : (int)HexFlatDirection.SouthWest,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Y:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.NorthEast
                        : (int)HexFlatDirection.North,
                    length - 1);
                break;

            case HexGeometry.HexAxis.Z:
                coordinates = a.GetNeighbor(
                    orientation == HexOrientation.Pointy
                        ? (int)HexPointyDirection.SouthEast
                        : (int)HexFlatDirection.SouthEast,
                    length - 1);
                break;

            }
            break;
        }

        return coordinates;
    }
}