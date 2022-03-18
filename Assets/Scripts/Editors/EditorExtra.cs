using UnityEditor;
using UnityEngine;

namespace Editors
{
    [CustomEditor(typeof(Extras))]
    public class EditorExtra : Editor
    {
        private GameObject _objWorld;
        public override void OnInspectorGUI()
        {
            Extras extras = (Extras) target;
            DrawDefaultInspector();
            
            if (GUILayout.Button(new GUIContent("Preprocess",
                    "Preprocess, must push the button to get all the necessary for this script to work")))
            {
                extras.Preprocess();
            }
            
            _objWorld =
                (GameObject) EditorGUILayout.ObjectField("ObjWorld", _objWorld, typeof(GameObject), true);
            
            if (GUILayout.Button(new GUIContent("Cube Word")))
            {
                extras.CubeWord(_objWorld);
            }
        }

    }
}