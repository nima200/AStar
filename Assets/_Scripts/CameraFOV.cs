using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

/*
 * CameraFOV 
 * Implements all Field of View (FOV) computation / optimization
 * 
 * This allows for a source (i.e., camera or cop) to detect if a GameObject with a particular mask layer
 * is in its FOV. 
 * It can, thereafter, use its information to implement in-game functionalities such as
 * communicating the target's position to an agent or request the target to perform actions.
 */

public class CameraFOV : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;

   	public bool DetectedRobber;
   	public Transform Bank;
    	public Agent Cop;
    	public Agent Robber;
    	public List<Vector3> RobberPath;
    	public List<Path> PossiblePaths = new List<Path>();
    	public Dictionary<Path, Pair<int, int>> PathMap = new Dictionary<Path, Pair<int, int>>();
	public LayerMask targetMask;
	public LayerMask obstacleMask;
    	public bool FoundCutOff;
    	public int pathIndex;


	public List<Transform> visibleTargets = new List<Transform>();

	public float meshResolution;
	public int edgeResolveIterations;
	public float edgeDstThreshold;

	public MeshFilter viewMeshFilter;
	Mesh viewMesh;

	// Creates the mesh for the camera FOV and searches for targets every fifth of a second (0.2f)
	void Start()
	{
	    Bank = GameObject.Find("Bank").transform;
		viewMesh = new Mesh ();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;

		StartCoroutine ("FindTargetsWithDelay", 0.2f);
	}


	// Looks for a target with a delay as input (e.g., every x seconds)
	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}


	// After all update functions have finished, calls to draw the camera FOV
	void LateUpdate() {
		DrawFieldOfView ();
  	}


	// Adds visible targets to a list (so that multiple robbers can be implemented in the project)
	public virtual void FindVisibleTargets() {
	    if (!DetectedRobber)
	    {
	        visibleTargets.Clear ();
	        Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

	        for (int i = 0; i < targetsInViewRadius.Length; i++) {
	            Transform target = targetsInViewRadius [i].transform;
	            Vector3 dirToTarget = (target.position - transform.position).normalized;
	            if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
	                float dstToTarget = Vector3.Distance (transform.position, target.position);
	                if (!Physics.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask) && !Robber.IsCaught)
	                {
	                    DetectedRobber = true;
	                    Robber.IsCaught = true;
	                    visibleTargets.Add (target);
	                    if (!FoundCutOff)
	                    {
	                        PathRequestManager.RequestPath(new PathRequest(target.position, Bank.position, OnDetectRobberPath));
	                    }
	                }
	            }
	        }
	    }
	}


	// Orders the waypoints of the robber's path by distance from the cop
	// and request pathfinding from the cop to the closest waypoint
    	private void OnDetectRobberPath(Path path, bool success)
    	{
        	RobberPath = path.Waypoints.OrderBy(p => Mathf.RoundToInt(Vector3.Distance(p, Cop.transform.position))).ToList();
        	PathRequestManager.RequestPath(new PathRequest(Cop.transform.position, RobberPath[pathIndex], CalculateTime));
    	}


	// Attempts to find the robber's path waypoint that is the first one to be reachable by the police officer before the robber
    	private void FindCutoff()
    	{
        	if (FoundCutOff) return;
		// try to find the cut-off and request pathfinding for the cop to get there
        	try
        	{
            		FoundCutOff = true;
            		var path = PathMap.First(p => p.Value.A < p.Value.B);
            		Cop.StartPath(path.Key);
        	}
        	catch (InvalidOperationException)
        	{
            		FoundCutOff = false;
            		pathIndex++;
            		PathRequestManager.RequestPath(new PathRequest(Cop.transform.position, RobberPath[pathIndex], CalculateTime));
        	}
    	}

    	private void CalculateTime(Path path, bool success)
    	{
        	int copTime = path.Waypoints.Length;
        	int robberTime = Robber.Path.DistanceOf(Robber.CurrentWaypoint, path.Waypoints[path.Waypoints.Length - 1]);
        	PathMap.Add(path, new Pair<int, int>(copTime, robberTime));
        	FindCutoff();
    	}


	// Draws the camera FOV and manages end of edges collisions from raycast
	// (i.e., sharp delimitation from edge end points)
	void DrawFieldOfView() {
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3> ();
		ViewCastInfo oldViewCast = new ViewCastInfo ();
		for (int i = 0; i <= stepCount; i++) {
			float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast (angle);

			if (i > 0) {
				bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
					EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
					if (edge.pointA != Vector3.zero) {
						viewPoints.Add (edge.pointA);
					}
					if (edge.pointB != Vector3.zero) {
						viewPoints.Add (edge.pointB);
					}
				}

			}


			viewPoints.Add (newViewCast.point);
			oldViewCast = newViewCast;
		}

		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]);

			if (i < vertexCount - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}

		viewMesh.Clear ();

		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals ();
	}


	// Returns the EdgeInfo from two ViewCastInfo objects
	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast (angle);

			bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo (minPoint, maxPoint);
	}


	// Returns a struct ViewCastInfo from an angle
	// The ViewCastInfo will either represent a hit with some obstacle or be on the edge of the FOV circle at the given angle
	ViewCastInfo ViewCast(float globalAngle) {
		Vector3 dir = DirFromAngle (globalAngle, true);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
			return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
		} else {
			return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
		}
	}


	// Returns a Vector3 of an angle from the gameObject's position
	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
		// corrects the angle if it is not global (to make it global)
		if (!angleIsGlobal) {
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}


	// Records information on a raycast
	public struct ViewCastInfo {
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;

		public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}


	// Records information on an edge (such as one hit by a raycast)
	public struct EdgeInfo {
		public Vector3 pointA;
		public Vector3 pointB;

		public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
			pointA = _pointA;
			pointB = _pointB;
		}
	}

}