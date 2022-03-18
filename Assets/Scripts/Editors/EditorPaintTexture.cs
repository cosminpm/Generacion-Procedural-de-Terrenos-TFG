using System;
using UnityEditor;
using UnityEngine;

namespace Editors
{
    [CustomEditor(typeof(PaintTexture))]
    public class EditorPaintTexture : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            PaintTexture paintTexture = (PaintTexture) target;
            
            if (GUILayout.Button(new GUIContent("Preprocess")))
            {
                paintTexture.Preprocess();
            }
            
            SerializedObject serializedGradient = new SerializedObject(paintTexture);
            SerializedProperty colorGradient = serializedGradient.FindProperty("gradient");

            // OnGUI
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(colorGradient, true, null);
            if (EditorGUI.EndChangeCheck())
            {
                serializedGradient.ApplyModifiedProperties();
                paintTexture.AssignColors();
            }
        }
    }
}