using UnityEngine;
using UnityEngine.UI;
using Mediapipe.SelfieSegmentation;
using Unity.RenderStreaming;
using System;


public class CameraStreamManager : MonoBehaviour
{
    enum Mode
    {
        Selfie,
        Main
    }

    [SerializeField] private SingleConnection connection;
    [SerializeField] private ReceiveVideoViewer receiveVideoViewer;

    [SerializeField] RawImage compositeImage;
    [SerializeField] SelfieSegmentationResource resource;
    [SerializeField] Shader shader;
    public Camera virtualSelfieCam;
    public Camera virtualMainCam;
    [SerializeField] Texture bgTextureSelfie;
    [SerializeField] Texture bgTextureMain;

    private Texture sourceTexture;
    private Texture bgTexture;
    private string connectionId;
    private Mode mode = Mode.Selfie;
    private Texture2D black;




    SelfieSegmentation segmentation;
    Material material;

    void Start()
    {
        material = new Material(shader);
        compositeImage.material = material;
        segmentation = new SelfieSegmentation(resource);
        black = new Texture2D(1, 1);
        black.SetPixel(0, 0, Color.black);
        black.Apply();
    }

    public void ToggleMainCamera()
    {
        StopSession();
        virtualSelfieCam.enabled = false;
        virtualMainCam.enabled = true;
        bgTexture = bgTextureMain;
        material.SetTexture("_backImage", bgTexture);
        material.SetFloat("_ScaleY", 1);
        compositeImage.texture = black;
        mode = Mode.Main;
    }

    public void ToggleSelfieCamera()
    {
        StartSession();
        virtualSelfieCam.enabled = true;
        virtualMainCam.enabled = false;
        bgTexture = bgTextureSelfie;
        material.SetTexture("_backImage", bgTexture);
        material.SetFloat("_ScaleY", -1);
        mode = Mode.Selfie;
    }

    public void ToggleMode()
    {
        switch (mode)
        {
            case Mode.Selfie:
                ToggleMainCamera();
                break;
            case Mode.Main:
                ToggleSelfieCamera();
                break;
        }
    }

    public void StartSession()
    {
        receiveVideoViewer.OnUpdateReceiveTexture += OnUpdateTexture;

        if (string.IsNullOrEmpty(connectionId))
        {
            connectionId = System.Guid.NewGuid().ToString("N");
        }
        connection.CreateConnection(connectionId);

    }

    public void StopSession()
    {
        receiveVideoViewer.OnUpdateReceiveTexture -= OnUpdateTexture;

        connection.DeleteConnection(connectionId);
        connectionId = String.Empty;
    }

    private void OnUpdateTexture(Texture receiveTexture)
    {
        sourceTexture = receiveTexture;
    }

    void LateUpdate(){

        if(sourceTexture && connection.IsConnected(connectionId))
        {
            // Predict segmentation by neural network model.
            segmentation.ProcessImage(sourceTexture);

            //Set segmentation texutre to `_MainTex` variable of shader.
            compositeImage.texture = segmentation.texture;

            material.SetTexture("_inputImage", sourceTexture);
        }
    }

    void OnApplicationQuit(){
        StopSession();
        segmentation.Dispose();
    }
}
