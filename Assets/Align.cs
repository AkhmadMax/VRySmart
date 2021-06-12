using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align : MonoBehaviour
{
    public Transform origin;
    public Transform directionTransform;

    public bool align;

    private void Update()
    {
        if(align)
        {
            transform.position = origin.position - transform.position;

            Vector3 direction = directionTransform.position - origin.position;
            Quaternion rotation = Quaternion.FromToRotation(transform.right, direction);
            transform.rotation = rotation;
        }
    }
}
