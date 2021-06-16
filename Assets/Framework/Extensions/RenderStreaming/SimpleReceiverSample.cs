using System;
using Unity.RenderStreaming;
using UnityEngine;
using UnityEngine.UI;

class SimpleReceiverSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private RenderStreaming renderStreaming;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RawImage remoteVideoImage;
    [SerializeField] private ReceiveVideoViewer receiveVideoViewer;
    [SerializeField] private SingleConnection connection;
#pragma warning restore 0649

    private string connectionId;

    void Awake()
    {
        receiveVideoViewer.OnUpdateReceiveTexture += texture => remoteVideoImage.texture = texture;
    }

    void Start()
    {
        if (renderStreaming.runOnAwake)
            return;
        renderStreaming.Run();
    }

    public void StartSession()
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            connectionId = System.Guid.NewGuid().ToString("N");
        }
        connection.CreateConnection(connectionId);

        AttachRemoteImageToRemotePlayerPos();
    }

    private void AttachRemoteImageToRemotePlayerPos()
    {
        canvas.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
        canvas.transform.localPosition = Vector3.zero + Vector3.up * 0.2f;
        canvas.transform.localRotation = Quaternion.identity;
    }

    public void StopSession()
    {
        connection.DeleteConnection(connectionId);
        connectionId = String.Empty;
    }

    public void ToggleSession(bool enabled)
    {
        if (enabled)
            StartSession();
        else
            StopSession();
    }
}
