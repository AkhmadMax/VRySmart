using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using UnityEngine.Events;

[Serializable]
public class OnCameraMsgEvent : UnityEvent<bool> { }

public class SimpleMessagesHandler : NetworkBehaviour
{
    public OnCameraMsgEvent OnCameraMsgReceived;

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
        OnCameraMsgReceived?.Invoke(value == 0 ? false : true);
    }

    public void SendCameraMsg(bool enabled)
    {
        IntegerMessage msg = new IntegerMessage(enabled ? 1 : 0);
        NetworkManager.singleton.client.Send(CustomMsgType.Camera, msg);
    }
}
