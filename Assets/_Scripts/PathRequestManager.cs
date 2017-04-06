using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PathRequestManager : MonoBehaviour {

    private static PathRequestManager _instance;
    private AStar _pathFinder;
    private readonly Queue<PathResult> _results = new Queue<PathResult>();

    private void Awake()
    {
        _instance = this;
        _pathFinder = GetComponent<AStar>();
    }

    private void Update()
    {
        if (_results.Count <= 0) return;
        int itemsInQueue = _results.Count;
        lock (_results)
        {
            for (int i = 0; i < itemsInQueue; i++)
            {
                var result = _results.Dequeue();
                result.CallBack(result.Path, result.SUCCESS);
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            _instance._pathFinder.FindPath(request, _instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (_results)
        {
            _results.Enqueue(result);
        }
    }
}