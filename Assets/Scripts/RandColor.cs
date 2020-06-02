using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandColor : MonoBehaviour
{
    public SpriteRenderer sr;

    private void Start()
    {
        sr.color = GetRandomColor();
    }

    private Color GetRandomColor()
    {
        var rcolor = Random.ColorHSV(
            0,1,
            1,1,
            0.8f,1,
            1, 1);
        return rcolor;
    }
}
