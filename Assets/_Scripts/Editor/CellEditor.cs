using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Cell))]
public class CellEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var myScript = (Cell) target;
        var allCells = (Cell[]) FindObjectsOfType(typeof(Cell));
        if (GUILayout.Button("Toggle Visibility"))
        {
            myScript.Hide();
        }
        if (GUILayout.Button("Toggle All Cells' Visibility"))
        {
            foreach (var cell in allCells)
            {
                cell.Hide();
            }
        }
    }
}
