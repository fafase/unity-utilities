using UnityEngine;
using System.Collections;

public static class WormCurve  
{

    public static Vector3 CatmullRom(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
        Vector3 a = 0.5f * (2f * p2);
        Vector3 b = 0.5f * (p3 - p1);
        Vector3 c = 0.5f * (2f * p1 - 5f * p2 + 4f * p3 - p4);
        Vector3 d = 0.5f * (-p1 + 3f * p2 - 3f * p3 + p4);

        Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);

        return pos;
    }
}
