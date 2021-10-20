using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilteredARObject : MonoBehaviour
{
    public Transform arObject;
    OneEuroFilter<Vector3> posFilter;
    OneEuroFilter<Quaternion> rotFilter;
    public float filterFrequency = 120.0f;


    private void Start()
    {
        posFilter = new OneEuroFilter<Vector3>(filterFrequency);
        rotFilter = new OneEuroFilter<Quaternion>(filterFrequency);

    }

    private void Update()
    {
        Vector3 filteredPos = posFilter.Filter(arObject.position);
        Quaternion filteredRot = rotFilter.Filter(arObject.rotation);
        transform.position = filteredPos;
        transform.rotation = filteredRot;
    }
}
