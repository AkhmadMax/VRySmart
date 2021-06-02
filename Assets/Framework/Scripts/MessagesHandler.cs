using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

using Google.XR.ARCoreExtensions.Samples.CloudAnchors;
using System;

public class MessagesHandler : NetworkBehaviour
{
    public CloudAnchorsNetworkManager manager;
    public event Action<int> OnCommandEvent;


    private void Start()
    {
        if(isServer)
        {
            NetworkServer.RegisterHandler(MsgType.Highest + 2, OnCommandFromClient);
        }
        if(isClient)
        {
            NetworkManager.singleton.client.RegisterHandler(MsgType.Highest + 1, OnCommandFromServer);
        }
    }

    private void Update()
    {
        if(SystemInfo.deviceType == DeviceType.Desktop && Input.GetKeyDown(KeyCode.Alpha1))
        {
            var msg = new IntegerMessage(1);
            NetworkManager.singleton.client.Send(MsgType.Highest + 2, msg);
        }
    }

    private void OnCommandFromServer(NetworkMessage netMsg)
    {
        Debug.Log("OnCommandFromServer");

        //if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            int msg = netMsg.ReadMessage<IntegerMessage>().value;
            OnCommandEvent?.Invoke(msg);
        }
    }

    private void OnCommandFromClient(NetworkMessage netMsg)
    {
        Debug.Log("OnCommandFromClient");
        MessageBase msg = new IntegerMessage(999);
        NetworkServer.SendToAll(MsgType.Highest + 1, msg);
    }
}
