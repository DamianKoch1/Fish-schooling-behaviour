using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FlockingSettings : ScriptableObject
{
    public LayerMask obstacleLayer;

    [Range(0, 10)]
    public float alignStrength = 1;

    [Range(0, 10)]
    public float separateStrength = 1;

    [Range(0, 10)]
    public float cohereStrength = 1;

    [Range(0, 20)]
    public float avoidanceStrength;

    public float maxSteerForce;
}
