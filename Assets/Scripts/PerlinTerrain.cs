using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerlinTerrain : MonoBehaviour
{
    public Terrain terrain;
    public float hillFrecuency = 10f;
    public float lowestHillHeight;
    public float highestHillHeight;

    public int mapw;
    public int maph;
    public int xbase; 
    public int ybase; 

    [NaughtyAttributes.Button()]
    public void GenerateTerrain()
    {
        int res = terrain.terrainData.heightmapResolution;
//        var heights = GenerateHeights(res, res);
        var heights = GenerateHeights(mapw, maph);
        terrain.terrainData.SetHeights(xbase, ybase, heights);
    }
    
    public float[, ] GenerateHeights(int width, int depth)
    {
//        float xoffset = Random.Range(-1000f, 1000f);
//        float zoffset = Random.Range(-1000f, 1000f);
        
        float xoffset = 0;
        float zoffset = 0;

        float terrainHeight = terrain.terrainData.size.y;
        
        float hillHeight = (highestHillHeight - lowestHillHeight) / terrainHeight;
        float baseHeight = lowestHillHeight / terrainHeight;
        float[,] heights = new float[depth, width];
        for (int i = 0; i < depth; i++)
        {
            for (int k = 0; k < width; k++)
            {
                float noise = Mathf.PerlinNoise(
                                   (float) i / depth * hillFrecuency + xoffset,
                                   (float) k / width * hillFrecuency + zoffset);
                heights[i, k] = baseHeight + noise * hillHeight;
            }
        }
        
        return heights;
    }

    [NaughtyAttributes.Button()]
    public void PrintInfo()
    {
        print(terrain.terrainData.heightmapResolution);
        print(terrain.terrainData.size);
    }
}
