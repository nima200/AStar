using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{

    public Vector3[] Path;
    public Transform Target;
    public float Speed = 5;
    private int _targetIndex;

    public void OnPathFound(Vector3[] newPath, bool pathFound)
    {
        if (!pathFound)
        {
            return;
        }
        Path = newPath;
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
    }

    public void StopPath()
    {
        StopCoroutine("FollowPath");
    }

    public void ResetPath()
    {
        _targetIndex = 0;
    }

    public void RequestPath(Transform target)
    {
        Target = target;
        StopPath();
        ResetPath();
        PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
    }

    private IEnumerator FollowPath()
    {
        if (Path.Length == 0) yield break;
        var currentWayPoint = Path[0];
        while (true)
        {
            if (transform.position == currentWayPoint)
            {
                _targetIndex++;
                if (_targetIndex >= Path.Length)
                {
                    yield break;
                }
                currentWayPoint = Path[_targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint,
                    Speed * Time.fixedDeltaTime);
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (Path == null) return;
        for (int i = _targetIndex; i < Path.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Path[i], 4);
        }
    }
}
