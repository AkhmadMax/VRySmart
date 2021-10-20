using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Gear : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right
    }

    public Side side;
    public Transform parent;

    public float posOffset = 0;

    void LateUpdate()
    {
        Vector3 pos = transform.localPosition;
        if (parent)
            pos.z = parent.localPosition.z + posOffset;
        transform.localPosition = pos;

        Vector3 euler = transform.localEulerAngles;
        euler.y = transform.localPosition.z * (side == Side.Right ? -7830 : 7830);
        transform.localEulerAngles = euler;
    }
}
