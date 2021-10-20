using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.RenderStreaming;
using Unity.WebRTC;
using Unity.RenderStreaming.Signaling;
using UnityEngine.Networking;

public class CameraStreamingManager : NetworkBehaviour
{
    public RenderStreaming renderStreaming;
    public List<SignalingHandlerBase> handlers = new List<SignalingHandlerBase>();

    private string signalingType = typeof(WebSocketSignaling).FullName;
    private float interval = 5.0f;
    private bool hardwareEncoderSupport = false;

    public override void OnStartServer()
    {
        StartStreaming(NetworkManager.singleton.networkAddress);
    }

    public override void OnStartClient()
    {
        StartStreaming(NetworkManager.singleton.networkAddress);
    }
    private void OnDisable()
    {
        StopStreaming();
    }

    public void StartStreaming(string serverURL)
    {
        RTCConfiguration conf = new RTCConfiguration();
        string signalingURL = "ws://" + serverURL;
        ISignaling signaling = CreateSignaling(
            signalingType, signalingURL, interval, SynchronizationContext.Current);
        renderStreaming.Run(conf, hardwareEncoderSupport, signaling, handlers.ToArray());
    }

    public void StopStreaming()
    {
        renderStreaming.Stop();
    }

    static ISignaling CreateSignaling(string type, string url, float interval, SynchronizationContext context)
    {
        object[] args = { url, interval, context };
        return (ISignaling)Activator.CreateInstance(typeof(WebSocketSignaling), args);
    }
}
