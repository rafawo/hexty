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

[System.Serializable]
public class HexGridUx
{
    public int Width = 6;
    public int Height = 6;

    public int PaddingWidth = 1;
    public int PaddingHeight = 1;

    public float OuterRadius = 10f;

    public Color DefaultColor = Color.white;
    public Color PaddingColor = Color.white;
    public Color ModeColor = Color.yellow;
    public Color IntersectionColor = Color.gray;
    public Color SelectedColor = Color.magenta;

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
}

[System.Serializable]
public class ProjectileDummy
{
    public GameObject Dummy;
    public PhysHex.Projectile Projectile;
}

[System.Serializable]
public class PhysHexUx
{
    public bool UsePhysHex = false;

    public PhysHex.Particle Particle;
    public float ForceMultiplier = 10f;
    public float ClampValue = 500f;

    public bool UseCustomProjectile = false;
    public string ProjectileType = PhysHex.ProjectileCommonTypeName.Pistol;
    public PhysHex.ProjectileRepository ProjectileRepository = new PhysHex.ProjectileRepository();
    public float ProjectileExpirySeconds = 2;
    public PhysHex.Projectile CustomProjectile = PhysHex.Projectile.Nil;

    public List<ProjectileDummy> Projectiles = new List<ProjectileDummy>();
    public int MaxProjectiles = 2;

    public bool UseFireworks = false;
    public PhysHex.Firework Firework = null;
    public List<GameObject> FireworkSparkPool = new List<GameObject>();
    public int FireworkSparkPoolLimit = 50;
    public List<PhysHex.FireworkPayload> FireworkPayloads = new List<PhysHex.FireworkPayload>() {
        new PhysHex.FireworkPayload {
            MinExpiry = 3f,
            MaxExpiry = 3f,
            MinVelocity = new Vector3(0f, 0f, 5f),
            MaxVelocity = new Vector3(0f, 0f, 5f),
            Damping = 0.5f,
            FuseCount = 1,
            AggregateParentVelocity = false,
            UseParentDirection = true,
        },
        new PhysHex.FireworkPayload {
            MinExpiry = 1f,
            MaxExpiry = 4f,
            MinVelocity = new Vector3(-5f, 0f, -5f),
            MaxVelocity = new Vector3(5f, 0f, 5f),
            Damping = 0.05f,
            FuseCount = 5,
            AggregateParentVelocity = true,
            UseParentDirection = false,
        },
    };
}

public class HexGrid : MonoBehaviour
{
    #region Members

    public HexCell CellPrefab;
    public Text CellLabelPrefab;

    [SerializeField]
    private HexGridUx HexParams = new HexGridUx();

    [SerializeField]
    private PhysHexUx PhysHexParams = new PhysHexUx();

    private Dictionary<HexCubeCoordinates, HexCell> hashedCells
        = new Dictionary<HexCubeCoordinates, HexCell>();

    private Dictionary<HexCubeCoordinates, List<HexCell>> paddingCells
        = new Dictionary<HexCubeCoordinates, List<HexCell>>();

    private Canvas gridCanvas;

    private HexMesh hexMesh;

    private HexOffsetType OffsetType { get => HexParams.UseEvenOffset ? HexOffsetType.Even : HexOffsetType.Odd; }

    private HexOrientation Orientation { get => HexParams.UseFlatHex ? HexOrientation.Flat : HexOrientation.Pointy; }

    private HexMetrics _metrics { get => new HexMetrics(HexParams.OuterRadius); }

    public Color SelectedColor { get => HexParams.SelectedColor; }
    public Color DefaultColor { get => HexParams.DefaultColor; }

    private HexWrapAround hexWrapAround;

    private GameObject _dummy;
    private CameraMovementController _cameraMovement;

    private List<HexConvexPolygon> _polygons = new List<HexConvexPolygon>();
    private List<HexLine> _lines = new List<HexLine>();

    private List<Text> _gridCanvasText = new List<Text>();

    #endregion

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

        if (PhysHexParams != null)
        {
            foreach (var p in PhysHexParams.Projectiles)
            {
                Destroy(p.Dummy);
            }
            PhysHexParams.Projectiles.Clear();
        }
        else
        {
            PhysHexParams = new PhysHexUx();
        }

        HexParams = HexParams ?? new HexGridUx();

        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
        hashedCells = new Dictionary<HexCubeCoordinates, HexCell>();
        paddingCells = new Dictionary<HexCubeCoordinates, List<HexCell>>();
        hexWrapAround = new HexWrapAround(HexParams.Width, HexParams.Height, OffsetType, Orientation);

        for (int x = -HexParams.PaddingWidth; x < HexParams.Width + HexParams.PaddingWidth; ++x)
        {
            for (int z = -HexParams.PaddingHeight; z < HexParams.Height + HexParams.PaddingHeight; ++z)
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
        _dummy = CreateDummy(Color.black);
        _cameraMovement = Camera.main.GetComponent<CameraMovementController>();
        _cameraMovement.ResetCamera();
        _cameraMovement.Hook(_dummy);

        if (HexParams.ColorCurrentPosition)
        {
            ColorCell(_dummy.transform.position, Color.green, hexWrapAround != null);
            ColorCellTriangle(_dummy.transform.position, Color.red, hexWrapAround != null);
            UpdateColors();
        }

        // Create a PhysHex particle that will be the placeholder for the dummy
        // sphere to showcase movement.
        PhysHexParams.Particle = new PhysHex.Particle {
            Damping = 0.75f,
            Mass = 10f,
            Force = new PhysHex.AccruedVector3(Vector3.zero, PhysHexParams.ForceMultiplier),
        };
        _dummy.transform.position = PhysHexParams.Particle.Position;
    }

    private HexCell CreateCellFromOffsetCoordinate(int x, int z)
        => CreateCellFromHexCoordinate(
            HexCubeCoordinates.FromOffsetCoordinates(x, z, Orientation, OffsetType));

    private HexCell CreateCellFromHexCoordinate(HexCubeCoordinates coordinates)
    {
        var position = GetCellPosition(coordinates);
        var cell = Instantiate<HexCell>(CellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.Coordinates = coordinates;
        cell.PaddingCoordinates = hexWrapAround.TransformHex(coordinates);
        cell.ViewColor = cell.IsPadding ? HexParams.PaddingColor : HexParams.DefaultColor;

        if (!HexParams.HideHexLabel)
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

    private GameObject CreateDummy(Color color)
    {
        var dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dummy.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        dummy.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
        return dummy;
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

    public Vector3 GetCellPosition(HexCubeCoordinates coordinates)
        => coordinates.ToPosition(_metrics, Orientation, OffsetType);

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
                        color == HexParams.DefaultColor && paddedCell.IsPadding
                            ? HexParams.PaddingColor
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
                        color == HexParams.DefaultColor && paddedCell.IsPadding
                            ? HexParams.PaddingColor
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

    private void MoveDummy()
    {
        if (PhysHexParams.UsePhysHex)
        {
            bool anyDirectionActive = false;

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Left))
            {
                var force = -Camera.main.transform.right;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                PhysHexParams.Particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Right))
            {
                var force = Camera.main.transform.right;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                PhysHexParams.Particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Down))
            {
                var force = -Camera.main.transform.forward;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                PhysHexParams.Particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Up))
            {
                var force = Camera.main.transform.forward;
                force.y = _dummy.transform.position.y;
                force.Normalize();
                PhysHexParams.Particle.Force.Accrue(force);
                anyDirectionActive = true;
            }

            PhysHexParams.Particle.Force = new PhysHex.AccruedVector3(
                Vector3.ClampMagnitude(
                    PhysHexParams.Particle.Force.Total, PhysHexParams.ClampValue), PhysHexParams.Particle.Force.Multiplier);
            PhysHexParams.Particle.Pause = !anyDirectionActive;
            PhysHexParams.Particle.Integrate(Time.deltaTime);

            if (!anyDirectionActive)
            {
                PhysHexParams.Particle.Force.Reset(PhysHexParams.ForceMultiplier);
                PhysHexParams.Particle.Velocity = Vector3.zero;
            }

            PhysHexParams.Particle.Position = hexWrapAround.TransformPosition(PhysHexParams.Particle.Position, _metrics);
            _dummy.transform.position = PhysHexParams.Particle.Position;
        }
        else
        {
            var force = Vector3.zero;

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Left))
            {
                force = -Camera.main.transform.right;
                force.Normalize();
            }
            else if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Right))
            {
                force = Camera.main.transform.right;
                force.Normalize();
            }

            if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Down))
            {
                force = -Camera.main.transform.forward;
                force.Normalize();
            }
            else if (InputUtilities.IsDirectionActive(InputUtilities.Direction.Up))
            {
                force = Camera.main.transform.forward;
                force.Normalize();
            }

            force.y = _dummy.transform.position.y;
            _dummy.transform.position += force * _cameraMovement.Step;
            _dummy.transform.position = hexWrapAround.TransformPosition(_dummy.transform.position, _metrics);
        }

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

        if (lastCell != newCell && newCell.ViewColor == HexParams.SelectedColor)
        {
            newCell = lastCell;
            newTriangle = lastTriangle;
            _dummy.transform.position = originalPosition;
        }

        if (lastCell != newCell && HexParams.ColorCurrentPosition)
        {
            ColorCell(lastCell, HexParams.DefaultColor, hexWrapAround != null);
            ColorCell(newCell, Color.green, hexWrapAround != null);
        }

        if (lastTriangle.Orientation == newTriangle.Orientation &&
            lastTriangle.Direction != newTriangle.Direction &&
            HexParams.ColorCurrentPosition)
        {
            ResetCellTriangleColor(lastCell, lastTriangle, hexWrapAround != null);
            ColorCellTriangle(newCell, newTriangle, Color.red, hexWrapAround != null);
        }

        HexParams.Direction = newTriangle;
        HexParams.Sections = newTriangle.Sections();
        HexParams.CurrentCoordinates = newCell.Coordinates;
        HexParams.CurrentHexCell = newCell;

        if (PhysHexParams.UsePhysHex)
        {
            ProcessPhysHexCommand();
        }
        else
        {
            ProcessCoordinatesCommand();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var cell in hashedCells.Values)
            {
                ColorCell(cell, HexParams.DefaultColor, true);
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

    private void ProcessPhysHexCommand()
    {
        foreach (var p in PhysHexParams.Projectiles)
        {
            if (p.Projectile.Integrate(Time.deltaTime))
            {
                p.Dummy.transform.position = p.Projectile.Particle.Position;
            }
            else
            {
                p.Dummy.SetActive(false);
            }
        }

        if (PhysHexParams.Firework != null)
        {
            if (!PhysHexParams.Firework.Integrate(Time.deltaTime))
            {
                PhysHexParams.Firework = null;
                foreach (var s in PhysHexParams.FireworkSparkPool)
                {
                    s.SetActive(false);
                }
            }
            else
            {
                foreach (var s in PhysHexParams.Firework.Sparks)
                {
                    if (!s.Projectile.Perishable.Expired)
                    {
                        if (PhysHexParams.FireworkSparkPool.Count < PhysHexParams.FireworkSparkPoolLimit)
                        {
                            var spark = CreateDummy(Color.red);
                            spark.transform.position = s.Projectile.Particle.Position;
                            PhysHexParams.FireworkSparkPool.Add(spark);
                            continue;
                        }
                        else
                        {
                            foreach (var spark in PhysHexParams.FireworkSparkPool)
                            {
                                if (!spark.activeSelf)
                                {
                                    spark.transform.position = s.Projectile.Particle.Position;
                                    spark.SetActive(true);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            PhysHexParams.ProjectileType = PhysHexParams.ProjectileType == PhysHex.ProjectileCommonTypeName.Pistol
                ? PhysHex.ProjectileCommonTypeName.Artillery
                : PhysHexParams.ProjectileType == PhysHex.ProjectileCommonTypeName.Artillery
                    ? PhysHex.ProjectileCommonTypeName.Fireball
                    : PhysHexParams.ProjectileType == PhysHex.ProjectileCommonTypeName.Fireball
                        ? PhysHex.ProjectileCommonTypeName.Laser
                        : PhysHex.ProjectileCommonTypeName.Pistol;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            PhysHexParams.UseFireworks = !PhysHexParams.UseFireworks;
        }

        var shooting = Input.GetMouseButtonUp(2);

        // Middle mouse click.
        // Shoot a projectile from the source coordinates of the
        // dummy to the destination coordinates.
        if (shooting && !PhysHexParams.UseFireworks)
        {
            int index = -1;
            bool freeSpace = false;

            if (PhysHexParams.Projectiles.Count == PhysHexParams.MaxProjectiles)
            {
                foreach (var p in PhysHexParams.Projectiles)
                {
                    ++index;
                    if (p.Projectile.Perishable.Expired)
                    {
                        freeSpace = true;
                        break;
                    }
                }
            }
            else
            {
                PhysHexParams.Projectiles.Add(new ProjectileDummy { Projectile = PhysHex.Projectile.Nil });
                index = PhysHexParams.Projectiles.Count - 1;
                freeSpace = true;
            }

            if (!freeSpace && PhysHexParams.Projectiles.Count == PhysHexParams.MaxProjectiles)
            {
                // There was no space for a new projectile
                return;
            }

            var pd = PhysHexParams.Projectiles[index];
            pd.Projectile.Reset(
                PhysHexParams.ProjectileExpirySeconds,
                GetCellPosition(GetMouseCell(false).Coordinates) - _dummy.transform.position,
                PhysHexParams.UseCustomProjectile
                    ? PhysHexParams.CustomProjectile.Particle.Clone()
                    : PhysHexParams.ProjectileRepository[PhysHexParams.ProjectileType].Particle.Clone());
            pd.Projectile.Particle.Position = _dummy.transform.position;

            // Remove gravity from all for now
            pd.Projectile.Particle.Acceleration = new PhysHex.AccruedVector3();

            if (pd.Dummy == null)
            {
                pd.Dummy = CreateDummy(Color.red);
            }

            pd.Dummy.SetActive(true);
            pd.Dummy.transform.position = _dummy.transform.position;
        }
        else if (shooting && PhysHexParams.UseFireworks && PhysHexParams.Firework == null)
        {
            PhysHexParams.Firework = new PhysHex.Firework(PhysHexParams.FireworkPayloads, new PhysHex.Particle {
                Position = _dummy.transform.position,
                Mass = 1f,
                Velocity = (GetCellPosition(GetMouseCell(false).Coordinates) - _dummy.transform.position).normalized,
            });
        }
    }

    private void ProcessCoordinatesCommand()
    {
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            int axisTypeIndex = ((int)HexParams.Axis + (shiftPressed ? 2 : 1));
            HexParams.Axis = (HexAxis)(axisTypeIndex % 3);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int vertexTypeIndex = ((int)HexParams.VertexType + (shiftPressed ? 5 : 1));
            HexParams.VertexType = (HexVertexType)(vertexTypeIndex % 6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int halfPlaneTypeIndex = ((int)HexParams.HalfPlaneType + (shiftPressed ? 1 : 1));
            HexParams.HalfPlaneType = (HexHalfPlaneType)(halfPlaneTypeIndex % 2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            int triangleTypeIndex = ((int)HexParams.TriangleType + (shiftPressed ? 1 : 1));
            HexParams.TriangleType = (HexTriangleType)(triangleTypeIndex % 2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            int quadrangleTypeIndex = ((int)HexParams.QuadrangleType + (shiftPressed ? 8 : 1));
            HexParams.QuadrangleType = (HexQuadrangleType)(quadrangleTypeIndex % 9);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            int modeIndex = ((int)HexParams.Mode) + (shiftPressed ? (int)HexGridMode.Count -1 : 1);
            HexParams.Mode = (HexGridMode)(modeIndex % (int)HexGridMode.Count);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            HexParams.SpawnUpwards = !HexParams.SpawnUpwards;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            switch (HexParams.Mode)
            {
            case HexGridMode.Diagonal_Neighbors:
                // Colors all diagonal coordinates
                for (int i = 0; i < 6; ++i)
                {
                    ColorCell(
                        GetCell(HexParams.CurrentCoordinates + HexDiagonalUtilities.TranslationVector(i)),
                        HexParams.ModeColor,
                        hexWrapAround != null);
                }
                break;

            case HexGridMode.Vertex:
                ColorCell(
                    GetCell(
                        HexParams.CurrentCoordinates.GetVertex(
                            HexParams.VertexLength,
                            HexParams.VertexType,
                            Orientation,
                            HexParams.Axis)),
                    HexParams.ModeColor,
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
                                HexParams.CurrentCoordinates,
                                angle)),
                        HexParams.ModeColor,
                        hexWrapAround != null);
                }
                break;

            case HexGridMode.HexLine:
                var hexLine = new HexLine(
                    HexParams.Axis,
                    HexParams.Axis == HexAxis.X
                        ? HexParams.CurrentCoordinates.X
                        : HexParams.Axis == HexAxis.Y
                            ? HexParams.CurrentCoordinates.Y
                            : HexParams.CurrentCoordinates.Z);
                foreach (var coords in hexLine.Range(HexParams.CurrentCoordinates, x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }

                    if (HexParams.WrapAroundHexGeometry && cell.IsPadding)
                    {
                        _lines.Add(new HexLine(
                            HexParams.Axis,
                            HexParams.Axis == HexAxis.X
                                ? cell.PaddingCoordinates.X
                                : HexParams.Axis == HexAxis.Y
                                    ? cell.PaddingCoordinates.Y
                                    : cell.PaddingCoordinates.Z));
                    }
                }
                _lines.Add(hexLine);
                break;

            case HexGridMode.Reflections:
                foreach (var line in _lines)
                {
                    var hex = HexParams.CurrentCoordinates.Reflect(line);
                    if (hexWrapAround != null)
                    {
                        hex = hexWrapAround.TransformHex(hex);
                    }
                    ColorCell(GetCell(hex), HexParams.ModeColor, hexWrapAround != null);
                }
                break;

            case HexGridMode.Interpolated_Line:
                var mouseCell = GetMouseCell(!HexParams.WrapAroundHexGeometry);
                if (mouseCell != null)
                {
                    foreach (var coords in HexInterpolatedLine.Range(
                        HexParams.CurrentCoordinates,
                        mouseCell.Coordinates,
                        _metrics,
                        Orientation,
                        OffsetType,
                        x => hashedCells.ContainsKey(x)))
                    {
                        var cell = GetCell(coords);
                        if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                        {
                            ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                        }
                    }
                }
                break;

            case HexGridMode.HexHalfPlane:
                var halfPlane = new HexHalfPlane(
                    HexParams.Axis,
                    HexParams.HalfPlaneType,
                    HexParams.Axis == HexAxis.X
                        ? HexParams.CurrentCoordinates.X
                        : HexParams.Axis == HexAxis.Y
                            ? HexParams.CurrentCoordinates.Y
                            : HexParams.CurrentCoordinates.Z);
                    foreach (var cell in hashedCells.Values)
                    {
                        if (!cell.IsPadding && halfPlane.Contains(cell.Coordinates))
                        {
                            ColorCell(cell, HexParams.ModeColor, false);
                        }
                    }
                break;

            case HexGridMode.Triangle:
                var triangle = HexTriangle.Spawn(
                    HexParams.CurrentCoordinates,
                    HexParams.TriangleLength,
                    HexParams.TriangleType,
                    Orientation,
                    HexParams.Axis,
                    HexParams.SpawnUpwards);
                foreach (var coords in triangle.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(triangle.ConvexPolygon);
                HexParams.TriangleType = triangle.Type;
                break;

            case HexGridMode.Quadrangle:
                var quadrangle = HexQuadrangle.Spawn(
                    HexParams.CurrentCoordinates,
                    HexParams.QuadrangleWidth,
                    HexParams.QuadrangleHeight,
                    HexParams.QuadrangleType,
                    Orientation,
                    HexParams.Axis,
                    HexParams.SpawnUpwards);
                foreach (var coords in quadrangle.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(quadrangle.ConvexPolygon);
                HexParams.QuadrangleTypeOnX = quadrangle.Type[HexAxis.X];
                HexParams.QuadrangleTypeOnY = quadrangle.Type[HexAxis.Y];
                HexParams.QuadrangleTypeOnZ = quadrangle.Type[HexAxis.Z];
                break;

            case HexGridMode.RegularHexagon:
                var hexagon = new HexRegularHexagon(HexParams.CurrentCoordinates, HexParams.RegularHexagonRadius);
                foreach (var coords in hexagon.Range(x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                _polygons.Add(hexagon.ConvexPolygon);
                break;

            case HexGridMode.PolygonsIntersections:
                foreach (var coords in HexConvexPolygon.Intersection(_polygons.ToArray(), x => hashedCells.ContainsKey(x)))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.IntersectionColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Ring:
                foreach (var coords in HexParams.CurrentCoordinates.Ring(HexParams.RegularHexagonRadius))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Spiral:
                foreach (var coords in HexParams.CurrentCoordinates.Spiral(HexParams.RegularHexagonRadius))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Flood:
                foreach (var coords in HexParams.CurrentCoordinates.Flood(HexParams.FloodMovement, WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.Visible:
                var visibleMouseCell = GetMouseCell();
                HexParams.IsMouseVisible = HexParams.CurrentCoordinates.Visible(
                    visibleMouseCell.Coordinates,
                    _metrics,
                    Orientation,
                    OffsetType,
                    WalkableCoordinates);
                break;

            case HexGridMode.FieldOfView:
                foreach (var coords in HexParams.CurrentCoordinates.FieldOfView(
                    HexParams.FieldOfViewRadius,
                    _metrics,
                    Orientation,
                    OffsetType,
                    WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;

            case HexGridMode.FindPath:
                var pathMouseCell = GetMouseCell();
                foreach (var coords in HexParams.CurrentCoordinates.FindPath(
                    pathMouseCell.Coordinates,
                    WalkableCoordinates))
                {
                    var cell = GetCell(coords);
                    if (!cell.IsPadding || HexParams.WrapAroundHexGeometry)
                    {
                        ColorCell(cell, HexParams.ModeColor, HexParams.WrapAroundHexGeometry);
                    }
                }
                break;
            }
        }
    }

    public bool WalkableCoordinates(HexCubeCoordinates coords)
        => hashedCells.ContainsKey(coords) &&
            hashedCells[coords].ViewColor != HexParams.SelectedColor &&
            (!hashedCells[coords].IsPadding || (hashedCells[coords].IsPadding && HexParams.WrapAroundHexGeometry));
}