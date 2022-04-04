using UnityEditor;
using UnityEngine;

namespace Editors
{
    [CustomEditor(typeof(AddObjects))]
    public class EditorAddObjects : Editor
    {
        private bool _drawCircle, _rotateAsTerrain, _avoidDoubleCollision, _rdmRotate, _applyBetweenHeights;
        private int _nObjects, _emptyMode;
        private GameObject _objectToSpawn, _emptyWithObjects;
        private float _perlinNoiseMultiplier, _limiterValue, _rsMin, _rsMax, _minHeight, _maxHeight;

        float radiosCircle;
        Color colorCircle = new Color(1, 0.8588f, 0.45098f, 0.5f);

        public override void OnInspectorGUI()
        {
            AddObjects addObjects = (AddObjects) target;
            DrawDefaultInspector();

            if (GUILayout.Button(new GUIContent("Preprocess",
                "Preprocess, must push the button to get all the necessary for this script to work")))
            {
                addObjects.Preprocess();
            }

            _objectToSpawn =
                (GameObject) EditorGUILayout.ObjectField("Object to spawn", _objectToSpawn, typeof(GameObject), true);

            _rotateAsTerrain = EditorGUILayout.Toggle("Rotate as terrain", _rotateAsTerrain);
            _avoidDoubleCollision = EditorGUILayout.Toggle("Avoid double collision", _avoidDoubleCollision);

            EditorGUI.BeginChangeCheck();
            _applyBetweenHeights = EditorGUILayout.Toggle("Add objects certain height", _applyBetweenHeights);
            EditorGUI.EndChangeCheck();

            _nObjects = EditorGUILayout.IntField(
                new GUIContent("Number of rays", "The number of elements that are gonna be spawned on the map"),
                _nObjects);

            EditorGUILayout.LabelField("• Random scale object", EditorStyles.miniBoldLabel);
            _rsMin = EditorGUILayout.FloatField("Range scale min", _rsMin);
            _rsMax = EditorGUILayout.FloatField("Range scale max", _rsMax);
            EditorGUILayout.LabelField("• Random rotate along Y-Axis", EditorStyles.miniBoldLabel);
            _rdmRotate = EditorGUILayout.Toggle("Rdm rotate", _rdmRotate);

            EditorGUI.BeginChangeCheck();
            _minHeight = EditorGUILayout.FloatField("Min Height to add objects", _minHeight);
            _maxHeight = EditorGUILayout.FloatField("Max Height to add objects", _maxHeight);
            EditorGUI.EndChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add Objects Randomly", EditorStyles.boldLabel);


            if (GUILayout.Button(new GUIContent("Apply hits",
                "Raycast multiple hits and add the objects to me map, according to the parameters passed")))
            {
                float[] heights = new[] {_minHeight, _maxHeight};
                addObjects.MultipleRaycast(_nObjects, _objectToSpawn, _rotateAsTerrain, _avoidDoubleCollision,
                    CheckIfRangeScale(_rsMin, _rsMax), _rdmRotate, heights);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add objects with Perlin Noise", EditorStyles.boldLabel);

            _perlinNoiseMultiplier = EditorGUILayout.Slider(_perlinNoiseMultiplier, 0.01f, 0.99f);
            _limiterValue = EditorGUILayout.FloatField("Limiter Value", _limiterValue);

            if (GUILayout.Button(new GUIContent("Add objects PN",
                "Raycast multiple hits and add the objects to me map, according to the parameters passed")))
            {
                addObjects.AddObjectsPN(_limiterValue, _perlinNoiseMultiplier, _objectToSpawn, _rotateAsTerrain,
                    _nObjects, _avoidDoubleCollision, CheckIfRangeScale(_rsMin, _rsMax), _rdmRotate);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Empty Child", EditorStyles.boldLabel);
            _emptyWithObjects =
                (GameObject) EditorGUILayout.ObjectField("EmptyObject", _emptyWithObjects, typeof(GameObject), true);

            _emptyMode = EditorGUILayout.IntField(
                new GUIContent("Mode of adding objects", "Mode 0 is randomly, mode 1 is perlin noise"),
                _emptyMode);
            if (GUILayout.Button(new GUIContent("Add Empty Child")))
            {
                float[] heights = new[] {_minHeight, _maxHeight};
                
                addObjects.AddObjectsFromEmpty(_emptyMode, _emptyWithObjects, _nObjects, _rotateAsTerrain,
                    _avoidDoubleCollision,
                    _limiterValue, _perlinNoiseMultiplier, CheckIfRangeScale(_rsMin, _rsMax), _rdmRotate, heights);
            }


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mouse brush", EditorStyles.boldLabel);
            radiosCircle = EditorGUILayout.FloatField("Radios of Circle", radiosCircle);
            _drawCircle = EditorGUILayout.Toggle("Draw Circle", _drawCircle);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (_drawCircle)
            {
                Vector3 mousePos = Event.current.mousePosition;
                mousePos.y = sv.camera.pixelHeight - mousePos.y;
                Ray ray = sv.camera.ScreenPointToRay(mousePos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Handles.color = colorCircle;

                    Handles.DrawSolidDisc(hit.point, hit.normal, radiosCircle);
                    sv.Repaint();
                }
            }

            if (_applyBetweenHeights)
            {
                DrawLinesHeight(Color.red);
            }
        }


        private float[] CheckIfRangeScale(float min, float max)
        {
            if (min == 0 && max == 0)
                return null;
            else
                return new[] {min, max};
        }


        private void DrawLinesHeight(Color color)
        {
            Handles.color = color;


            AddObjects addObjects = (AddObjects) target;
            Vector3 origen = addObjects.transform.position;

            origen = new Vector3(origen.x - 0.25f, origen.y, origen.z - 0.25f);
            Vector3 VecMaxHeight = new Vector3(origen.x, origen.y + _maxHeight, origen.z);
            Vector3 VecMinHeight = new Vector3(origen.x, origen.y + _minHeight, origen.z);
            Debug.Log(_maxHeight);
            Handles.DrawLine(VecMinHeight, VecMaxHeight, 2);
            Handles.color = Color.red;
            Debug.Log(_maxHeight - _minHeight);
            Handles.Label(Vector3.Lerp(VecMaxHeight, VecMinHeight, 0.5f), "Range:" + (_maxHeight - _minHeight));
        }
    }
}