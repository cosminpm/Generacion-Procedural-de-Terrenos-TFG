using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GridGeneratorEditor
{
    [ExecuteInEditMode]
    public class GridGenerator : MonoBehaviour
    {
        [HideInInspector] public Mesh mesh;

        [HideInInspector] public Vector3[] vertices;
        [HideInInspector] public int[] triangles;

        [Range(1, 50)] public int xSize = 1;
        [Range(1, 50)] public int zSize = 1;
        [HideInInspector] public bool finishMeshZero;
        private GameObject[] _arrayOfVertexSphere;
        private bool _applyChanges;
        private Material _material;

        private void Start()
        {
            Initialize();
        }

        private void OnValidate()
        {
            if (!finishMeshZero)
                _applyChanges = true;
        }

        private void Update()
        {
            if (_applyChanges)
            {
                _applyChanges = false;
                CreateShape();
                UpdateMesh();
                DrawSpheres();
            }

            if (finishMeshZero)
            {
                HideVertices();
                RemovePreSpheres();
                GridTerrain gridTerrain = gameObject.AddComponent(typeof(GridTerrain)) as GridTerrain;
                if (gridTerrain.initializationFinished)
                {
                    GUI.enabled = false;
                    DestroyImmediate(this);
                    GUI.enabled = true;
                }
            }
            else
            {
                ChangeColorOfVertex();
                MoveVerticesHigh();
            }
        }


        // Initialize the variables and mesh needed
        void Initialize()
        {
            if (mesh == null)
            {
                mesh = new Mesh();
            }

            // Create mesh elements needed
            if (GetComponent<MeshFilter>() == null)
            {
                gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();

                GetComponent<MeshFilter>().mesh = mesh;
            }

            gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
        }


        // Creates the shape of the mesh, in this case a shape of a grid plane
        void CreateShape()
        {
            // Create the vertices, the size is always +1
            vertices = new Vector3[(xSize + 1) * (zSize + 1)];

            for (int i = 0, z = 0; z < zSize + 1; z++)
            {
                for (int x = 0; x < xSize + 1; x++)
                {
                    vertices[i] = new Vector3(x, 0, z);
                    i++;
                }
            }

            // Create the triangles
            triangles = new int[xSize * zSize * 6];
            for (int z = 0, tris = 0, vert = 0; z < zSize; z++)
            {
                for (var x = 0; x < xSize; x++)
                {
                    triangles[tris] = vert;
                    triangles[tris + 1] = vert + xSize + 1;
                    triangles[tris + 2] = vert + 1;

                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + xSize + 1;
                    triangles[tris + 5] = vert + xSize + 2;
                    vert++;
                    tris += 6;
                }

                vert++;
            }
        }

        void UpdateMesh()
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            transform.position = new Vector3(0, 0, 0);
        }

        private void RemovePreSpheres()
        {
            if (_arrayOfVertexSphere != null)
            {
                for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
                {
                    if (_arrayOfVertexSphere[i])
                    {
                        DestroyImmediate(_arrayOfVertexSphere[i].gameObject);
                        _arrayOfVertexSphere[i] = null;
                    }
                }

                _arrayOfVertexSphere = null;
            }
        }

        // Draws the cubes of the mesh
        private void DrawSpheres()
        {
            RemovePreSpheres();
            _arrayOfVertexSphere = new GameObject[vertices.Length];
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                if (_arrayOfVertexSphere[i] == null)
                {
                    _arrayOfVertexSphere[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    DestroyImmediate(_arrayOfVertexSphere[i].gameObject.GetComponent<BoxCollider>());
                    _arrayOfVertexSphere[i].name = "vertex" + i;
                    _arrayOfVertexSphere[i].transform.localScale = new Vector3(.1f, .1f, .1f);
                    _arrayOfVertexSphere[i].transform.position = transform.position + vertices[i];
                    _arrayOfVertexSphere[i].transform.parent = gameObject.transform;
                }
            }

            InitializeMaterials();
        }

        private void ChangeColorOfVertex()
        {
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                if (Selection.Contains(_arrayOfVertexSphere[i]))
                {
                    Material material = new Material(Shader.Find("Standard"));
                    material.color = Color.red;
                    _arrayOfVertexSphere[i].gameObject.GetComponent<Renderer>().material = material;
                }

                else
                {
                    Material material = new Material(Shader.Find("Standard"));
                    material.color = Color.white;
                    _arrayOfVertexSphere[i].gameObject.GetComponent<Renderer>().material = material;
                }
            }
        }

        public void HideVertices()
        {
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                _arrayOfVertexSphere[i].SetActive(false);
            }
        }

        public void ShowVertices()
        {
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                _arrayOfVertexSphere[i].SetActive(true);
            }
        }

        private void InitializeMaterials()
        {
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                {
                    _arrayOfVertexSphere[i].gameObject.GetComponent<MeshRenderer>().material =
                        new Material(Shader.Find("Standard"));
                }
            }
        }

        private void MoveVerticesHigh()
        {
            for (var i = 0; i < _arrayOfVertexSphere.Length; i++)
            {
                if (Selection.Contains(_arrayOfVertexSphere[i]))
                {
                    Vector3 vertex = new Vector3(vertices[i].x,
                        -(transform.position - _arrayOfVertexSphere[i].transform.position).y, vertices[i].z);
                    vertices[i] = vertex;
                    // POSSIBLE IMPLEMENTATION FOR LATER
                    //vertices[i] = -(transform.position - _arrayOfVertexSphere[i].transform.position);
                }
            }

            UpdateMesh();
        }
    }
}