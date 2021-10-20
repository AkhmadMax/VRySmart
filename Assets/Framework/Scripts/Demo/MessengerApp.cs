using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessengerApp : MonoBehaviour
{
    public AppsManager appsManager;
    public ContactTemplate contactTemplate;
    public RectTransform chatTemplate;
    public MessageTemplate messageTemplate;

    public RectTransform contactsView;
    public RectTransform contentView;

    public Button backBtn;

    [Serializable]
    public class Chat
    {
        public string name;
        public string[] messages;
    }

    [Serializable]
    public class Messenger
    {
        public Chat[] chats;
    }

    Messenger messenger;
    
    [HideInInspector]
    public Dictionary<ContactTemplate, RectTransform> chats = new Dictionary<ContactTemplate, RectTransform>();

    private void Start()
    {
        Init();
        //gameObject.SetActive(false);
    }

    void Init()
    {
        string jsonPath = Application.streamingAssetsPath + "/" + "messenger.json";
        string json = File.ReadAllText(jsonPath);
        messenger = JsonUtility.FromJson<Messenger>(json);
        backBtn.onClick.AddListener(OnBackBtn);

        foreach (Chat chat in messenger.chats)
        {
            ContactTemplate contact = Instantiate(contactTemplate, contactsView);
            contact.name.text = chat.name;
            contact.icon.text = chat.name[0].ToString();
            contact.iconBg.color = UnityEngine.Random.ColorHSV(0, 1, 0.5f, 0.5f, 0.8f, 0.8f, 1, 1);
            contact.OnContactSelected.AddListener(OnContactSelected);
            //contact.messages = chat.messages;

            RectTransform chatView = Instantiate(chatTemplate, contentView);
            chats.Add(contact, chatView);
            chatView.gameObject.SetActive(false);

            foreach (string msg in chat.messages)
            {
                MessageTemplate message = Instantiate(messageTemplate, chatView);
                message.text.text = msg;
            }
        }
    }

    private void OnBackBtn()
    {
        foreach (KeyValuePair<ContactTemplate, RectTransform> entry in chats)
        {
            if(entry.Value.gameObject.activeInHierarchy)
            {
                contactsView.gameObject.SetActive(true);
                entry.Value.gameObject.SetActive(false);
                return;
            }
        }
        appsManager.OpenMenu();
    }

    public void OnContactSelected(ContactTemplate contact)
    {
        foreach (KeyValuePair<ContactTemplate, RectTransform> entry in chats)
        {
            if (entry.Key == contact)
                entry.Value.gameObject.SetActive(true);
            else
                entry.Value.gameObject.SetActive(false);
        }
        contactsView.gameObject.SetActive(false);
    }
}
