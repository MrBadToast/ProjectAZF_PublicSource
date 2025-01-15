using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AZFUtilities : MonoBehaviour
{
    static public Vector3 F3ToVec3(float3 f3)
    {
        return new Vector3(f3.x, f3.y, f3.z);
    }
}
