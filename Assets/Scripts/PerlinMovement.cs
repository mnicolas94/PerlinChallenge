using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinMovement : MonoBehaviour
{
    public Bounds movementBounds;
    public float speed;
    
    public float _xOffset;
    public float _yOffset;
    public float _zOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        _xOffset = Random.Range(-10000.0f, 10000.0f);
        _yOffset = Random.Range(-10000.0f, 10000.0f);
        _zOffset = Random.Range(-10000.0f, 10000.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float xnoise = Mathf.PerlinNoise(_xOffset, Time.fixedTime * speed);
        float ynoise = Mathf.PerlinNoise(_yOffset, Time.fixedTime * speed);
        float znoise = Mathf.PerlinNoise(_zOffset, Time.fixedTime * speed);
        var pos = transform.position;
        pos.x = Mathf.Lerp(movementBounds.min.x, movementBounds.max.x, xnoise);
        pos.y = Mathf.Lerp(movementBounds.min.y, movementBounds.max.y, ynoise);
        float scale = Mathf.Lerp(movementBounds.min.z, movementBounds.max.z, znoise);
        transform.position = pos;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
