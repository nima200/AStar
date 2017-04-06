using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopCamera : CameraFOV {

    public override void FindVisibleTargets()
    {
        visibleTargets.Clear();
        var targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (var c in targetsInViewRadius)
        {
            var target = c.transform;
            var dirToTarget = (target.position - transform.position).normalized;
            if (!(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)) continue;
            float dstToTarget = Vector3.Distance(transform.position, target.position);
            if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
            {
                visibleTargets.Add(target);
                // MATTHIEU 
            }
        }
    }
}
