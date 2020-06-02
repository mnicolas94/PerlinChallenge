using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class AudioToHeightmap : MonoBehaviour
{
    public new AudioSource audio;
    public int sampleSize;
    public FFTWindow window = FFTWindow.Rectangular;

    public int maxHarmonics;
    public float harmonicsThresholds = 0.5f;
    public int variance;
    
    public float minFrecuency;
    public float maxFrecuency;

    public int trackLastFramesCount;
    private List<float[]> _lastFramesHeights;

    public float noiseInfluence;
    public float perlinSpreadX;
    public float perlinSpeedZ;
    private float _perlinx;
    private float _perlinz;
    
    private void Start()
    {
        _lastFramesHeights = new List<float[]>();
        _perlinx = Random.Range(-1000f, 1000f);
        _perlinz = Random.Range(-1000f, 1000f);
    }

    public float[] GetRowHeightMap(int length)
    {
        float[] heights = new float[length];
        float[] rawData = new float[sampleSize];
        float[] spectrum = new float[sampleSize];
        int sampleRate = AudioSettings.outputSampleRate;
        float hertzPerBin = sampleRate * 0.5f / sampleSize;
        int minFrecBin = (int)(minFrecuency / hertzPerBin);
        int maxFrecBin = (int)(maxFrecuency / hertzPerBin);
        
        int relevantSamples = maxFrecBin - minFrecBin;
        float binsPerHeight = (float)relevantSamples / length;
        
        audio.GetOutputData(rawData, 0);
        audio.GetSpectrumData(spectrum, 0, window);

        float maxAmplitude = Mathf.Max(rawData);

        List<int> harmonicBins = NoteAndHarmonics(spectrum, harmonicsThresholds);

        for (int x = 0; x < length; x++)
        {
            float binsInfluence = 0;
            int ini = minFrecBin + (int)(x * binsPerHeight);
            int end = minFrecBin + (int)(x * binsPerHeight + binsPerHeight);
            end = Math.Max(end, ini + 1);
            for (int bin = ini; bin < end; bin++)
            {
                float gn = GaussNorm(bin, harmonicBins);
                binsInfluence += gn;
            }
            
            float height = binsInfluence / (end - ini) * maxAmplitude;
            height = Mathf.Clamp01(height);
            heights[x] = height;
        }
        
        _lastFramesHeights.Add(heights);
        if (_lastFramesHeights.Count > trackLastFramesCount)
            _lastFramesHeights.RemoveAt(0);

        heights = MeanFrames();

        ApplyPerlinNoise(ref heights);
        
        return heights;
    }
    
    public float[,] GetHeightMap(int depth, int width)
    {
        float[,] heightmap = new float[depth, width];
        float[] rawData = new float[sampleSize];
        float[] spectrum = new float[sampleSize];
        float scaleX = ((float)sampleSize) / width;
        float scaleZ = ((float)sampleSize) / depth;
        
        audio.GetOutputData(rawData, 0);
        audio.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heightmap[z, x] = Mathf.Abs(rawData[(int)(x * scaleX)]) * spectrum[(int)(z * scaleZ)];
            }
        }
        
        return heightmap;
    }

    private float AbsoluteMean(float[] samples)
    {
        float mean = 0;
        int len = samples.Length;
        for (int i = 0; i < len; i++)
        {
            mean += Mathf.Abs(samples[i]);
        }

        return mean / len;
    }

    private List<int> NoteAndHarmonics(float[] spectrum, float threshold)
    {
        List<int> harmonicBins = new List<int>();
        int len = spectrum.Length;
        int sampleRate = AudioSettings.outputSampleRate;
        float hertzPerBin = sampleRate * 0.5f / len;
        float hallfHertzPerBin = hertzPerBin / 2;

        float maxAmplitude = Mathf.NegativeInfinity;
        float fundamentalFrec = hallfHertzPerBin;
        for (int i = 0; i < len; i++)
        {
            if (spectrum[i] > maxAmplitude)
            {
                maxAmplitude = spectrum[i];
                fundamentalFrec = i * hertzPerBin + hallfHertzPerBin;
            }
        }

        for (int i = 1; i <= maxHarmonics + 1; i++)
        {
            int bin = (int)(i * fundamentalFrec / hertzPerBin);
            if (bin >= len)  // si se pasa del límite de las frecuencias
                break;
            if (spectrum[bin] >= threshold * maxAmplitude)
            {
                harmonicBins.Add(bin);
            }
        }

        return harmonicBins;
    }

    private float GaussNorm(int bin, List<int> harmonicBins)
    {
        float sum = 0;
        for (int i = 0; i < harmonicBins.Count; i++)
        {
            float g = Mathf.Exp(-Mathf.Pow(bin - harmonicBins[i], 2) / (variance * variance * 2));
            sum += g;
        }

        return sum;
    }

    private float[] MeanFrames()
    {
        int frames = _lastFramesHeights.Count;
        if (frames > 0)
        {
            int width = _lastFramesHeights[0].Length;
            float[] mean = new float[width];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < frames; j++)
                {
                    mean[i] += _lastFramesHeights[j][i];
                }

                mean[i] /= frames;
            }

            return mean;
        }
        else
        {
            return new float[0];
        }
    }

    void ApplyPerlinNoise(ref float[] heights)
    {
        int len = heights.Length;

        for (int x = 0; x < len; x++)
        {
            float noise = Mathf.PerlinNoise(x * perlinSpreadX + _perlinx, _perlinz);
            heights[x] = (heights[x] * (1 - noiseInfluence) + noise * noiseInfluence);
            heights[x] /= 2;
        }

        _perlinz += perlinSpeedZ;
    }
        
}
