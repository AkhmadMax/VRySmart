using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokaphone : MonoBehaviour
{
    GameObject marker;
    GameObject phone;

    public Transform rig;
    private Vector3 offset;

    void Update()
    {
        if (!marker)
            marker = GameObject.FindGameObjectWithTag("Marker");
        if(!phone)
            phone = GameObject.Find("Phone(Clone)");

        if (Input.GetKeyDown(KeyCode.Alpha2) && marker && phone)
        {

            Vector3 originToMarker = new Vector3(marker.transform.position.x, 0, marker.transform.position.z);
            Vector3 originToPhone = new Vector3(phone.transform.position.x, 0, phone.transform.position.z);
            Quaternion rot = Quaternion.FromToRotation(originToMarker, originToPhone);
            Debug.Log(originToMarker + " " + originToPhone + " " + rot);
            rig.rotation *= rot;
        }

        if(Input.GetKeyDown(KeyCode.Alpha3) && marker && phone)
        {
            offset = marker.transform.position - phone.transform.position;
            rig.transform.position -= offset;
        }
    }
}
