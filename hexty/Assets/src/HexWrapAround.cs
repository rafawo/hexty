// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using UnityEngine;

/// <summary>
/// Hex grid wrap around class that describes the limits
/// for which the grid should be constrained to.
/// The wrap around logic is based on the offset coordinates
/// of the hex cube coordinates.
/// </summary>
public class HexWrapAround
{
    public readonly int Width;
    public readonly int Height;

    public readonly HexOffsetType OffsetType;
    public readonly HexOrientation Orientation;

    private readonly Vector3 _modOffset;

    private HexWrapAround() { }

    public HexWrapAround(int width, int height, HexOffsetType type, HexOrientation orientation)
    {
        if ((orientation == HexOrientation.Pointy) && ((height&1) == 1))
        {
            throw new System.ArgumentException("Pointy hex must have an even wrapping height!");
        }
        else if ((orientation == HexOrientation.Flat) && ((width&1) == 1))
        {
            throw new System.ArgumentException("Flat hex must have an even wrapping width!");
        }

        Width = width;
        Height = height;
        OffsetType = type;
        Orientation = orientation;
        _modOffset = new Vector3(Width, 0, Height);
    }

    public HexCubeCoordinates TransformHex(HexCubeCoordinates coordinates)
    {
        var offsetCoordinates = OffsetType.FromHexCube(Orientation, coordinates) + _modOffset;
        return OffsetType.ToHexCube(Orientation, (int)(offsetCoordinates.x % Width), (int)(offsetCoordinates.z % Height));
    }

    public Vector3 TransformPosition(Vector3 position, HexMetrics metrics)
    {
        var hex = HexCubeCoordinates.FromPosition(position, metrics, Orientation);
        var transformedHex = TransformHex(hex);

        if (hex != transformedHex)
        {
            var offset = position - hex.ToPosition(metrics, Orientation, OffsetType);
            return transformedHex.ToPosition(metrics, Orientation, OffsetType) + offset;
        }
        else
        {
            return position;
        }
    }
}