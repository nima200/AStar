using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Hexagon))]
public class CellEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var myScript = (Hexagon) target;
        var allCells = (Hexagon[]) FindObjectsOfType(typeof(Hexagon));
        if (GUILayout.Button("Toggle Visibility"))
        {
            myScript.Hide();
        }
        if (GUILayout.Button("Toggle All Hexagons' Visibility"))
        {
            foreach (var cell in allCells)
            {
                cell.Hide();
            }
        }
    }
}
