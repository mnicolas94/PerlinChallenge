using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{
    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale) {
        // create an empty noise map with the mapDepth and mapWidth coordinates
        float[,] noiseMap = new float[mapDepth, mapWidth];

        float xoffset = Random.Range(-100f, 100f);
        float zoffset = Random.Range(-100f, 100f);
 
        for (int zIndex = 0; zIndex < mapDepth; zIndex ++) {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++) {
                // calculate sample indices based on the coordinates and the scale
                float sampleX = xIndex / scale + xoffset;
                float sampleZ = zIndex / scale + zoffset;
 
                // generate noise value using PerlinNoise
                float noise = Mathf.PerlinNoise (sampleX, sampleZ);
 
                noiseMap [zIndex, xIndex] = noise;
            }
        }
 
        return noiseMap;
    }

}
