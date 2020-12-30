// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using HexGeometry;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HexGridMode
{
    Diagonal_Neighbors,
    Vertex,
    Rotations,
    HexLine,
    Reflections,
    Interpolated_Line,
    HexHalfPlane,
    Triangle,
    Quadrangle,
    RegularHexagon,
    PolygonsIntersections,
    Ring,
    Spiral,
    Flood,
    Visible,
    FieldOfView,
    FindPath,
    Count,
}

public class HexGrid : MonoBehaviour
{
    #region Members (Unity)

    public bool UsePhysHex = false;

    public int Width = 6;
    public int Height = 6;

    public int PaddingWidth = 1;
    public int PaddingHeight = 1;

    public float OuterRadius = 10f;

    public HexCell CellPrefab;

    public Color DefaultColor = Color.white;
    public Color PaddingColor = Color.white;
    public Color ModeColor = Color.yellow;
    public Color IntersectionColor = Color.gray;
    public Color SelectedColor = Color.magenta;

    public Text CellLabelPrefab;

    public bool UseEvenOffset = false;
    public bool UseFlatHex = false;
    public bool HideHexLabel = false;
    public bool ColorCurrentPosition = true;
    public bool WrapAroundHexGeometry = false;
    public bool SpawnUpwards = true;

    public int VertexLength = 3;
    public int TriangleLength = 3;
    public int QuadrangleWidth = 3;
    public int QuadrangleHeight = 5;
    public int RegularHexagonRadius = 3;
    public int FloodMovement = 5;
    public int FieldOfViewRadius = 5;

    public HexAxis Axis = HexAxis.Z;
    public HexVertexType VertexType;
    public HexHalfPlaneType HalfPlaneType;
    public HexTriangleType TriangleType;
    public HexQuadrangleType QuadrangleType;

    public HexQuadrangleType QuadrangleTypeOnX;
    public HexQuadrangleType QuadrangleTypeOnY;
    public HexQuadrangleType QuadrangleTypeOnZ;

    public HexGridMode Mode = HexGridMode.Diagonal_Neighbors;

    public bool IsMouseVisible = true;

    public HexCubeCoordinates CurrentCoordinates;
    public HexCell CurrentHexCell;
    public HexDirection Direction;
    public HexSection[] Sections;

    #endregion

    #region Members

    private Dictionary<HexCubeCoordinates, HexCell> hashedCells
        = new Dictionary<HexCubeCoordinates, HexCell>();

    private Dictionary<HexCubeCoordinates, List<HexCell>> paddingCells
        = new Dictionary<HexCubeCoordinates, List<HexCell>>();

    private Canvas gridCanvas;

    private HexMesh hexMesh;

    private HexOffsetType OffsetType
    {
        get
        {
            return UseEvenOffset
                ? HexOffsetType.Even
                : HexOffsetType.Odd;
        }
    }

    private HexOrientation Orientation
    {
        get
        {
            return UseFlatHex
                ? HexOrientation.Flat
                : HexOrientation.Pointy;
        }
    }

    private HexMetrics _metrics
    {
        get
        {
            return new HexMetrics(OuterRadius);
        }
    }

    private HexWrapAround hexWrapAround;

    private GameObject _dummy;
    [SerializeField]
    private PhysHex.Particle _particle;
    private float ForceMultiplier = 10f;
    private float ClampValue = 500f;
    private CameraMovementController _cameraMovement;

    private List<HexConvexPolygon> _polygons = new List<HexConvexPolygon>();
    private List<HexLine> _lines = new List<HexLine>();

    private List<Text> _gridCanvasText = new List<Text>();

    #endregion

    #region Methods

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (hashedCells != null)
        {
            foreach (var cell in hashedCells.Values)
            {
                Destroy(cell.gameObject);
            }
        }

        foreach (var text in _gridCanvasText)
        {
            Destroy(text.gameObject);
        }

        if (_dummy != null)
        {
            Destroy(_dummy.gameObject);
        }

        _gridCanvasText.Clear();

        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
        hashedCells = new Dictionary<HexCubeCoordinates, HexCell>();
        paddingCells = new Dictionary<HexCubeCoordinates, List<HexCell>>();
        hexWrapAround = new HexWrapAround(Width, Height, OffsetType, Orientation);

        for (int x = -PaddingWidth; x < Width + PaddingWidth; ++x)
        {
            for (int z = -PaddingHeight; z < Height + PaddingHeight; ++z)
            {
                var cell = CreateCellFromOffsetCoordinate(x, z);
                hashedCells.Add(cell.Coordinates, cell);

                if (!paddingCells.ContainsKey(cell.PaddingCoordinates))
                {
                    paddingCells.Add(cell.PaddingCoordinates, new List<HexCell>());
                }

                paddingCells[cell.PaddingCoordinates].Add(cell);
            }
        }

        foreach (var cell in hashedCells.Values)
        {
            cell.InitializeNeighbors(hashedCells, hexWrapAround);
        }
    }

    private void TriangulateMesh()
        => hexMesh.Triangulate(hashedCells.Values, _metrics, Orientation, OffsetType);

    private void Start()
    {
        Reset();
    }

    private void Reset()
    {
        TriangulateMesh();

        // Create a dummy sphere that will be used to get hooked up to the camera.
        // Start its position at origin and with a rotation of 270 degrees, so that
        // when the camera is hooked it starts looking towards negative Z (0, 0, -1)
        _dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _dummy.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
        _cameraMovement = Camera.main.GetComponent<CameraMovementController>();
        _cameraMovement.ResetCamera();
        _cameraMovement.Hook(_dummy);

        if (ColorCurrentPosition)
        {
            ColorCell(_dummy.transform.position, Color.green, hexWrapAround != null);
            ColorCellTriangle(_dummy.transform.position, Color.red, hexWrapAround != null);
            UpdateColors();
        }

        // Create a PhysHex particle that will be the placeholder for the dummy
        // sphere to showcase movement.
        _particle = new PhysHex.Particle {
            Damping = 0.75f,
            Mass = 10f,
            Force = new PhysHex.AccruedVector3(Vector3.zero, ForceMultiplier),
        };
        _dummy.transform.position = _particle.Position;
    }

    private HexCell CreateCellFromOffsetCoordinate(int x, int z)
        => CreateCellFromHexCoordinate(
            HexCubeCoordinates.FromOffsetCoordinates(x, z, Orientation, OffsetType));

    private HexCell CreateCellFromHexCoordinate(HexCubeCoordinates coordinates)
    {
        var position = coordinates.ToPosition(_metrics, Orientation, OffsetType);
        var cell = Instantiate<HexCell>(CellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.Coordinates = coordinates;
        cell.PaddingCoordinates = hexWrapAround.TransformHex(coordinates);
        cell.ViewColor = cell.IsPadding ? PaddingColor : DefaultColor;

        if (!HideHexLabel)
        {
            Text label = Instantiate<Text>(CellLabelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = cell.PaddingCoordinates.ToStringOnSeparateLines();
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            _gridCanvasText.Add(label);
        }

        return cell;
    }

    public HexCell GetCell(Vector3 position, bool wrapAround = true)
    {
        var pos = transform.InverseTransformDirection(position);
        var hex = HexCubeCoordinates.FromPosition(pos, _metrics, Orientation);
        return GetCell(wrapAround ? hexWrapAround.TransformHex(hex) : hex);
    }

    public HexCell GetCell(HexCubeCoordinates coordinates)
        => hashedCells.ContainsKey(coordinates)
            ? hashedCells[coordinates]
            : null;

    public void ColorCell(Vector3 position, Color color, bool wrapAround = false)
        => ColorCell(GetCell(position), color, wrapAround);

    public void ColorCell(HexCell cell, Color color, bool wrapAround = false)
    {
        if (cell != null)
        {
            hexMesh.UpdateCellColor(cell, color);

            if (paddingCells.ContainsKey(cell.PaddingCoordinates) && wrapAround)
            {
                foreach (var paddedCell in paddingCells[cell.PaddingCoordinates])
                {
                    hexMesh.UpdateCellColor(
                        paddedCell,
                        color == DefaultColor && paddedCell.IsPadding
                            ? PaddingColor
                            : color);
                }
            }
        }
    }

    public void ColorCellTriangle(Vector3 position, Color color, bool wrapAround = false)
    {
        var cell = GetCell(position);
        if (cell != null)
        {
            ColorCellTriangle(
                cell,
                cell.Coordinates.GetRelativeMeshTriangleDirection(
                    position,
                    _metrics,
                    Orientation,
                    OffsetType),
                color,
                wrapAround);
        }
    }

    public void ColorCellTriangle(Vector3 position, HexDirection direction, Color color, bool wrapAround = false)
        => ColorCellTriangle(GetCell(position), direction, color, wrapAround);

    public void ColorCellTriangle(HexCell cell, HexDirection direction, Color color, bool wrapAround = false)
    {
        if (cell != null)
        {
            hexMesh.UpdateCellTriangleColor(cell, direction.Direction, color);

            if (paddingCells.ContainsKey(cell.PaddingCoordinates) && wrapAround)
            {
                foreach (var paddedCell in paddingCells[cell.PaddingCoordinates])
                {
                    hexMesh.UpdateCellTriangleColor(
                        paddedCell,
                        direction.Direction,
                        color == DefaultColor && paddedCell.IsPadding
                            ? PaddingColor
                            : color);
                }
            }
        }
    }

    public void ResetCellTriangleColor(Vector3 position, HexDirection direction, bool wrapAround = false)
     => ResetCellTriangleColor(GetCell(position), direction, wrapAround);

    public void ResetCellTriangleColor(HexCell cell, HexDirection direction, bool wrapAround = false)
    {
        if (cell != null)
        {
            ColorCellTriangle(cell, direction, cell.ViewColor, wrapAround);
        }
    }

    public void UpdateColors()
        => hexMesh.UpdateColors();

    public HexCell GetMouseCell(bool wrapAround = true)
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            return GetCell(hit.point, wrapAround);
        }
        else
        {
            return null;
        }
    }

    private enum InputDirection { Left, Right, Up, Down }

    private bool IsDirectionActive(InputDirection direction)
    {
        switch (direction)
        {
            case InputDirection.Left: return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0;
            case InputDirection.Right: return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0;
            case InputDirection.Down: return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0;
            case InputDirection.Up: return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0;
        }
        return false;
    }

    private void MoveDummy()
    {
        if (UsePhysHex)
        {
            bool anyDirectionActive = false;

            if (IsDirectionActive(InputDirection.Left))
            {
                var force = -Camera.main.transform.right;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                _particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (IsDirectionActive(InputDirection.Right))
            {
                var force = Camera.main.transform.right;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                _particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (IsDirectionActive(InputDirection.Down))
            {
                var force = -Camera.main.transform.forward;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                _particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (IsDirectionActive(InputDirection.Up))
            {
                var force = Camera.main.transform.forward;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                _particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            _particle.Force = new PhysHex.AccruedVector3(
                Vector3.ClampMagnitude(
                    _particle.Force.Total, ClampValue), _particle.Force.Multiplier);
            _particle.Pause = !anyDirectionActive;
            _particle.Integrate(Time.deltaTime);

            if (!anyDirectionActive)
            {
                _particle.Force.Reset(ForceMultiplier);
                _particle.Velocity = Vector3.zero;
            }

            _dummy.transform.position = _particle.Position;
        }
        else
        {
            var force = Vector3.zero;

            if (IsDirectionActive(InputDirection.Left))
            {
                force = -Camera.main.transform.right;
                force.Normalize();
            }
            else if (IsDirectionActive(InputDirection.Right))
            {
                force = Camera.main.transform.right;
                force.Normalize();
            }

            if (IsDirectionActive(InputDirection.Down))
            {
                force = -Camera.main.transform.forward;
                force.Normalize();
            }
            else if (IsDirectionActive(InputDirection.Up))
            {
                force = Camera.main.transform.forward;
                force.Normalize();
            }

            force.y = _dummy.transform.position.y;
            _dummy.transform.position += force * _cameraMovement.Step;
        }

        _dummy.transform.position = hexWrapAround.TransformPosition(_dummy.transform.position, _metrics);
        _dummy.transform.Rotate(
            0,
            Input.GetKey(KeyCode.J)
                ? _cameraMovement.Step
                : Input.GetKey(KeyCode.K)
                    ? -_cameraMovement.Step
                    : 0,
            0
        );
    }

    public void Update()
    {
        var lastCell = GetCell(_dummy.transform.position);
        var metrics = _metrics;
        var lastTriangle = lastCell != null
            ? lastCell.Coordinates
                .GetRelativeMeshTriangleDirection(_dummy.transform.transform.position, metrics, Orientation, OffsetType)
            : new HexDirection(Orientation, 0);

        var originalPosition = _dummy.transform.position;
        MoveDummy();

        var newCell = GetCell(_dummy.transform.position);
        var newTriangle = newCell != null
            ? newCell.Coordinates
                .GetRelativeMeshTriangleDirection(_dummy.transform.position, metrics, Orientation, OffsetType)
            : new HexDirection(Orientation, 0);

        if (lastCell != newCell && newCell.ViewColor == SelectedColor)
        {
            newCell = lastCell;
            newTriangle = lastTriangle;
            _dummy.transform.position = originalPosition;
        }

        if (lastCell != newCell && ColorCurrentPosition)
        {
            ColorCell(lastCell, DefaultColor, hexWrapAround != null);
            ColorCell(newCell, Color.green, hexWrapAround != null);
        }

        if (lastTriangle.Orientation == newTriangle.Orientation &&
            lastTriangle.Direction != newTriangle.Direction &&
            ColorCurrentPosition)
        {
            ResetCellTriangleColor(lastCell, lastTriangle, hexWrapAround != null);
            ColorCellTriangle(newCell, newTriangle, Color.red, hexWrapAround != null);
        }

        Direction = newTriangle;
        Sections = newTriangle.Sections();
        CurrentCoordinates = newCell.Coordinates;
        CurrentHexCell = newCell;

        ProcessCoordinatesCommand();

        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var cell in hashedCells.Values)
            {
                ColorCell(cell, DefaultColor, true);
            }

            _polygons.Clear();
            _lines.Clear();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            Initialize();
            Reset();
        }

        UpdateColors();
    }

    private void ProcessCoordinatesCommand()
    {
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            int axisTypeIndex = ((int)Axis + (shiftPressed ? 2 : 1));
            Axis = (HexAxis)(axisTypeIndex % 3);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int vertexTypeIndex = ((int)VertexType + (shiftPressed ? 5 : 1));
            VertexType = (HexVertexType)(vertexTypeIndex % 6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int halfPlaneTypeIndex = ((int)HalfPlaneType + (shiftPressed ? 1 : 1));
            HalfPlaneType = (HexHalfPlaneType)(halfPlaneTypeIndex % 2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            int triangleTypeIndex = ((int)TriangleType + (shiftPressed ? 1 : 1));
            TriangleType = (HexTriangleType)(triangleTypeIndex % 2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            int quadrangleTypeIndex = ((int)QuadrangleType + (shiftPressed ? 8 : 1));
            QuadrangleType = (HexQuadrangleType)(quadrangleTypeIndex % 9);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            int modeIndex = ((int)Mode) + (shiftPressed ? (int)HexGridMode.Count -1 : 1);
            Mode = (HexGridMode)(modeIndex % (int)HexGridMode.Count);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            SpawnUpwards = !SpawnUpwards;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            switch (Mode)
            {
            case HexGridMode.Diagonal_Neighbors:
                // Colors all diagonal coordinates
                for (int i = 0; i < 6; ++i)
                {
                    ColorCell(
                        GetCell(CurrentCoordinates + HexDiagonalUtilities.TranslationVector(i)),
                        ModeColor,
                        hexWrapAround != null);
                }
                break;

            case HexGridMode.Vertex:
                ColorCell(
                    GetCell(
                        CurrentCoordinates.GetVertex(
                            VertexLength,
                            VertexType,
                            Orientation,
                            Axis)),
                    ModeColor,
                    hexWrapAround != null);
                break;

            case HexGridMode.Rotations:
                var rotatee = GetMouseCell(hexWrapAround != null);
                foreach (var angle in new HexAngleDegree[]
                {
                    HexAngleDegree.d60,
                    HexAngleDegree.d120,
                    HexAngleDegree.d180,
                    HexAngleDegree.d240,
                    HexAngleDegree.d300,
                    HexAngleDegree.d360,
                })
                {
                    ColorCell(
                        GetCell(
                            rotatee.Coordinates.Rotate(
                                CurrentCoordinates,
                                angle)),
                        ModeColor,
                        hexWrapAround != null);
                }
                break;

            case HexGridMode.HexLine:
                var hexLine = new HexLine(
                    Axis,
                    Axis == HexAxis.X
                        ? CurrentCoordinates.X
                        : Axis == HexAxis.Y
                            ? CurrentCoordinates.Y
                            : CurrentCoordinates.Z);
                foreach (var coords in hexLine.Range(CurrentCoordinates, x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }

                    if (WrapAroundHexGeometry && cell.IsPadding)
                    {
                        _lines.Add(new HexLine(
                            Axis,
                            Axis == HexAxis.X
                                ? cell.PaddingCoordinates.X
                                : Axis == HexAxis.Y
                                    ? cell.PaddingCoordinates.Y
                                    : cell.PaddingCoordinates.Z));
                    }
                }
                _lines.Add(hexLine);
                break;

            case HexGridMode.Reflections:
                foreach (var line in _lines)
                {
                    var hex = CurrentCoordinates.Reflect(line);
                    if (hexWrapAround != null)
                    {
                        hex = hexWrapAround.TransformHex(hex);
                    }
                    ColorCell(GetCell(hex), ModeColor, hexWrapAround != null);
                }
                break;

            case HexGridMode.Interpolated_Line:
                var mouseCell = GetMouseCell(!WrapAroundHexGeometry);
                if (mouseCell != null)
                {
                    foreach (var coords in HexInterpolatedLine.Range(
                        CurrentCoordinates,
                        mouseCell.Coordinates,
                        _metrics,
                        Orientation,
                        OffsetType,
                        x => hashedCells.ContainsKey(x)))
                    {
                        var cell = GetCell(coords);
                        if (!cell.IsPadding || WrapAroundHexGeometry)
                        {
                            ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                        }
                    }
                }
                break;

            case HexGridMode.HexHalfPlane:
                var halfPlane = new HexHalfPlane(
                    Axis,
                    HalfPlaneType,
                    Axis == HexAxis.X
                        ? CurrentCoordinates.X
                        : Axis == HexAxis.Y
                            ? CurrentCoordinates.Y
                            : CurrentCoordinates.Z);
                    foreach (var cell in hashedCells.Values)
                    {
                        if (!cell.IsPadding && halfPlane.Contains(cell.Coordinates))
                        {
                            ColorCell(cell, ModeColor, false);
                        }
                    }
                break;

            case HexGridMode.Triangle:
                var triangle = HexTriangle.Spawn(
                    CurrentCoordinates,
                    TriangleLength,
                    TriangleType,
                    Orientation,
                    Axis,
                    SpawnUpwards);
                foreach (var coords in triangle.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(triangle.ConvexPolygon);
                TriangleType = triangle.Type;
                break;

            case HexGridMode.Quadrangle:
                var quadrangle = HexQuadrangle.Spawn(
                    CurrentCoordinates,
                    QuadrangleWidth,
                    QuadrangleHeight,
                    QuadrangleType,
                    Orientation,
                    Axis,
                    SpawnUpwards);
                foreach (var coords in quadrangle.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(quadrangle.ConvexPolygon);
                QuadrangleTypeOnX = quadrangle.Type[HexAxis.X];
                QuadrangleTypeOnY = quadrangle.Type[HexAxis.Y];
                QuadrangleTypeOnZ = quadrangle.Type[HexAxis.Z];
                break;

            case HexGridMode.RegularHexagon:
                var hexagon = new HexRegularHexagon(CurrentCoordinates, RegularHexagonRadius);
                foreach (var coords in hexagon.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(hexagon.ConvexPolygon);
                break;

            case HexGridMode.PolygonsIntersections:
                foreach (var coords in HexConvexPolygon.Intersection(_polygons.ToArray(), x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, IntersectionColor, WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Ring:
                foreach (var coords in CurrentCoordinates.Ring(RegularHexagonRadius))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Spiral:
                foreach (var coords in CurrentCoordinates.Spiral(RegularHexagonRadius))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Flood:
                foreach (var coords in CurrentCoordinates.Flood(FloodMovement, WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Visible:
                var visibleMouseCell = GetMouseCell();
                IsMouseVisible = CurrentCoordinates.Visible(
                    visibleMouseCell.Coordinates,
                    _metrics,
                    Orientation,
                    OffsetType,
                    WalkableCoordinates);
                break;

            case HexGridMode.FieldOfView:
                foreach (var coords in CurrentCoordinates.FieldOfView(
                    FieldOfViewRadius,
                    _metrics,
                    Orientation,
                    OffsetType,
                    WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.FindPath:
                var pathMouseCell = GetMouseCell();
                foreach (var coords in CurrentCoordinates.FindPath(
                    pathMouseCell.Coordinates,
                    WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || WrapAroundHexGeometry)
                    {
                        ColorCell(cell, ModeColor, WrapAroundHexGeometry);
                    }
                }
                break;
            }
        }
    }

    public bool WalkableCoordinates(HexCubeCoordinates coords)
        => hashedCells.ContainsKey(coords) &&
            hashedCells[coords].ViewColor != SelectedColor &&
            (!hashedCells[coords].IsPadding || (hashedCells[coords].IsPadding && WrapAroundHexGeometry));

    #endregion
}