using UnityEditor;
using UnityEngine;

namespace Editors
{
    [CustomEditor(typeof(GridTerrain))]
    public class EditorGridTerrain : Editor
    {
        private float _perlinNoiseMultiplier;
        private float _baseHeight;
        private int _indexSliderPM;
        private float _minHeight, _maxHeight, _smoothValue;
        private bool _drawLineHeight, _bOctaves;
        private GameObject _objectInsideNoise, _objectInsideSmooth, _objectToPush;
        private Vector3 _directionPush;


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridTerrain gridTerrain = (GridTerrain) target;

            // Subidivsion Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mesh Subdivision", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Subdivide Mesh",
                    "Button that is gonna subdivide the mesh once. If you press the button multiple times it will be subdivided more and more.")))
            {
                gridTerrain.Subdivide();
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            // Noise Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Perlin Noise", EditorStyles.boldLabel);
            _baseHeight = EditorGUILayout.FloatField(new GUIContent("Base height", "The height of the Perlin noise"),
                _baseHeight);
            EditorGUILayout.LabelField("Perlin Noise Multiplier", EditorStyles.label);
            _perlinNoiseMultiplier = EditorGUILayout.Slider(_perlinNoiseMultiplier, 0.01f, 0.99f);
            _bOctaves = EditorGUILayout.Toggle("ApplyOctaves", _bOctaves);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("• Apply Noise on all Mesh", EditorStyles.miniBoldLabel);

            if (GUILayout.Button(new GUIContent("Apply PN general",
                    "Apply the Perlin Noise, you have to take in consideration that that the Perlin Noise is based on the height and the value of the slider, if the height is zero the first time it wont take in consideration the \"ground\"")))
            {
                gridTerrain.PerlinNoise(_perlinNoiseMultiplier, _baseHeight, _bOctaves);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            // Draw Line of the height

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("• Apply Noise on Certain Height", EditorStyles.miniBoldLabel);
            _minHeight = EditorGUILayout.FloatField(
                new GUIContent("Min Height", "Min Height to start applying the noise"),
                _minHeight);
            _maxHeight = EditorGUILayout.FloatField(
                new GUIContent("Max Height", "Max Height to start applying the noise"),
                _maxHeight);
            _drawLineHeight =
                EditorGUILayout.Toggle(new GUIContent("Draw Lines Height", "Toggle to draw the lines of the height"),
                    _drawLineHeight);

            if (GUILayout.Button(new GUIContent("Apply PN on range",
                    "Apply the Perlin Noise, you have to take in consideration that that the Perlin Noise is based on the height and the value of the slider. This noise will be applied only on the vertex that are between the heights selected")))
            {
                gridTerrain.PerlinNoise(_perlinNoiseMultiplier, _baseHeight, _minHeight, _maxHeight, _bOctaves);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("• Apply Noise on inside Cube", EditorStyles.miniBoldLabel);
            _objectInsideNoise =
                (GameObject) EditorGUILayout.ObjectField("Object to apply PN inside", _objectInsideNoise,
                    typeof(GameObject));
            if (GUILayout.Button("Apply noise inside cube"))
            {
                Vector3[] vertices = gridTerrain.VertexInsideBox(_objectInsideNoise);
                gridTerrain.PerlinNoise(_perlinNoiseMultiplier, _baseHeight, vertices, _bOctaves);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }


            // Exterior Vertex To Zero
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Borders vertex to zero", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Apply borders to zero", "Put all the borders height to zero")))
            {
                gridTerrain.PutAllEdgeVertexToZero();
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            // Previous meshes
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Previous meshes", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Go back to original mesh",
                    "Recovers the original mesh, the mesh generated by the mesh generator")))
            {
                gridTerrain.ToOriginalMesh();
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Slide between previous meshes", EditorStyles.miniBoldLabel);
            _indexSliderPM = EditorGUILayout.IntSlider(_indexSliderPM, 0, gridTerrain.previousMeshes.Count - 1);
            if (EditorGUI.EndChangeCheck())
            {
                gridTerrain.ChangeToPreviousMesh(_indexSliderPM);
            }

            // Collider Section
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mesh Collider", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent("Update mesh collider",
                    "If the mesh doesn't have a mesh collider it will create one, if it does have one, will be updated to the last modification")))
            {
                gridTerrain.UpdateMeshCollider();
            }

            // Smooth Section

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Smoothness", EditorStyles.boldLabel);
            _objectInsideSmooth =
                (GameObject) EditorGUILayout.ObjectField("Object to apply smooth inside", _objectInsideSmooth,
                    typeof(GameObject));
            _smoothValue = EditorGUILayout.Slider(_smoothValue, 0, 1);
            if (GUILayout.Button(new GUIContent("Apply smoothness inside area")))
            {
                gridTerrain.ApplySmoothInsideBox(_objectInsideSmooth, _smoothValue);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            // Push Section

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Push", EditorStyles.boldLabel);
            _objectToPush =
                (GameObject) EditorGUILayout.ObjectField("Object to Push", _objectToPush,
                    typeof(GameObject));
            _directionPush = EditorGUILayout.Vector3Field("Direction to push", _directionPush);
            if (GUILayout.Button(new GUIContent("Push")))
            {
                gridTerrain.PushMesh(_objectToPush);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }

            if (GUILayout.Button(new GUIContent("Push with direction")))
            {
                gridTerrain.PushMeshWithDirection(_objectToPush, _directionPush);
                gridTerrain.UpdateMesh();
                gridTerrain.UpdateLastXMesh();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Finish Mesh", EditorStyles.boldLabel);
            // Finish Mesh
            if (GUILayout.Button(new GUIContent("Finish mesh")))
            {
                gridTerrain.FinishMesh();
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void DrawLinesHeight(Color color)
        {
            Handles.color = color;
            GridTerrain gridTerrain = (GridTerrain) target;
            Vector3 origen = gridTerrain.transform.position;
            origen = new Vector3(origen.x - 0.25f, origen.y, origen.z - 0.25f);
            Vector3 VecMaxHeight = new Vector3(origen.x, origen.y + _maxHeight, origen.z);
            Vector3 VecMinHeight = new Vector3(origen.x, origen.y + _minHeight, origen.z);
            Handles.DrawLine(VecMinHeight, VecMaxHeight, 2);
            Handles.color = Color.red;
            Handles.Label(Vector3.Lerp(VecMaxHeight, VecMinHeight, 0.5f), "Range:" + (_maxHeight - _minHeight));
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (_drawLineHeight)
            {
                DrawLinesHeight(Color.red);
            }
        }
    }
}