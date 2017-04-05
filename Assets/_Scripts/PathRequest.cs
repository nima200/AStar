using System;
using UnityEngine;
using UnityEngine.AI;

public struct PathRequest
{
    public Vector3 PathStart;
    public Vector3 PathEnd;
    public Action<Path, bool> Callback;

    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Path, bool> callback)
    {
        PathStart = pathStart;
        PathEnd = pathEnd;
        Callback = callback;
    }
}