using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualization : MonoBehaviour
{
    public new AudioSource audio;
    public Bounds viewBounds;

    public LineRenderer line;
    
    public int sampleSize;
    public bool frecuency;

    public Text minCurrent;
    public Text maxCurrent;
    public Text minHist;
    public Text maxHist;
    public Text fundFrec;
    
    private float _minh;
    private float _maxh;

    private void Start()
    {
        _minh = Mathf.Infinity;
        _maxh = Mathf.NegativeInfinity;
    }

    void Update()
    {
        float[] spectrum = new float[sampleSize];
        if (frecuency)
        {
            audio.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        }
        else
        {
            audio.GetOutputData(spectrum, 0);
        }

        PrintDataUI(spectrum);

        float spectSize = _maxh - _minh;
        Vector2 center = new Vector2((float)sampleSize / 2, 0);
        Bounds spectrumBounds = new Bounds(center, new Vector3(sampleSize, spectSize));

        line.positionCount = spectrum.Length;
        for (int i = 0; i < spectrum.Length; i++)
        {
            var pos = NormalizeToBounds(new Vector2(i, spectrum[i]), spectrumBounds, viewBounds);
            line.SetPosition(i, pos);
        }
    }

    private void PrintDataUI(float[] data)
    {
        float min = Mathf.Min(data);
        float max = Mathf.Max(data);
        minCurrent.text = min.ToString();
        maxCurrent.text = max.ToString();
        if (min < _minh)
        {
            minHist.text = minCurrent.text;
            _minh = min;
        }

        if (max > _maxh)
        {
            maxHist.text = maxCurrent.text;
            _maxh = max;
        }

        if (frecuency)
        {
            float ff = FundamentalFrecuency(data);
            fundFrec.text = string.Format("{0:0.00} Hz", ff);
        }
    }

    private Vector2 NormalizeToBounds(Vector2 vec, Bounds from, Bounds to)
    {
        float nx = (vec.x - from.min.x) / (from.size.x) * (to.size.x) + to.min.x;
        float ny = (vec.y - from.min.y) / (from.size.y) * (to.size.y) + to.min.y;
        return new Vector2(nx, ny);
    }

    float FundamentalFrecuency(float[] spectrum)
    {
        int samples = spectrum.Length;
        int sampleRate = AudioSettings.outputSampleRate;
        float hertzPerBin = sampleRate * 0.5f / samples;
        float maxAmplitude = Mathf.NegativeInfinity;
        int maxBin = 0;
        for (int bin = 0; bin < samples; bin++)
        {
            float amp = spectrum[bin];
            if (amp > maxAmplitude)
            {
                maxAmplitude = amp;
                maxBin = bin;
            }
        }

        return (maxBin + 0.5f) * hertzPerBin;
    }
}
