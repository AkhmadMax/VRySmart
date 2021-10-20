using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.RenderStreaming;

public class MobileUIManager : MonoBehaviour
{
    public NetworkManager manager;
    public InputField ipInputField;
    public RectTransform networkConfigWindow;
    public Text status;
    public CameraStreamingManager streamingManager;
    bool networkConfigIsHidden;

    void Awake()
    {
        ipInputField.text = manager.networkAddress;
    }

    public void Connect()
    {
        manager.networkAddress = ipInputField.text;
        manager.StartClient();
    }

    private void Update()
    {
        status.text = "";

        if(manager.IsClientConnected() == false && networkConfigIsHidden)
        {
            ShowNetworkConfigWindow();
        }

        if (manager.IsClientConnected() == true && networkConfigIsHidden == false)
        {
            HideNetworkConfigWindow();
        }

        bool noConnection = (manager.client == null || manager.client.connection == null || manager.client.connection.connectionId == -1);

        if(noConnection == false)
        {
            status.text = "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..";

        }

        if (manager.IsClientConnected())
        {
            status.text = "Client: address=" + manager.networkAddress;                
        }
    }

    void ShowNetworkConfigWindow()
    {
        networkConfigWindow.gameObject.SetActive(true);
        networkConfigIsHidden = false;
    }

    void HideNetworkConfigWindow()
    {
        networkConfigWindow.gameObject.SetActive(false);
        networkConfigIsHidden = true;
    }
}
