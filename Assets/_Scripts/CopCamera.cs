using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * CopCamera inherits from CameraFOV
 * The only modification is what it does upon finding a target
 */

public class CopCamera : CameraFOV {

	// overrides the method to stop the robber when the officer sees him
	override public void FindVisibleTargets() {
		if (!DetectedRobber)
		{
			visibleTargets.Clear ();
			Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++) {
				Transform target = targetsInViewRadius [i].transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
					float dstToTarget = Vector3.Distance (transform.position, target.position);
					if (!Physics.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask))
					{
						DetectedRobber = true;
						visibleTargets.Add (target);
						target.GetComponent<Agent>().StopPath();
					}
				}
			}
		}
	}
}