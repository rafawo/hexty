// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using UnityEngine;

/// <summary>
/// Simple struct that defines a texture dimensions (width, height).
/// </summary>
public struct UvTexDimensions
{
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public Vector2 vector { get { return new Vector2(Width, Height); } }

    public UvTexDimensions(uint width, uint height)
    {
        Width = width;
        Height = height;
    }

    public UvTexDimensions(Vector2 v)
    {
        Width = (uint)v.x;
        Height = (uint)v.y;
    }

    public static UvTexDimensions zero = new UvTexDimensions { Width = 0, Height = 0 };
    public static UvTexDimensions one = new UvTexDimensions { Width = 1, Height = 1 };

    public override string ToString() => string.Format("({0},{1})", Width, Height);
}

/// <summary>
/// Simple abstraction for a set of UV coordinates representing a square.
/// Individual vertex-uv values can be obtained through the fields named after the vertex location,
/// or Arr can be used to get them all together.
/// </summary>
public struct UvTexSquare
{
    public Vector2 LowerLeft;
    public Vector2 LowerRight;
    public Vector2 UpperLeft;
    public Vector2 UpperRight;

    /// <summary>
    /// Array of vector representing the UV coordinates of a square.
    /// [0] = LowerLeft
    /// [1] = LowerRight
    /// [2] = UpperLeft
    /// [3] = UpperRight
    /// </summary>
    /// <value></value>
    public Vector2[] Arr
    {
        get
        {
            return new Vector2[] { LowerLeft, LowerRight, UpperLeft, UpperRight };
        }
    }
}

/// <summary>
/// Class used to store necessary parameters to compute UV coordinates for an arbitrary texture.
/// Note that the fields describe an arbitrary square clip within a texture.
/// </summary>
public class UvTexClip
{
    private UvTexDimensions _origin = UvTexDimensions.zero;
    private UvTexDimensions _clip = UvTexDimensions.one;
    private UvTexDimensions _tex = UvTexDimensions.one;
    private UvTexDimensions _lowerBoundOffset = UvTexDimensions.zero;
    private UvTexDimensions _upperBoundOffset = UvTexDimensions.zero;

    /// <summary>
    /// Origin coordinates in pixels within the texture where the clip is described from.
    /// </summary>
    public UvTexDimensions Origin { get { return _origin; } set { UpdateField(ref _origin, value); } }

    /// <summary>
    /// Dimensions of the texture clip where a nested image is completely bounded.
    /// </summary>
    public UvTexDimensions ClipDimensions { get { return _clip; } set { UpdateField(ref _clip, value); } }

    /// <summary>
    /// Dimensions of the texture itself in pixels.
    /// </summary>
    public UvTexDimensions TextureDimensions { get { return _tex; } set { UpdateField(ref _tex, value); } }

    /// <summary>
    /// Optional lower bound offset within the clip square.
    /// Essentially increases the origin by the given offset when calculating UV coordinates.
    /// Defaults to (0, 0).
    /// </summary>
    public UvTexDimensions LowerBoundOffset { get { return _lowerBoundOffset; } set { UpdateField(ref _lowerBoundOffset, value); } }

    /// <summary>
    /// Optional upper bound offset within the clip square.
    /// Essentially decreases the clip dimensions by the given offset when calculating UV coordinates.
    /// Defaults to (0, 0).
    /// </summary>
    public UvTexDimensions UpperBoundOffset { get { return _upperBoundOffset; } set { UpdateField(ref _lowerBoundOffset, value); } }

    /// <summary>
    /// Shortcut to the origin coordinates plus the lower bound offset.
    /// </summary>
    public UvTexDimensions OffsettedOrigin { get { return new UvTexDimensions(Origin.vector + LowerBoundOffset.vector); } }

    /// <summary>
    /// Shortcut to the clip dimensions coordinates minus the upper bound offset.
    /// </summary>
    public UvTexDimensions OffsettedClipDimensions { get { return new UvTexDimensions(ClipDimensions.vector + UpperBoundOffset.vector); } }

    /// <summary>
    /// Constructs a default UvTexClip, which is origin at (0, 0),
    /// clip dimensions being (1, 1) and texture dimensions (1, 1).
    /// Both lower and upper bound offsets are set to (0, 0).
    /// This essentially describes an UV clip whose entire image is the texture itself.
    /// </summary>
    public UvTexClip()
    {
        Validate();
    }

    /// <summary>
    /// Shortcut property that describes the default values of an UvTexClip instance.
    /// </summary>
    public static UvTexClip Default = new UvTexClip();

    /// <summary>
    /// Constructor of UvTexClip that assign an origin, clip dimensions and texture dimensions.
    /// </summary>
    public UvTexClip(UvTexDimensions origin, UvTexDimensions clipDimensions, UvTexDimensions textureDimensions)
    {
        _origin = origin;
        _clip = clipDimensions;
        _tex = textureDimensions;
        Validate();
    }

    /// <summary>
    /// Computes the UV coordinate described by this instance of parameters at the given unitary UV.
    /// The supplied unitary UV coordinate essentially determines the equivalent UV
    /// coordinate that we are trying to compute as if the clip within the texture
    /// was the entire texture. As such, the unitary UV is expected to be constrained
    /// within the 0-1 range in both axis.
    /// </summary>
    /// <param name="unitaryUv">Relative UV coordinate to the clip in the texture.</param>
    /// <returns></returns>
    public Vector2 Compute(Vector2 unitaryUv)
    {
        if (unitaryUv.x < 0 || unitaryUv.y > 1 || unitaryUv.y < 0 || unitaryUv.y > 1)
        {
            throw new System.ArgumentException(string.Format("The supplied unitary UV coordinates ({0}) is invalid", unitaryUv));
        }

        var clipOffset = unitaryUv * OffsettedClipDimensions.vector;
        var relativeCoord = OffsettedOrigin.vector + clipOffset;
        var uv = relativeCoord / TextureDimensions.vector;

        return uv;
    }

    /// <summary>
    /// Computes the UV coordinates for the clip's texture square.
    /// This is useful when the image within the clip is actually the entire clip (considering offsets).
    /// </summary>
    /// <returns>UvTexSquare of the clip.</returns>
    public UvTexSquare ComputeUvTexSquare()
        => new UvTexSquare
        {
            LowerLeft = Compute(new Vector2(0f, 0f)),
            LowerRight = Compute(new Vector2(1f, 0f)),
            UpperLeft = Compute(new Vector2(0f, 1f)),
            UpperRight = Compute(new Vector2(1f, 1f)),
        };

    private void UpdateField(ref UvTexDimensions field, UvTexDimensions value)
    {
        var previousValue = field;
        try
        {
            field = value;
            Validate();
        }
        catch
        {
            field = previousValue;
            throw;
        }
    }

    private void Validate()
    {
        if ((OffsettedOrigin.Width > TextureDimensions.Width) ||
            (OffsettedOrigin.Height > TextureDimensions.Height))
        {
            throw new System.ArgumentException(string.Format("Origin {0} cannot be bigger than TextureDimensions {1}", OffsettedOrigin, TextureDimensions));
        }

        if ((OffsettedClipDimensions.Width > TextureDimensions.Width) ||
            (OffsettedClipDimensions.Height > TextureDimensions.Height))
        {
            throw new System.ArgumentException(string.Format("ClipDimensions {0} cannot be bigger than TextureDimensions {1}", OffsettedClipDimensions, TextureDimensions));
        }

        var clipEnd = new UvTexDimensions(OffsettedOrigin.vector + OffsettedClipDimensions.vector);
        if ((clipEnd.Width > TextureDimensions.Width) ||
            (clipEnd.Height > TextureDimensions.Height))
        {
            throw new System.ArgumentException(string.Format("Origin {0} plus ClipDimensions {1} cannot be bigger than TextureDimensions {2}", OffsettedOrigin, OffsettedClipDimensions, TextureDimensions));
        }
    }
}

/// <summary>
/// Describes the two possible orientations a texture grid of tiles
/// can be oriented for UV computation purposes.
/// </summary>
public enum UvGridOrientation
{
    /// <summary>
    /// Given a 2 dimensional grid, index/frame 0 starts on the upper left corner
    /// and increases to the right, following to the row below it.
    ///
    /// Example:
    /// 0, 1, 2, 3, 4
    /// 5, 6, 7, 8, 9
    /// </summary>
    Downwards,

    /// <summary>
    /// Given a 2 dimensional grid, index/frame 0 starts on the lower left corner
    /// and increases to the right, following to the row above it.
    ///
    /// Example:
    /// 5, 6, 7, 8, 9
    /// 0, 1, 2, 3, 4
    /// </summary>
    Upwards,
}

/// <summary>
/// Describes a 2 dimensional texture grid with frames/tiles and provides
/// convenient functions that help compute UV coordinates given the index or row/column coordinates.
/// </summary>
public struct UvGrid
{
    /// <summary>
    /// Number of rows that the grid has.
    /// </summary>
    public uint Rows { get; set; }

    /// <summary>
    /// Number of columns that the grid has.
    /// </summary>
    public uint Columns { get; set; }

    /// <summary>
    /// Overall texture dimensions.
    /// Used to determine clip size when computing the grid frame/tile.
    /// </summary>
    public UvTexDimensions TextureDimensions { get; set; }

    /// <summary>
    /// Determines the frame/tile orientation within the grid.
    /// </summary>
    public UvGridOrientation Orientation { get; set; }

    /// <summary>
    /// Indexer by frame number.
    /// </summary>
    /// <value>Returns a valid UvTexClip that corresponds to the given frame number.</value>
    public UvTexClip this[uint frame]
    {
        get
        {
            return ToUvTexClip(frame);
        }
    }

    /// <summary>
    /// Indexer by (row, column) coordinates.
    /// </summary>
    /// <value>Returns a valid UvTexClip that corresponds to the given (row, column) coordinate.</value>
    public UvTexClip this[uint row, uint column]
    {
        get
        {
            return ToUvTexClip(row, column);
        }
    }

    /// <summary>
    /// Returns a valid UvTexClip object for the given frame/tile within the grid.
    /// </summary>
    public UvTexClip ToUvTexClip(uint frame)
    {
        if (frame >= (Rows * Columns))
        {
            throw new System.ArgumentException("Specified frame must be constrainted by the grid size");
        }

        return ToUvTexClip(frame % Columns, frame / Columns);
    }

    /// <summary>
    /// Returns a valid UvTexClip object for the given (row, column) coordinate within the grid.
    /// </summary>
    public UvTexClip ToUvTexClip(uint row, uint column)
    {
        if ((row >= Rows) || (column >= Columns))
        {
            throw new System.ArgumentException("Specified row and column must be constrainted by the grid size");
        }

        // Compute the frame row/column value based on the orientation
        // and then scale it by frame size
        var frameSize = new Vector2(TextureDimensions.Width / Rows, TextureDimensions.Height / Columns);
        var framev = new Vector2(
            row,
            Orientation == UvGridOrientation.Upwards
                ? column
                : Columns - 1 - column) * frameSize;

        return new UvTexClip(new UvTexDimensions(framev), new UvTexDimensions(frameSize), TextureDimensions);
    }
}