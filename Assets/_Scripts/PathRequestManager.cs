using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {

    private readonly Queue<PathRequest> _pathRequests = new Queue<PathRequest>();
    private PathRequest _currentPathRequest;
    private static PathRequestManager _instance;
    private AStar _pathFinder;
    private bool _isProcessingPath;

    private void Awake()
    {
        _instance = this;
        _pathFinder = GetComponent<AStar>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callBack)
    {
        var newRequest = new PathRequest(pathStart, pathEnd, callBack);   
        _instance._pathRequests.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (_isProcessingPath || _pathRequests.Count == 0) return;
        _currentPathRequest = _pathRequests.Dequeue();
        _isProcessingPath = true;
        _pathFinder.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        _currentPathRequest.Callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }
}
