using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleNetworkManger : MonoBehaviour
{
    public string networkAddress = "192.168.0.211";

    private void Awake()
    {
        NetworkManager networkManager = GetComponent<NetworkManager>();
        networkManager.networkAddress = this.networkAddress;
    }
}
