using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class WebCamToRawImage : MonoBehaviour
{
    [SerializeField, Tooltip("Device index of web camera")]
    private int deviceIndex = 0;

    private WebCamTexture m_webCamTexture;
    private Coroutine m_startVideoCorutine;

    public RawImage webcamImage;

    protected virtual void Start()
    {
        m_startVideoCorutine = StartCoroutine(StartVideo());
        
        foreach(WebCamDevice device in WebCamTexture.devices)
        {
            Debug.Log(device.name);
        }
    }

    protected virtual void OnEnable()
    {
        m_webCamTexture?.Play();
    }

    protected virtual void OnDisable()
    {
        if (m_startVideoCorutine != null)
        {
            StopCoroutine(m_startVideoCorutine);
            m_startVideoCorutine = null;
        }
        m_webCamTexture?.Pause();
    }

    IEnumerator StartVideo()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogFormat("WebCam device not found");
            yield break;
        }

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogFormat("authorization for using the device is denied");
            yield break;
        }

        WebCamDevice userCameraDevice = WebCamTexture.devices[deviceIndex];
        m_webCamTexture = new WebCamTexture(userCameraDevice.name, 640, 480);
        m_webCamTexture.Play();
        webcamImage.texture = m_webCamTexture;
        yield return new WaitUntil(() => m_webCamTexture.didUpdateThisFrame);
        m_startVideoCorutine = null;
    }
}
