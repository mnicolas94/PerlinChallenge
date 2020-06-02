using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor.UI;
using UnityEngine.XR;

public class CreatePlane : MonoBehaviour
{
    private const int VerticesLimit = 64000;
    private const int TrianglesLimit = 390000;  // esto es un aproximado
    private Vector4 Tangent = new Vector4(1f, 0f, 0f, -1f);

    public Action<float> eventTerrainMoved;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public AudioToHeightmap athm;
    public TerrainColor terrainColor;

    public float heightScale;

    public int widthSegments = 1;
    public int lengthSegments = 1;
    public float width = 1.0f;
    public float length = 1.0f;
    
    [HideInInspector] [SerializeField] private int _currentwidthSegments = 1;
    [HideInInspector] [SerializeField] private int _currentlengthSegments = 1;
    [HideInInspector] [SerializeField] private float _currentwidth = 1.0f;
    [HideInInspector] [SerializeField] private float _currentlength = 1.0f;

    [HideInInspector] [SerializeField] private List<Vector3> _vertices;
    [HideInInspector] [SerializeField] private List<Vector2> _uvs;
    [HideInInspector] [SerializeField] private List<int> _triangles;
    [HideInInspector] [SerializeField] private List<Vector4> _tangents;
    [HideInInspector] [SerializeField] private List<Color32> _colors;

    private void Start()
    {
        _vertices = new List<Vector3>(VerticesLimit);
        _uvs = new List<Vector2>(VerticesLimit);
        _triangles = new List<int>(TrianglesLimit);
        _tangents = new List<Vector4>(VerticesLimit);
        _colors = new List<Color32>(VerticesLimit);
    }

    [NaughtyAttributes.Button()]
    public void InitializePlane()
    {
        _currentwidthSegments = widthSegments;
        _currentlengthSegments = lengthSegments;
        _currentwidth = width;
        _currentlength = length;
		Mesh m = meshFilter.sharedMesh;
        m.Clear();
		
        int hCount = widthSegments+1;
        int verticalLimit = VerticesLimit / hCount;
        // limitar la cantidad de filas para no sobrepasar el límite de vértices
        int vCount = Math.Min(lengthSegments+1, verticalLimit);
        
        _vertices.Clear();
        _uvs.Clear();
        _triangles.Clear();
        _tangents.Clear();
        _colors.Clear();
        
        float uvFactorX = 1.0f/widthSegments;
        float uvFactorY = 1.0f/lengthSegments;
        float scaleX = width/widthSegments;
        float scaleY = length/lengthSegments;
        for (float y = 0.0f; y < vCount; y++)
        {
            for (float x = 0.0f; x < hCount; x++)
            {
                _vertices.Add(new Vector3(x*scaleX, 0.0f, y*scaleY));
                _tangents.Add(Tangent);
                _uvs.Add(new Vector2(x*uvFactorX, y*uvFactorY));
                _colors.Add(Color.white);
            }
        }

        for (int y = 0; y < vCount - 1; y++)
        {
            for (int x = 0; x < hCount - 1; x++)
            {
                _triangles.Add((y     * hCount) + x);
                _triangles.Add(((y+1) * hCount) + x);
                _triangles.Add((y     * hCount) + x + 1);

                _triangles.Add(((y+1) * hCount) + x);
                _triangles.Add(((y+1) * hCount) + x + 1);
                _triangles.Add((y     * hCount) + x + 1);
            }
        }
        
        m.SetVertices(_vertices);
        m.SetUVs(0, _uvs);
        m.SetTriangles(_triangles, 0);
        m.SetTangents(_tangents);
        m.SetColors(_colors);
        m.RecalculateNormals();
        meshFilter.sharedMesh = m;
        m.RecalculateBounds();
    }

    [NaughtyAttributes.Button()]
    public bool AddRow()
    {
		Mesh m = meshFilter.sharedMesh;
        _vertices.Clear();
        m.GetVertices(_vertices);
        int hCount = _currentwidthSegments + 1;
        bool overflow = _vertices.Count + hCount > VerticesLimit;
        
        if (!overflow)  // si añadir la fila no sobrepasa el límite de vértices
        {
            _uvs.Clear();
            _triangles.Clear();
            _tangents.Clear();
            _colors.Clear();
            m.GetTriangles(_triangles, 0);
            m.GetTangents(_tangents);
            m.GetColors(_colors);
            
            float scaleX = _currentwidth/_currentwidthSegments;
            float scaleY = _currentlength/_currentlengthSegments;
        
            // aumentar la cantidad de vértices a lo largo
            _currentlength += scaleY;
            _currentlengthSegments++;
        
            int vCount = _currentlengthSegments + 1;
            int lastY = vCount - 1;
            float uvFactorX = 1.0f/_currentwidthSegments;
            float uvFactorY = 1.0f/_currentlengthSegments;
            
            for (float x = 0.0f; x < hCount; x++)
            {
                _vertices.Add(new Vector3(x * scaleX, 0.0f, lastY * scaleY));
                _tangents.Add(Tangent);
                _colors.Add(Color.white);

                for (float y = 0.0f; y < vCount; y++)  // Los uv hay que recalcularlos completos
                {
                    _uvs.Add(new Vector2(x*uvFactorX, y*uvFactorY));
                }
            }

            int lastSegmentY = _currentlengthSegments - 1;
            for (int x = 0; x < _currentwidthSegments; x++)
            {
                _triangles.Add(lastSegmentY * hCount + x);
                _triangles.Add((lastSegmentY + 1) * hCount + x);
                _triangles.Add(lastSegmentY * hCount + x + 1);

                _triangles.Add((lastSegmentY + 1) * hCount + x);
                _triangles.Add((lastSegmentY + 1) * hCount + x + 1);
                _triangles.Add(lastSegmentY * hCount + x + 1);
            }
            
            m.SetVertices(_vertices);
            m.SetUVs(0, _uvs);
            m.SetTriangles(_triangles, 0);
            m.SetTangents(_tangents);
            m.SetColors(_colors);
            m.RecalculateNormals();
 
            meshFilter.sharedMesh = m;
            m.RecalculateBounds();
        }

        return overflow;
    }

    public void DisplaceVerticesInZ()
    {
        _vertices.Clear();
        _colors.Clear();
        meshFilter.sharedMesh.GetVertices(_vertices);
        meshFilter.sharedMesh.GetColors(_colors);
        int hCount = _currentwidthSegments + 1;
        int vCount = _vertices.Count / hCount;

        for (int z = 1; z < vCount; z++)
        {
            for (int x = 0; x < hCount; x++)
            {
                int index = z * hCount + x;
                int beforeIndex = (z - 1) * hCount + x;
                Vector3 beforeVertex = _vertices[beforeIndex];
                beforeVertex.y = _vertices[index].y;
                _vertices[beforeIndex] = beforeVertex;
                _colors[beforeIndex] = _colors[index];
            }
        }
        meshFilter.sharedMesh.SetVertices(_vertices);
        meshFilter.sharedMesh.SetColors(_colors);
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();
    }

    public void UpdateLastRowMeshVertices(float[] heights, Color32[] colors)
    {
        _vertices.Clear();
        _colors.Clear();
        meshFilter.sharedMesh.GetVertices(_vertices);
        meshFilter.sharedMesh.GetColors(_colors);
        int lastRowIndex = _vertices.Count - _currentwidthSegments;
        for (int xIndex = 0; xIndex < heights.Length; xIndex++) {
            float height = heights[xIndex];
            Color32 color = colors[xIndex];

            Vector3 vertex = _vertices[lastRowIndex];
            _vertices[lastRowIndex] = new Vector3(vertex.x, height * heightScale, vertex.z);
            _colors[lastRowIndex] = color;

            lastRowIndex++;
        }

        meshFilter.sharedMesh.SetVertices(_vertices);
        meshFilter.sharedMesh.SetColors(_colors);
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();
    }
    
    public void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength (0);
        int tileWidth = heightMap.GetLength (1);

        _vertices.Clear();
        meshFilter.sharedMesh.GetVertices(_vertices);
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++) {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++) {
                float height = heightMap[zIndex, xIndex];

                Vector3 vertex = _vertices[vertexIndex];
                _vertices[vertexIndex] = new Vector3(vertex.x, height * heightScale, vertex.z);

                vertexIndex++;
            }
        }

        meshFilter.sharedMesh.SetVertices(_vertices);
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals();
    }
    
    private void Update()
    {
        float segmentLength = length / lengthSegments;
        bool overflow = AddRow();
        if (!overflow)
        {
            transform.position = transform.position + transform.forward * -segmentLength;
        }
        else
        {
            DisplaceVerticesInZ();
        }
        float[] rowheights = athm.GetRowHeightMap(_currentwidthSegments);
        Color32[] colors = terrainColor.GetHeightColors(rowheights);
        UpdateLastRowMeshVertices(rowheights, colors);
        eventTerrainMoved?.Invoke(segmentLength);
    }

    [NaughtyAttributes.Button()]
    public void PrintInfo()
    {
        print("Vertices: " + meshFilter.sharedMesh.vertices.Length);
        print("Dimensions: " + _currentwidthSegments + " x " + _currentlengthSegments);
        print("Colors: " + meshFilter.sharedMesh.colors.Length);
    }

    public float[,] GetHeightmap(out float minHeight, out float maxHeigh)
    {
        _vertices.Clear();
        meshFilter.sharedMesh.GetVertices(_vertices);
        int numVertices = _vertices.Count;
        int width = _currentwidthSegments + 1;
        int depth = numVertices / width;

        minHeight = Mathf.Infinity;
        maxHeigh = Mathf.NegativeInfinity;
        
        float[,] hmp = new float[depth, width];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = z * width + x;
                float height = _vertices[index].y;
                hmp[z, x] = height;

                if (height < minHeight) minHeight = height;
                if (height > maxHeigh) maxHeigh = height;
            }
        }
        return hmp;
    }
}

public enum Orientation
    {
        Horizontal,
        Vertical
    }
