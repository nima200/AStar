using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{

    public Path Path;
    public Transform Target;
    public float Speed = 5;
    private int _targetIndex;
    public Vector3 CurrentWaypoint;

    public void OnPathFound(Path newPath, bool pathFound)
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
        PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, OnPathFound));
    }

    private IEnumerator FollowPath()
    {
        if (Path.Waypoints.Length == 0) yield break;
        CurrentWaypoint = Path.Waypoints[0];
        while (true)
        {
            if (transform.position == CurrentWaypoint)
            {
                _targetIndex++;
                if (_targetIndex >= Path.Waypoints.Length)
                {
                    yield break;
                }
                CurrentWaypoint = Path.Waypoints[_targetIndex];
            }
            /*transform.position = Vector3.MoveTowards(transform.position, currentWayPoint,
                    Speed * Time.fixedDeltaTime);*/
            transform.position = CurrentWaypoint;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StartPath(Path p)
    {
        Path = p;
        StartCoroutine("FollowPath");
    }

    public void OnDrawGizmos()
    {
        if (Path == null) return;
        for (int i = _targetIndex; i < Path.Waypoints.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(Path.Waypoints[i], 4);
        }
    }
}
