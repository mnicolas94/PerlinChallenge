using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour
{
    public float heighMultiplier;

    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;
 
    [SerializeField]
    private MeshRenderer tileRenderer;
 
    [SerializeField]
    private MeshFilter meshFilter;
 
    [SerializeField] 
    private MeshCollider meshCollider;
 
    [SerializeField]
    private float mapScale;
 
    void Start() {
        GenerateTile ();
    }
 
    [NaughtyAttributes.Button()]
    void GenerateTile() {
        // calculate tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt (meshVertices.Length);
        int tileWidth = tileDepth;
 
        // calculate the offsets based on the tile position
        float[,] heightMap = this.noiseMapGeneration.GenerateNoiseMap (tileDepth, tileWidth, this.mapScale);
 
        // generate a heightMap using noise
        Texture2D tileTexture = BuildTexture (heightMap);
        tileRenderer.material.mainTexture = tileTexture;
        
        UpdateMeshVertices(heightMap);
    }
 
    private Texture2D BuildTexture(float[,] heightMap) {
        int tileDepth = heightMap.GetLength (0);
        int tileWidth = heightMap.GetLength (1);
 
        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++) {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++) {
                // transform the 2D map index is an Array index
                int colorIndex = zIndex * tileWidth+ xIndex;
                float height= heightMap[zIndex, xIndex];
                // assign as color a shade of grey proportional to the height value
                colorMap [colorIndex] = Color.Lerp (Color.black, Color.white, height);
            }
        }
 
        // create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D (tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels (colorMap);
        tileTexture.Apply ();
 
        return tileTexture;
    }

    public void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength (0);
        int tileWidth = heightMap.GetLength (1);

        Vector3[] meshvertices = meshFilter.mesh.vertices;
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++) {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++) {
                float height = heightMap[zIndex, xIndex];

                Vector3 vertex = meshvertices[vertexIndex];
                meshvertices[vertexIndex] = new Vector3(vertex.x, height * heighMultiplier, vertex.z);

                vertexIndex++;
            }
        }

        meshFilter.mesh.vertices = meshvertices;
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();;

        meshCollider.sharedMesh = meshFilter.mesh;
    }
}
