using System;

public struct PathResult
{
    public Path Path;
    public bool SUCCESS;
    public Action<Path, bool> CallBack;

    public PathResult(Path path, bool success, Action<Path, bool> callBack)
    {
        Path = path;
        SUCCESS = success;
        CallBack = callBack;
    }
}