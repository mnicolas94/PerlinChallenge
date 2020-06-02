using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainColor : MonoBehaviour
{
    public ColorFloatDict heightColorThresholds;

    public Color32 roadColor;
    public Color32 roadEdgeColor;
    
    public float roadWidth;
    public float roadEdgeWith;
    public float evadeHillAcceleration;
    
    public float perlinAcceleration;
    public float perlinSpeed;

    public float tocenterAcceleration;

    private float _roadPos;
    private float _roadNoisePos;

    private void Start()
    {
        _roadPos = 0f;  // esta variabke debe ir de [-1 ; 1]
        _roadNoisePos = Random.Range(-1000f, 1000f);
    }

    public Color32[] GetHeightColors(float[] heights)
    {
        int len = heights.Length;
        Color32[] colors = new Color32[len];
        var colorKeys = new List<Color32>(heightColorThresholds.Keys);

        _roadPos = NextRoadPos(heights);
        _roadPos = Mathf.Clamp(_roadPos, -1 + roadWidth, 1 - roadWidth);  // limitarlo pa que el camino no se salga del terreno
        float leftEdge = _roadPos - roadWidth;
        float righEdge = _roadPos + roadWidth;

        int leleIndex = (int)((leftEdge + 1) / 2 * len);                    // borde izq del borde izq del camino
        int lereIndex = (int)(((leftEdge + roadEdgeWith) + 1) / 2 * len);     // borde der del borde izq del camino
        int rereIndex = (int)((righEdge + 1) / 2 * len);                    // borde der del borde der del camino
        int releIndex = (int)(((righEdge - roadEdgeWith) + 1) / 2 * len);     // borde izq del borde der del camino
        
        for (int x = 0; x < len; x++)
        {
            if (roadEdgeWith > 0 && x >= leleIndex && x <= lereIndex)  // color del borde izq del camino
            {
                colors[x] = roadEdgeColor;
            }
            else if (roadEdgeWith > 0 && x >= releIndex && x <= rereIndex)  // color del borde der del camino
            {
                colors[x] = roadEdgeColor;
            }
            else if (x > lereIndex && x < releIndex)  // color del camino
            {
                colors[x] = roadColor;
            }
            else
            {
                float height = heights[x];

                for (int i = colorKeys.Count - 1; i >= 0; i--)
                {
                    Color32 color = colorKeys[i];
                    float thresh = heightColorThresholds[color];
                    if (height <= thresh)
                    {
                        colors[x] = color;
                        break;
                    }
                }
            }
        }

        return colors;
    }

    float NextRoadPos(float[] heights)
    {
        float evadeSpeed = EvadeSpeed(heights);
        float noiseSpeed = NextNoiseSpeed();
        float tocenterSpeed = ToCenterSpeed();
        float speed = (evadeSpeed + noiseSpeed + tocenterSpeed) / 3;
        return _roadPos + speed;
    }
    
    public float EvadeSpeed(float[] heights)
    {
        int len = heights.Length;
        
        // calcular movimiento que evada las lomitas evadiendo más a las lomas más altas y más cercanas
        int roadPosIndex = Mathf.RoundToInt((_roadPos + 1) / 2 * len);
        float evadeDir = 0;
        for (int x = 0; x < len; x++)
        {
            float height = heights[x];  // [0 ; 1]
            
            float dist = (roadPosIndex - x) / (float)len;  // esto va de [-1 ; 1]
            float sign = Mathf.Sign(dist);
            float distInfluence = sign * Mathf.Pow(dist - sign, 2);  // -(x + 1)^2 para x < 0 y (x - 1)^2 para x > 0
            if (dist == 0)
            {
                distInfluence = Random.Range(0, 2) * 2 - 1;  // -1 o 1
            }

            evadeDir += height * distInfluence;  // [-1 ; 1]
        }
        evadeDir /= len;
        
        float evadeSpeed = evadeDir * evadeHillAcceleration;
        return evadeSpeed;
    }
    
    float NextNoiseSpeed()
    {
//        float n = Mathf.PerlinNoise(_roadNoisePos, _roadNoisePos) + 0.043f;  // esta suma se debe a que perlin me da en promedio 0.457
//        n = Mathf.Clamp01(n);
//        float noiseDir = (n - 0.5f) * 2f;  // llevarlo al rango [-1 ; 1]
        float noiseDir = noise.cnoise(Vector2.one * _roadNoisePos);
        _roadNoisePos += perlinSpeed;
        float noiseSpeed = noiseDir * perlinAcceleration;
        return noiseSpeed;
    }

    float ToCenterSpeed()
    {
        return -_roadPos * tocenterAcceleration;
    }
}

[System.Serializable]
public class ColorFloatDict : SerializableDictionaryBase<Color32, float>
{
}