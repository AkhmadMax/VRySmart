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
    public bool isServer;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        networkManager.networkAddress = networkAddress;
    }

    void Start()
    {
        if (isServer)
            StartServer();
        else
            StartClient();
    }

    private void OnDestroy()
    {
        if (isServer)
            networkManager.StopServer();
        else
            networkManager.StopClient();
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
