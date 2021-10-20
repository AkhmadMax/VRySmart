using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraApp : MonoBehaviour
{
    public CameraStreamManager streamManager;
    public AppsManager appsManager;
    public ImageTransmitter imageTransmitter;
    public AudioSource audioSource;
    public RenderTexture deviceScreen;
    public Button shutterBtn;
    public AudioClip shutterSound;
    public PhotoMessageTemplate photoMessageTemplate;
    public List<Texture2D> photos = new List<Texture2D>();

    // Messenger setup
    bool messengerSetup;
    RectTransform chatView;

    public void MessengerSetup(RectTransform chatView)
    {
        this.chatView = chatView;
        messengerSetup = true;
    }

    private void Start()
    {
        shutterBtn.onClick.AddListener(OnShutter);
    }


    private void OnShutter()
    {
        audioSource.PlayOneShot(shutterSound);
        if(messengerSetup)
        {
            SendAsMessage();
        }
        else
        {
            SaveToGallery();
        }
        StartCoroutine(DisableShutterButtonUntilSaved());
        messengerSetup = false;
    }

    IEnumerator DisableShutterButtonUntilSaved()
    {
        shutterBtn.interactable = false;
        while (imageTransmitter.isTransmitting)
        {
            yield return new WaitForEndOfFrame();
        }
        shutterBtn.interactable = true;
    }

    private void SaveToGallery()
    {
        int height = (deviceScreen.width / 3) * 4;
        var tex = new Texture2D(deviceScreen.width, height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = deviceScreen;
        tex.ReadPixels(new Rect(0, 0, deviceScreen.width, height), 0, 0);
        tex.Apply();
        photos.Add(tex);
        imageTransmitter.Send(tex);
    }

    private void SendAsMessage()
    {
        // open chat
        appsManager.OpenMessenger();
        chatView.gameObject.SetActive(true);
        int height = (deviceScreen.width / 3) * 4;
        var tex = new Texture2D(deviceScreen.width, height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = deviceScreen;
        tex.ReadPixels(new Rect(0, 0, deviceScreen.width, height), 0, 0);
        tex.Apply();

        PhotoMessageTemplate photoMessage = Instantiate(photoMessageTemplate, chatView);
        photoMessage.image.texture = tex;
    }

    private void OnEnable()
    {
        streamManager.ToggleSelfieCamera();
    }

    private void OnDisable()
    {
        streamManager.StopSession();
    }

    public void ToggleMode()
    {
        streamManager.ToggleMode();
    }

    private void OnDestroy()
    {
        photos.Clear();
    }
}
