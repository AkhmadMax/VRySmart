using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ContactTemplate : MonoBehaviour
{
    public Text name;
    public Text icon;
    public Image iconBg;
    public Button button;
    //public string[] messages;

    public class OnContactSelectedEvent : UnityEvent<ContactTemplate>{}
    public OnContactSelectedEvent OnContactSelected;

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
        if (OnContactSelected == null)
            OnContactSelected = new OnContactSelectedEvent();
    }

    private void OnClick()
    {
        if (OnContactSelected != null)
        {
            OnContactSelected.Invoke(this);
        }
    }
}
