using UnityEngine;

public class PathContainer : MonoBehaviour
{
    public enum PathType { Coins, Obstacles }

    public PathType pathType = PathType.Coins;

    public Transform startPoint;
    public Transform endPoint;
}