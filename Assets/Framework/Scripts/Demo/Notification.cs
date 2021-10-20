using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Notification : MonoBehaviour
{
    public HapticNetworkBehaviour hapticApi;
    public AudioSource audioSource;
    public AudioClip notificationSfx;

    public MessageTemplate messageTemplate;
    public AppsManager appsManager;
    public CameraApp cameraApp;
    public MessengerApp messenger;
    public Button cameraBtn;
    public Button quickReplyBtn;

    private void Start()
    {
        cameraBtn.onClick.AddListener(OnOpenCameraBtn);
        quickReplyBtn.onClick.AddListener(OnQuickReplyButton);

    }

    private void OnEnable()
    {
        audioSource.PlayOneShot(notificationSfx);
        StartCoroutine(HapticSignal(6, 0.5f));
    }

    void OnOpenCameraBtn()
    {
        appsManager.OpenCamera();
        RectTransform chatView = messenger.chats.First(c => c.Key.name.text == "Mom").Value;
        cameraApp.MessengerSetup(chatView);
        messenger.contactsView.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void OnQuickReplyButton()
    {
        RectTransform chatView = messenger.chats.First(c => c.Key.name.text == "Mom").Value;
        MessageTemplate message = Instantiate(messageTemplate, chatView);
        message.text.text = "I'll call you later";
        appsManager.OpenMenu();
        gameObject.SetActive(false);
    }

    IEnumerator HapticSignal(int count, float interval)
    {
        for(int i = 0; i < count; i++)
        {
            hapticApi.SendHapticRequest();
            yield return new WaitForSeconds(interval);
        }

    }
}
