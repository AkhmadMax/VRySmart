using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    public Transform rig;

    static Vector3 offsetPos;
    static Quaternion offsetRot;

    static Vector3 arPosAtTargetA;
    static Vector3 arucoPosAtTargetA;

    private void OnTriggerEnter(Collider other)
    {
        Transform arTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Transform arucoTransform = GameObject.FindGameObjectWithTag("Marker").transform;

        // Reconstruct positional offset
        if (other.tag == "Marker" && gameObject.name == "TargetA")
        {
            arPosAtTargetA = arTransform.position;
            arucoPosAtTargetA = arucoTransform.position;
            offsetPos = arTransform.position - arucoTransform.position;
        }

        // Reconstruct rotation offset
        if (other.tag == "Marker" && gameObject.name == "TargetB")
        {
            Vector3 arVector = arTransform.position - arPosAtTargetA;
            Vector3 arucoVector = arucoTransform.position - arucoPosAtTargetA;
            arucoVector.y = 0;
            arVector.y = 0;
            Quaternion offsetRot = Quaternion.FromToRotation(arVector, arucoVector);

            rig.rotation *= Quaternion.Inverse(offsetRot);
            rig.position += offsetPos;

        }
    }
}
