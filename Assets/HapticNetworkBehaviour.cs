using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class HapticNetworkBehaviour : NetworkBehaviour
{
    public NetworkManager manager;
    public override void OnStartClient()
    {
        manager.client.RegisterHandler(CustomMsgType.Haptic, OnHapticRequest);
        Vibration.Init();
    }

    [Server]
    public void SendHapticRequest()
    {
        EmptyMessage msg = new EmptyMessage();
        NetworkServer.SendToAll(CustomMsgType.Haptic, msg);
    }

    [Client]
    private void OnHapticRequest(NetworkMessage netMsg)
    {
        Vibration.VibratePop();
    }


}
