using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class testperlin : MonoBehaviour
{
    public Text text;

    public float speed;

    private float _roadNoisePos;

    public int n;
    
    [NaughtyAttributes.Button()]
    public void Compute()
    {
        _roadNoisePos = Random.Range(-1000f, 1000f);    

        float mean = 0;
        for (int i = 0; i < n; i++)
        {
//            float n = Mathf.PerlinNoise(_roadNoisePos, _roadNoisePos);
//            float n = noise.pnoise(Vector2.one * this._roadNoisePos, Vector2.one * this._roadNoisePos);
            float n = noise.cnoise(Vector2.one * this._roadNoisePos);
            mean += n;
            _roadNoisePos += speed;
        }
        mean /= n;
        text.text = mean.ToString();
    }
}
