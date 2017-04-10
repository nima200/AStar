using UnityEngine;
using System.Collections;
using UnityEditor;

/*
 * CopFOVEditor
 * Edits the cop's camera GUI to make it visible in the scene and game modes
 */

[CustomEditor (typeof (CopCamera))]
public class CopFOVEditor : Editor {

	void OnSceneGUI() {
		CopCamera fow = (CopCamera)target;
		Handles.color = Color.white;	// This is used as the outline of the FOV's radius / angle

		// Draws the circle of the FOV it which the angle will determine what can be seen (much like a Venn diagram
		Handles.DrawWireArc (fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);

		// Draws the angle
		Vector3 viewAngleA = fow.DirFromAngle (-fow.viewAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle (fow.viewAngle / 2, false);
		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

		// Draws a red line for each of the enemies in the FOV
		Handles.color = Color.red;
		foreach (Transform visibleTarget in fow.visibleTargets) {
			Handles.DrawLine (fow.transform.position, visibleTarget.position);
		}
	}

}