using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System;

public class MessagesHandler : NetworkBehaviour
{
    public event Action<int> OnCommandEvent;
    public GameObject markerScreen;
    public GameObject receiverScreen;
    private void Awake()
    {
        OnCommandEvent += OnCommand;
    }

    private void OnDestroy()
    {
        OnCommandEvent -= OnCommand;
    }

    private void Start()
    {
        if(isServer)
        {
            NetworkServer.RegisterHandler(CustomMsgType.RegNumCmd, OnCommandFromClient);
        }
        if(isClient)
        {
            NetworkManager.singleton.client.RegisterHandler(CustomMsgType.SendNumCmd, OnCommandFromServer);
            NetworkManager.singleton.client.RegisterHandler(CustomMsgType.Calibration, OnCalibrationEvent);
        }
    }

    [Client]
    private void OnCalibrationEvent(NetworkMessage netMsg)
    {
        GameObject phone = FindObjectOfType<PhoneController>().gameObject;
        if(phone)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Calibka";
            sphere.transform.localScale = Vector3.one * 0.1f;
            Destroy(sphere.GetComponent<Collider>());
            sphere.transform.position = phone.transform.Find("Sphere").position;
        }
    }

    private void Update()
    {
        if(SystemInfo.deviceType == DeviceType.Desktop && Input.inputString.Length > 0)
        {
            IntegerMessage msg;
            switch (Input.inputString)
            {
                case "1": 
                    msg = new IntegerMessage(1);
                    NetworkManager.singleton.client.Send(CustomMsgType.RegNumCmd, msg);
                    break;
                case "2":
                    msg = new IntegerMessage(2);
                    NetworkManager.singleton.client.Send(CustomMsgType.RegNumCmd, msg);
                    break;
                case "3":
                    msg = new IntegerMessage(3);
                    NetworkManager.singleton.client.Send(CustomMsgType.RegNumCmd, msg);
                    break;
                case "4":
                    msg = new IntegerMessage(4);
                    NetworkManager.singleton.client.Send(CustomMsgType.RegNumCmd, msg);
                    break;
                case "5":
                    msg = new IntegerMessage(5);
                    NetworkManager.singleton.client.Send(CustomMsgType.RegNumCmd, msg);
                    break;
            }


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
        int value = netMsg.ReadMessage<IntegerMessage>().value;
        MessageBase msg = new IntegerMessage(value);
        NetworkServer.SendToAll(CustomMsgType.SendNumCmd, msg);
    }

    private void OnCommand(int obj)
    {
        switch (obj)
        {
            case 1:
                if (markerScreen)
                    ToggleMarkerScreen();
                break;
            case 4:
                if (receiverScreen)
                    ToggleReceiverScree();
                break;
        }
    }

    private void ToggleReceiverScree()
    {
        receiverScreen.SetActive(!receiverScreen.activeInHierarchy);
    }

    private void ToggleMarkerScreen()
    {
        markerScreen.SetActive(!markerScreen.activeInHierarchy);
    }

    public void SendCalibrationEvent()
    {
        StringMessage stringMessage = new StringMessage();
        NetworkServer.SendToAll(CustomMsgType.Calibration, stringMessage);
    }
}
