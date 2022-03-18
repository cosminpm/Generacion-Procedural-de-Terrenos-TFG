using GridGeneratorEditor;
using UnityEditor;
using UnityEngine;

namespace Editors
{
    [CustomEditor(typeof(GridGenerator))]
    public class EditorMesh0 : Editor
    {
        private bool _hideVertices;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GridGenerator gridGenerator = (GridGenerator) target;
            _hideVertices = EditorGUILayout.Toggle(new GUIContent("Hide vertices", "Toggle to hide the vertices, they wont disappear, they just will be disabled. "), _hideVertices);
            
            if (GUILayout.Button(new GUIContent("Finish mesh",
                "Finish the mesh, and go to the next one")))
            {
                gridGenerator.finishMeshZero = true;
            }
            
            if (_hideVertices)
            {
                gridGenerator.HideVertices();
            }
            else
            {
                gridGenerator.ShowVertices();
            }

        }
    }
}
