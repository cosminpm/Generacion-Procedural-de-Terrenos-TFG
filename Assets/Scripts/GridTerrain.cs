using System;
using GridGeneratorEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class GridTerrain : MonoBehaviour
{
    private Mesh _mesh;
    private bool _applyChanges;
    private Vector3[] _vertices;
    private int[] _triangles;
    [HideInInspector] public bool initializationFinished;
    private GridGenerator _gridGenerator;
    private int _xSize, _zSise, _originalXSize, _originalZSize;
    private Vector3 _originalPosition;

    private Vector3[] _originalVertices;
    private int[] _originalTriangles;
    public List<FullMesh> previousMeshes;
    private int _nOfPreviuousMeshes = 10;
    private Color[] _colors;

    public float[] octavesF, octavesA;

    private GameObject _vertex;

    // We create a class with the mesh and the sizes
    // I am going to use this to use this to update the last five mesh
    public class FullMesh
    {
        private int xSize;
        private int zSize;
        private Vector3[] vertices;
        private int[] triangles;

        public FullMesh(int x, int z, Vector3[] v, int[] t)
        {
            xSize = x;
            zSize = z;
            vertices = v;
            triangles = t;
        }

        public int getxSize()
        {
            return xSize;
        }

        public int getzSize()
        {
            return zSize;
        }

        public Vector3[] getVertices()
        {
            return vertices;
        }

        public int[] getTriangles()
        {
            return triangles;
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void InitializePreviousMeshes()
    {
        previousMeshes = new List<FullMesh> {new FullMesh(_xSize, _zSise, _vertices, _triangles)};
    }

    public void UpdateLastXMesh()
    {
        if (previousMeshes.Count > _nOfPreviuousMeshes)
        {
            previousMeshes.RemoveAt(0);
        }

        previousMeshes.Add(new FullMesh(_xSize, _zSise, _vertices, _triangles));
    }

    public void ChangeToPreviousMesh(int indexPreviousMeshes)
    {
        _xSize = previousMeshes.ElementAt(indexPreviousMeshes).getxSize();
        _zSise = previousMeshes.ElementAt(indexPreviousMeshes).getzSize();

        int verticesLen = previousMeshes.ElementAt(indexPreviousMeshes).getVertices().Length;
        int trianglesLen = previousMeshes.ElementAt(indexPreviousMeshes).getTriangles().Length;

        _vertices = new Vector3[verticesLen];
        _triangles = new int[trianglesLen];

        Array.Copy(previousMeshes.ElementAt(indexPreviousMeshes).getVertices(), _vertices, verticesLen);
        Array.Copy(previousMeshes.ElementAt(indexPreviousMeshes).getTriangles(), _triangles, trianglesLen);

        UpdateMesh();
    }

    private void OnValidate()
    {
        _applyChanges = true;
    }

    private void Update()
    {
        if (_applyChanges)
        {
            _applyChanges = false;
            UpdateMesh();
        }
    }

    private void Initialize()
    {
        _gridGenerator = GetComponent<GridGenerator>();
        _mesh = _gridGenerator.mesh;
        CreateShape();
        _xSize = _gridGenerator.xSize;
        _zSise = _gridGenerator.zSize;
        InitializePreviousMeshes();
        UpdateMesh();
        _originalZSize = _zSise;
        _originalXSize = _xSize;

        _originalTriangles = _gridGenerator.triangles;
        _originalVertices = _gridGenerator.vertices;

        if (_gridGenerator.finishMeshZero)
            DestroyImmediate(_gridGenerator);
        initializationFinished = true;
    }

    private void CreateShape()
    {
        _vertices = _mesh.vertices;
        _triangles = _mesh.triangles;
    }

    public void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = new Vector3[_vertices.Length];
        _mesh.triangles = new int[_triangles.Length];

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
    }

    public void Subdivide()
    {
        List<Vector3> verticesAux = new List<Vector3>();
        List<int> trianglesAux = new List<int>();

        foreach (var v in _vertices)
        {
            verticesAux.Add(v);
        }

        for (int i = 0; i < _triangles.Length; i += 6)
        {
            Vector3 p0 = _vertices[_triangles[i + 2]]; // Down-Left
            Vector3 p1 = _vertices[_triangles[i]]; // Top-Left
            Vector3 p2 = _vertices[_triangles[i + 5]]; // Down-Right
            Vector3 p3 = _vertices[_triangles[i + 1]]; // Top-Right


            Vector3 q0 = .5f * (p0 + p1); // Mid-Left
            Vector3 q1 = .5f * (p2 + p3); // Mid-Right
            Vector3 q2 = .5f * (p1 + p3); // Mid-Top
            Vector3 q3 = .5f * (p0 + p2); // Mid-Down 

            Vector3 q4 = .25f * (p0 + p1 + p2 + p3); // Mid


            int j0, j1, j2, j3, j4;
            if (verticesAux.Contains(q0))
            {
                j0 = verticesAux.IndexOf(q0);
            }
            else
            {
                verticesAux.Add(q0);
                j0 = verticesAux.Count - 1;
            }

            if (verticesAux.Contains(q1))
            {
                j1 = verticesAux.IndexOf(q1);
            }
            else
            {
                verticesAux.Add(q1);
                j1 = verticesAux.Count - 1;
            }

            if (verticesAux.Contains(q2))
            {
                j2 = verticesAux.IndexOf(q2);
            }
            else
            {
                verticesAux.Add(q2);
                j2 = verticesAux.Count - 1;
            }

            if (verticesAux.Contains(q3))
            {
                j3 = verticesAux.IndexOf(q3);
            }
            else
            {
                verticesAux.Add(q3);
                j3 = verticesAux.Count - 1;
            }

            if (verticesAux.Contains(q4))
            {
                j4 = verticesAux.IndexOf(q4);
            }
            else
            {
                verticesAux.Add(q4);
                j4 = verticesAux.Count - 1;
            }

            // First square
            trianglesAux.Add(_triangles[i]);
            trianglesAux.Add(j2);
            trianglesAux.Add(j0);

            trianglesAux.Add(j0);
            trianglesAux.Add(j2);
            trianglesAux.Add(j4);

            // Second square
            trianglesAux.Add(j2);
            trianglesAux.Add(_triangles[i + 1]);
            trianglesAux.Add(j4);

            trianglesAux.Add(j4);
            trianglesAux.Add(_triangles[i + 1]);
            trianglesAux.Add(j1);

            // Third square
            trianglesAux.Add(j4);
            trianglesAux.Add(j1);
            trianglesAux.Add(j3);

            trianglesAux.Add(j3);
            trianglesAux.Add(j1);
            trianglesAux.Add(_triangles[i + 5]);

            // Forth Square
            trianglesAux.Add(j0);
            trianglesAux.Add(j4);
            trianglesAux.Add(_triangles[i + 3]);

            trianglesAux.Add(_triangles[i + 3]);
            trianglesAux.Add(j4);
            trianglesAux.Add(j3);
        }

        _triangles = new int[trianglesAux.Count];
        _vertices = new Vector3[verticesAux.Count];

        _triangles = trianglesAux.ToArray();
        _vertices = verticesAux.ToArray();

        // Recalculate xSize and zSize
        _xSize = _xSize * 2;
        _zSise = _zSise * 2;
    }

    public void ToOriginalMesh()
    {
        _triangles = new int[_originalTriangles.Length];
        _vertices = new Vector3[_originalVertices.Length];
        Array.Copy(_originalTriangles, _triangles, _originalTriangles.Length);
        Array.Copy(_originalVertices, _vertices, _originalVertices.Length);
        _zSise = _originalZSize;
        _xSize = _originalXSize;
    }

    private Vector3[] GetCornersVector()
    {
        Vector3 topRight = _vertices[0];
        Vector3 botLeft = _vertices[0];
        Vector3 topLeft = _vertices[0];
        Vector3 botRight = _vertices[0];

        for (int i = 1; i < _vertices.Length; i++)
        {
            if (_vertices[i].x >= topRight.x && _vertices[i].z >= topRight.z)
                topRight = _vertices[i];
            if (_vertices[i].x <= botLeft.x && _vertices[i].z <= botLeft.z)
                botLeft = _vertices[i];
            if (_vertices[i].x >= topLeft.x && _vertices[i].z <= topLeft.z)
                topLeft = _vertices[i];
            if (_vertices[i].x <= botRight.x && _vertices[i].z >= botRight.z)
                botRight = _vertices[i];
        }

        var position = transform.position;
        Vector3[] r = {position + topRight, position + topLeft, position + botRight, position + botLeft};
        return r;
    }

    public void PutAllEdgeVertexToZero()
    {
        float TOLERANCE = 0.001f;
        Vector3[] r = GetCornersVector();

        float topMoreDistant = r[0].x;
        float botMoreDistant = r[3].x;
        float leftMoreDistant = r[3].z;
        float rightMoreDistant = r[0].z;

        for (int i = 0; i < _vertices.Length; i++)
        {
            if (Math.Abs(_vertices[i].x - topMoreDistant) < TOLERANCE)
            {
                _vertices[i] = new Vector3(_vertices[i].x, 0, _vertices[i].z);
            }

            if (Math.Abs(_vertices[i].x - botMoreDistant) < TOLERANCE)
            {
                _vertices[i] = new Vector3(_vertices[i].x, 0, _vertices[i].z);
            }

            if (Math.Abs(_vertices[i].z - leftMoreDistant) < TOLERANCE)
            {
                _vertices[i] = new Vector3(_vertices[i].x, 0, _vertices[i].z);
            }

            if (Math.Abs(_vertices[i].z - rightMoreDistant) < TOLERANCE)
            {
                _vertices[i] = new Vector3(_vertices[i].x, 0, _vertices[i].z);
            }
        }
    }

    public void PerlinNoise(float perlinNoiseMultiplier, float baseHeight, bool bOctaves)
    {
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (!bOctaves)
                verticesAux[i] = ApplyPerlinNoise(_vertices[i].x, _vertices[i].z, _vertices[i].y, perlinNoiseMultiplier,
                    baseHeight);
            else
                verticesAux[i] = ApplyOctaves(_vertices[i].x, _vertices[i].z, _vertices[i].y, perlinNoiseMultiplier);
        }

        verticesAux = RemoveMeanHeight(verticesAux);
        _vertices = verticesAux;
    }

    public void PerlinNoise(float perlinNoiseMultiplier, float baseHeight, float minHeight, float maxHeight,
        bool bOctaves)
    {
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (_vertices[i].y > minHeight && _vertices[i].y < maxHeight)
            {
                if (!bOctaves)
                    verticesAux[i] = ApplyPerlinNoise(_vertices[i].x, _vertices[i].z, _vertices[i].y,
                        perlinNoiseMultiplier,
                        baseHeight);
                else
                    verticesAux[i] = ApplyOctaves(_vertices[i].x, _vertices[i].z, _vertices[i].y,
                        perlinNoiseMultiplier);
            }
            else
            {
                verticesAux[i] = _vertices[i];
            }
        }

        verticesAux = RemoveMeanHeight(verticesAux);
        _vertices = verticesAux;
    }

    public void PerlinNoise(float perlinNoiseMultiplier, float baseHeight, Vector3[] vertices, bool bOctaves)
    {
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (vertices.Contains(_vertices[i]))
            {
                if (!bOctaves)
                    verticesAux[i] = ApplyPerlinNoise(_vertices[i].x, _vertices[i].z, _vertices[i].y,
                        perlinNoiseMultiplier,
                        baseHeight);
                else
                    verticesAux[i] = ApplyOctaves(_vertices[i].x, _vertices[i].z, _vertices[i].y,
                        perlinNoiseMultiplier);
            }
            else
            {
                verticesAux[i] = _vertices[i];
            }
        }

        verticesAux = RemoveMeanHeight(verticesAux);
        _vertices = verticesAux;
    }

    private float GetMeanHeight(Vector3[] vertices)
    {
        float mean = 0;
        int count = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y != 0)
            {
                mean += vertices[i].y;
                count += 1;
            }
        }

        mean /= count;
        return mean;
    }

    private Vector3[] RemoveMeanHeight(Vector3[] vertices)
    {
        Vector3[] verticesAux = new Vector3[vertices.Length];
        float mean = GetMeanHeight(vertices);
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesAux[i] = new Vector3(vertices[i].x, vertices[i].y - mean, vertices[i].z);
        }

        return verticesAux;
    }

    public void UpdateMeshCollider()
    {
        MeshCollider meshc = GetComponent<MeshCollider>();
        if (meshc != null)
        {
            DestroyImmediate(meshc);
        }

        meshc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        meshc.sharedMesh = _mesh;
    }

    public Vector3[] VertexInsideBox(GameObject box)
    {
        if (box == null)
        {
            Debug.LogError("Error: Box not generated. First hit the button Generate Cube");
            return null;
        }

        Bounds bounds = box.GetComponent<Collider>().bounds;
        List<Vector3> vInsideBox = new List<Vector3>();
        foreach (var v in _vertices)
        {
            if (bounds.Contains(v))
            {
                vInsideBox.Add(v);
            }
        }

        Vector3 center = bounds.center;
        List<Vector3> vInsideObject = new List<Vector3>();

        RaycastHit rc1, rc2;

        foreach (var v in vInsideBox)
        {
            bool hit1 = Physics.Raycast(v, center.normalized, out rc1, Mathf.Infinity);
            bool hit2 = Physics.Raycast(v, -center.normalized, out rc2, Mathf.Infinity);
            if (hit1 && hit2 && rc1.collider.CompareTag("Collision") && rc2.collider.CompareTag("Collision"))
            {
                vInsideObject.Add(v);
            }
        }

        return vInsideObject.ToArray();
    }

    public void ApplySmoothInsideBox(GameObject go, float p)
    {
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        Vector3[] vertex = VertexInsideBox(go);
        float mean = GetMeanHeight(vertex);

        for (int i = 0; i < _vertices.Length; i++)
        {
            if (vertex.Contains(_vertices[i]))
            {
                float height;
                height = _vertices[i].y * p + (1 - p) * mean;
                verticesAux[i] = new Vector3(_vertices[i].x, height, _vertices[i].z);
            }

            else
            {
                verticesAux[i] = _vertices[i];
            }
        }

        _vertices = verticesAux;
    }

    public void PushMesh(GameObject box)
    {
        RaycastHit hit;
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        Vector3[] vInside = VertexInsideBox(box);
        Vector3 center = box.GetComponent<Collider>().bounds.center;
        ;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Vector3 d = Vector3.Normalize(_vertices[i] - center);
            if (vInside.Contains(_vertices[i]) && Physics.Raycast(center, d,
                    out hit, Mathf.Infinity))
                verticesAux[i] = hit.point;
            else
                verticesAux[i] = _vertices[i];
        }
        _vertices = verticesAux;
    }

    public void PushMeshWithDirection(GameObject box, Vector3 direction)
    {
        RaycastHit hit;
        Vector3[] verticesAux = new Vector3[_vertices.Length];
        Vector3[] vInside = VertexInsideBox(box);
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (vInside.Contains(_vertices[i]) && Physics.Raycast(_vertices[i], direction,
                    out hit, Mathf.Infinity))
                verticesAux[i] = hit.point;
            else
                verticesAux[i] = _vertices[i];
        }
        _vertices = verticesAux;
    }


    private Vector3 ApplyPerlinNoise(float x, float z, float y, float perlinNoiseMultiplier, float baseHeight)
    {
        float r = (Mathf.PerlinNoise(x * perlinNoiseMultiplier, z * perlinNoiseMultiplier) + 1) *
                  (y + baseHeight);
        return new Vector3(x, r, z);
    }

    private Vector3 ApplyOctaves(float x, float z, float y, float perlinNoiseMultiplier)
    {
        float r = 0;
        for (int i = 0; i < octavesF.Length; i++)
        {
            r += octavesA[i] * Mathf.PerlinNoise(octavesF[i] * x * perlinNoiseMultiplier,
                octavesF[i] * z * perlinNoiseMultiplier);
        }

        r += y;
        return new Vector3(x, r, z);
    }

    public void FinishMesh()
    {
        UpdateMeshCollider();
        GUI.enabled = false;
        DestroyImmediate(this);
        GUI.enabled = true;
    }


// Auxiliar functions for debugging dont take a look
    public void DrawVertex(Vector3 point)
    {
        _vertex = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _vertex.transform.localScale = new Vector3(.5f, .5f, .5f);
        _vertex.transform.position += point;
    }

    public void RemoveVertex()
    {
        DestroyImmediate(_vertex);
        _vertex = null;
    }
}