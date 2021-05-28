using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VertexColorObject;

namespace VertexColorPainter
{
    [CustomEditor(typeof(VCObject))]
    public class SVTXObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Painter Window"))
                VertexColorPainterWindow.OpenPainterWindow();
        }
    }
}