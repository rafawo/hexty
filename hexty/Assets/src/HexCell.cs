using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCubeCoordinates Coordinates;
    public HexCubeCoordinates PaddingCoordinates;

    public bool IsPadding
    {
        get
        {
            return Coordinates != PaddingCoordinates;
        }
    }

    public Color ViewColor;

    public int MeshColorIndex = 0;

    [SerializeField]
    private HexCell[] neighbors = new HexCell[6];

    [SerializeField]
    private HexCubeCoordinates neighbor0;

    [SerializeField]
    private HexCubeCoordinates neighbor1;

    [SerializeField]
    private HexCubeCoordinates neighbor2;

    [SerializeField]
    private HexCubeCoordinates neighbor3;

    [SerializeField]
    private HexCubeCoordinates neighbor4;

    [SerializeField]
    private HexCubeCoordinates neighbor5;

    public HexCell GetNeighbor(HexDirection direction)
        => neighbors[direction.Direction];

    public HexCell GetNeighbor(int direction)
        => neighbors[direction % 6];

    private HexCell ResolveNeighbor(int direction, Dictionary<HexCubeCoordinates, HexCell> hashGrid, HexWrapAround wrapAround = null)
    {
        HexCell neighbor = null;
        var hex = Coordinates.GetNeighbor(direction);

        if (wrapAround != null)
        {
            hex = wrapAround.TransformHex(hex);
        }

        if (!hashGrid.TryGetValue(hex, out neighbor))
        {
            neighbor = null;
        }

        return neighbor;
    }

    public void InitializeNeighbors(Dictionary<HexCubeCoordinates, HexCell> hashGrid, HexWrapAround wrapAround = null)
    {
        neighbors[0] = ResolveNeighbor(0, hashGrid, wrapAround);
        neighbor0 = neighbors[0] != null ? neighbors[0].Coordinates : null;

        neighbors[1] = ResolveNeighbor(1, hashGrid, wrapAround);
        neighbor1 = neighbors[1] != null ? neighbors[1].Coordinates : null;

        neighbors[2] = ResolveNeighbor(2, hashGrid, wrapAround);
        neighbor2 = neighbors[2] != null ? neighbors[2].Coordinates : null;

        neighbors[3] = ResolveNeighbor(3, hashGrid, wrapAround);
        neighbor3 = neighbors[3] != null ? neighbors[3].Coordinates : null;

        neighbors[4] = ResolveNeighbor(4, hashGrid, wrapAround);
        neighbor4 = neighbors[4] != null ? neighbors[4].Coordinates : null;

        neighbors[5] = ResolveNeighbor(5, hashGrid, wrapAround);
        neighbor5 = neighbors[5] != null ? neighbors[5].Coordinates : null;
    }

    public override int GetHashCode()
        => Coordinates.GetHashCode();

    public override bool Equals(object other)
        => other is HexCell cell && cell.Coordinates.Equals(this.Coordinates);

    public static implicit operator bool(HexCell cell)
        => !object.ReferenceEquals(cell, null);
}
