using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintTexture : MonoBehaviour
{
    // Public variables
    [HideInInspector]public Gradient gradient;
    
    
    // Private variables
    private Color[] _colors;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private float _minH, _maxH;

    public void Preprocess()
    {
        CalculateUVs();
        GetMinAndMaxHeight();
    }
    
    private void CalculateUVs()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _vertices = _mesh.vertices;
        _uvs = new Vector2[_vertices.Length];

        for (int i = 0; i < _uvs.Length; i++)
        {
            float x = _vertices[i].x;
            float z = _vertices[i].z;
            _uvs[i] = new Vector2(x , z );
        }
        _mesh.uv = _uvs;
    }

    public void AssignColors()
    {
        _colors = new Color[GetComponent<MeshFilter>().sharedMesh.vertices.Length];
        for (int i = 0; i < _uvs.Length; i++)
        {
            float height = Mathf.InverseLerp(_minH, _maxH,_vertices[i].y);
            _colors[i] = gradient.Evaluate(height);
        }
        
        _mesh.colors= _colors;
    }

    private void GetMinAndMaxHeight()
    {
        _minH = Mathf.Infinity;
        _maxH = -Mathf.Infinity;
        for (int i = 0; i < _vertices.Length; i++)
        {
            float height = _vertices[i].y;
            if (height > _maxH)
                _maxH = height;
            if (height < _minH)
                _minH = height;
        }
    }
}