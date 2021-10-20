using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    [Serializable]
    public class DialogueInfo
    {
        public AppsManager.AppName name;
        public AudioClip clip;
    }
    public AppsManager appsManager;
    public AudioSource audioSource;
    public TMP_Text text;
    public List<DialogueInfo> dialogueInfo = new List<DialogueInfo>();

    class Dialogue
    {
        public string intro;
        public string device;
        public string slingshot;
        public string lightsaber;
        public string camera;
        public string messenger;
    }
    Dialogue dialogue;

    // To check if corresponding dialogue was already show for this app
    Dictionary<AppsManager.AppName, bool> isShown = new Dictionary<AppsManager.AppName, bool>();

    private void Start()
    {
        string json = File.ReadAllText(Application.streamingAssetsPath + "/" + "dialogue.json");
        appsManager.OnAppActivated.AddListener(OnAppActivated);
        ReadJsonToObject(json);

        foreach(AppsManager.AppName appName in Enum.GetValues(typeof(AppsManager.AppName)))
        {
            isShown.Add(appName, false);
        }
    }

    private void OnAppActivated(AppsManager.AppName appName)
    {
        if (isShown[appName] == false)
        {
            switch (appName)
            {
                case AppsManager.AppName.welcomeScreen:
                    SayIntro();
                    break;
                case AppsManager.AppName.menu:
                    SayDevice();
                    break;
                case AppsManager.AppName.slingshot:
                    SaySlingshot();
                    break;
                case AppsManager.AppName.lighsaber:
                    SayLightsaber();
                    break;
                case AppsManager.AppName.camera:
                    SayCamera();
                    break;
                case AppsManager.AppName.messenger:
                    SayMessenger();
                    break;
            }
        }
        isShown[appName] = true;

        if(appName == AppsManager.AppName.menu)
        {
            bool allShown = true;
            foreach (var key in isShown.Keys.ToList())
            {
                allShown = isShown[key];
            }
            if(allShown)
            {
                StartCoroutine(WaitAndSendMessage());
            }
        }
    }

    void ReadJsonToObject(string json)
    {
        dialogue = JsonUtility.FromJson<Dialogue>(json);
    }

    void SayIntro()
    {
        text.text = dialogue.intro;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.welcomeScreen);
        audioSource.PlayOneShot(info.clip);

    }

    void SayDevice()
    {
        text.text = dialogue.device;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.menu);
        audioSource.PlayOneShot(info.clip);
    }

    void SayMessenger()
    {
        text.text = dialogue.messenger;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.messenger);
        audioSource.PlayOneShot(info.clip);
    }

    void SaySlingshot()
    {
        text.text = dialogue.slingshot;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.slingshot);
        audioSource.PlayOneShot(info.clip);
    }

    void SayLightsaber()
    {
        text.text = dialogue.lightsaber;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.lighsaber);
        audioSource.PlayOneShot(info.clip);
    }

    void SayCamera()
    {
        text.text = dialogue.camera;
        DialogueInfo info = dialogueInfo.First(x => x.name == AppsManager.AppName.camera);
        audioSource.PlayOneShot(info.clip);
    }

    IEnumerator WaitAndSendMessage()
    {
        yield return new WaitForSeconds(5);

    }



}
