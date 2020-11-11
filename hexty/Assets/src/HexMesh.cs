// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh hexMesh;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<int> triangles;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private List<Color> colors;

    public bool UseHexGridTexture = false;

    private UvTexDimensions _texDimensions = new UvTexDimensions(113, 171);
    private UvTexDimensions _texClipDimensions = new UvTexDimensions(15, 16);
    private uint _hexGridRows = 10;
    private uint _hexGridColumns = 7;

    private void Awake()
    {
        hexMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = hexMesh;
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        colors = new List<Color>();
        triangles = new List<int>();

        if (UseHexGridTexture)
        {
            meshRenderer.material = Resources.Load<Material>("HexMeshGridColor");
        }
    }

    public void Triangulate(IEnumerable<HexCell> cells, HexMetrics metrics, HexOrientation orientation, HexOffsetType type)
    {
        hexMesh.Clear();
        vertices.Clear();
        uvs.Clear();
        colors.Clear();
        triangles.Clear();

        foreach (var cell in cells)
        {
            Triangulate(cell, metrics, orientation, type);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.uv = uvs.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();

        SetTexture(orientation);

        meshCollider.sharedMesh = hexMesh;
    }

    public void SetTexture(HexOrientation orientation)
    {
        if (UseHexGridTexture)
        {
            return;
        }

        if (orientation == HexOrientation.Pointy)
        {
            meshRenderer.material.SetFloat("_HexTextureLerp", 0);
        }
        else if (orientation == HexOrientation.Flat)
        {
            meshRenderer.material.SetFloat("_HexTextureLerp", 1);
        }
    }

    private void Triangulate(HexCell cell, HexMetrics metrics, HexOrientation orientation, HexOffsetType type)
    {
        cell.MeshColorIndex = colors.Count;
        foreach (var d in HexDirectionUtilities.Values(orientation))
        {
            Triangulate(d, cell, metrics, type);
        }
    }

    private Vector2[] GetUvs(HexDirection direction)
    {
        if (UseHexGridTexture)
        {
            var row = (uint)Random.Range(0, _hexGridRows - 1);
            var column = (uint)Random.Range(0, _hexGridColumns - 1);
            var origin = new UvTexDimensions(
                (column * (_texClipDimensions.Width + 1)) + 1,
                (row * (_texClipDimensions.Height + 1)) + 1
            );
            var clip = new UvTexClip(origin, _texClipDimensions, _texDimensions);
            return HexCubeCoordinates.GetMeshUvs(direction, clip);
        }
        else
        {
            return HexCubeCoordinates.GetMeshUvs(direction);
        }
    }

    private void Triangulate(HexDirection direction, HexCell cell, HexMetrics metrics, HexOffsetType type)
    {
        var vertices = cell.Coordinates.GetMeshTriangle(direction, metrics, type);
        var uvs = GetUvs(direction);
        AddTriangle(vertices[0], vertices[1], vertices[2], uvs[0], uvs[1], uvs[2]);
        AddTriangleColor(cell.ViewColor);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3) {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }

    private void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void UpdateCellColor(HexCell cell, Color color)
    {
        cell.ViewColor = color;
        foreach (var d in HexDirectionUtilities.Directions())
        {
            UpdateCellTriangleColor(cell, d, cell.ViewColor);
        }
    }

    public void UpdateCellTriangleColor(HexCell cell, int direction, Color color)
    {
        var index = (int)direction * 3;
        for (int i = index; i < index + 3; ++i)
        {
            colors[cell.MeshColorIndex + i] = color;
        }
    }

    public void UpdateColors()
    {
        hexMesh.colors = colors.ToArray();
    }
}
