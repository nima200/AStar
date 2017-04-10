using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * CopCamera inherits from CameraFOV
 * The only modification is what it does upon finding a target
 */

public class CopCamera : CameraFOV {
	// overrides the method to stop the robber when the officer sees him
	public override void FindVisibleTargets() {
		if (!DetectedRobber)
		{
			visibleTargets.Clear ();
			// Handles the actual collision with a GameObject, checking it against a targetMask
			Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++) {
				Transform target = targetsInViewRadius[i].transform;
				// Determines a direction to the target
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
					float dstToTarget = Vector3.Distance (transform.position, target.position);
					// If the raycast has not hit a wall, it must have been a robber in this case
					if (!Physics.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask))
					{
						// This is the script for a cop FOV, so he must arrest the robber upon seeing him
						DetectedRobber = true;
						visibleTargets.Add (target);
						target.GetComponent<Agent>().StopPath();
					}
				}
			}
		}
	}
}