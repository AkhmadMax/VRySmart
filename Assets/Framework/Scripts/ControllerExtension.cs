using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.CloudAnchors;

public class ControllerExtension : MonoBehaviour
{
    public GameObject markerScreen;
    public MessagesHandler messagesHandler;
    public CloudAnchorsExampleController originalController;

    private void Awake()
    {
        messagesHandler.OnCommandEvent += MessagesHandler_OnCommandEvent;
    }
    private void OnDestroy()
    {
        messagesHandler.OnCommandEvent -= MessagesHandler_OnCommandEvent;
    }

    private void MessagesHandler_OnCommandEvent(int obj)
    {
        ToggleMarkerScreen();
    }

    private void ToggleMarkerScreen()
    {
        markerScreen.SetActive(!markerScreen.activeInHierarchy);
    }
}
