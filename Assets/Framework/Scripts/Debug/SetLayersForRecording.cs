using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLayersForRecording : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        Camera camera = gameObject.GetComponent<Camera>();
        camera.cullingMask = camera.cullingMask | (1 << LayerMask.NameToLayer("AR"));
        camera.cullingMask = camera.cullingMask & ~(1 << LayerMask.NameToLayer("DualCamera (Right)"));
    }
}
