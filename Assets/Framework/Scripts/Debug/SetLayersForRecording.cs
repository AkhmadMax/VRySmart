using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLayersForRecording : MonoBehaviour
{
    public bool rightCam;

    // Start is called before the first frame update
    void Update()
    {
        Camera camera = gameObject.GetComponent<Camera>();
        camera.cullingMask = camera.cullingMask | (1 << LayerMask.NameToLayer("AR"));
        if(rightCam)
            camera.cullingMask = camera.cullingMask & ~(1 << LayerMask.NameToLayer("DualCamera (Right)"));
        else
            camera.cullingMask = camera.cullingMask & ~(1 << LayerMask.NameToLayer("DualCamera (Left)"));
    }
}
