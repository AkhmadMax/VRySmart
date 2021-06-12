using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

public class SimplePlayer : NetworkBehaviour
{
    AnchorCreator anchorCreator;
    NetworkTransform networkTransform;
    ARCameraManager arCamera;

    [Client]
    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
        anchorCreator = FindObjectOfType<AnchorCreator>();
        arCamera = FindObjectOfType<ARCameraManager>();

        networkTransform.sendInterval = 0.01f;
        if (Application.platform == RuntimePlatform.Android)
        {
            gameObject.AddComponent<ARPoseDriver>();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach(Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }

    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (anchorCreator.lastAnchor)
            {
                transform.position = arCamera.transform.position - anchorCreator.lastAnchor.transform.position;
            }
            else
            {
                transform.position = Vector3.zero;
            }
        }
    }
}
