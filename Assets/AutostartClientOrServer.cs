using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AutostartClientOrServer : MonoBehaviour
{
    NetworkClient client;
    int cooldown = 5;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
            StartCoroutine(StartClientCoroutine());

        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            StartServerCoroutine();

    }
    IEnumerator StartClientCoroutine()
    {
        do
        {
            client = NetworkManager.singleton.StartClient();
            if (client.isConnected)
                yield return null;
            else
                yield return new WaitForSeconds(cooldown);
        }
        while (!client.isConnected);
    }

    void StartServerCoroutine()
    {
        NetworkManager.singleton.StartServer();
    }
}
