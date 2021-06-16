using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;

public class SimpleMessagesHandler : NetworkBehaviour
{
    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        NetworkServer.RegisterHandler(CustomMsgType.Camera, OnCameraMsg);
    }

    [Server]
    private void OnCameraMsg(NetworkMessage netMsg)
    {
        int value = netMsg.ReadMessage<IntegerMessage>().value;
        Debug.Log(value);
    }

    public void SendCameraMsg(bool enabled)
    {
        IntegerMessage msg = new IntegerMessage(enabled ? 1 : 0);
        NetworkManager.singleton.client.Send(CustomMsgType.Camera, msg);
    }
}
