using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;

public class SimpleNetworkManger : MonoBehaviour
{
    public string networkAddress = "192.168.0.211";
    NetworkManager networkManager;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.networkAddress = networkAddress;
    }

    public void StartServer()
    {
        networkManager.StartServer();
    }

    public void StartClient()
    {
        networkManager.StartClient();
    }
}
