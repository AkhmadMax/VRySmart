using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;
using System.IO;

public class ImageTransmitter : NetworkBehaviour
{
    //public RawImage output;
    NetworkTransmitter networkTransmitter;
    public bool isTransmitting { get; private set; }

    private void Awake()
    {
        //on client and server
        networkTransmitter = GetComponent<NetworkTransmitter>();
    }


    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler(CustomMsgType.TransmissionFinished, OnTransmissionFinished);

    }

    [Server]
    private void OnTransmissionFinished(NetworkMessage msg)
    {
        Debug.Log("TransmissionFinished");
        isTransmitting = false;
    }

    public override void OnStartClient()
    {
        //on client: listen for and handle received data
        networkTransmitter.OnDataCompletelyReceived += MyCompletelyReceivedHandler;
        //networkTransmitter.OnDataFragmentReceived += MyFragmentReceivedHandler;
    }

    [Server]
    public void Send(Texture2D image)
    {
        isTransmitting = true;
        //on server: transmit data. myDataToSend is an object serialized to byte array.
        byte[] data = image.EncodeToJPG(); 
        StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(0, data));
    }

    //on client this will be called once the complete data array has been received
    [Client]
    private void MyCompletelyReceivedHandler(int transmissionId, byte[] data)
    {
        //deserialize data to object and do further things with it...
        //Texture2D input = new Texture2D(540, 720, TextureFormat.RGBA32, false);
        //input.LoadRawTextureData(data);
        //input.Apply();
        SaveToGallery.SaveImageToGallery(data, "Test" + Random.RandomRange(0, 100000), "Description");

        EmptyMessage emptyMessage = new EmptyMessage();
        NetworkManager.singleton.client.Send(CustomMsgType.TransmissionFinished, emptyMessage);
    }
    //on clients this will be called every time a chunk (fragment of complete data) has been received
    [Client]
    private void MyFragmentReceivedHandler(int transmissionId, byte[] data)
    {
        //Debug.Log(data.Length);
        //update a progress bar or do something else with the information
    }
}
