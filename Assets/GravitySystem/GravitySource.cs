using PurrNet;
using UnityEngine;

public class GravitySource : NetworkIdentity
{
    public int mass;
}

public struct GravityConstant
{
    public const float G = 6.674f;
}