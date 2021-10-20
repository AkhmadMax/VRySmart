using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AppActivatedEvent : UnityEvent<AppsManager.AppName> { }


public class AppsManager : MonoBehaviour
{

    public AppActivatedEvent OnAppActivated;

    [Serializable]
    public enum AppName
    {
        welcomeScreen,
        menu,
        slingshot,
        lighsaber,
        camera,
        messenger,
        gallery,
        notification
    }

    [Serializable]
    public enum AppType
    {
        device,
        screen
    }

    [Serializable]
    public class AppInfo
    {
        public AppName appName;
        public AppType appType;
        public RectTransform rectTransform;
        public Device device;
        internal bool isActive;
    }

    public GameObject screen;
    public GameObject debugOSD;
    public List<AppInfo> apps = new List<AppInfo>();

    private AppInfo previousApp;

    private void Awake()
    {
        if (OnAppActivated == null)
            OnAppActivated = new AppActivatedEvent();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            OpenApp(AppName.welcomeScreen);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            ShowNotification();
        }
        if(Input.GetKeyUp(KeyCode.Alpha0))
        {
            if (debugOSD.activeSelf)
                debugOSD.SetActive(false);
            else
                debugOSD.SetActive(true);
        }    
    }

    public void OpenApp(AppName appName)
    {
        foreach(AppInfo appInfo in apps)
        {
            if (appInfo.appName == appName && appInfo.isActive == false)
            {
                appInfo.isActive = true;
                StopAllCoroutines();

                StartCoroutine(EnableTouchProxy(appInfo));
                
                // Activate Virtual Screen if the app is Screen based, otherwise disable it
                if(appInfo.appType == AppType.screen)
                {
                    if(screen.activeSelf == false)
                    {
                        StartCoroutine(EnableScreen(appInfo));
                    }
                }
                else
                {
                    screen.SetActive(false);
                }

                // Activate associated device if the app is device dependent
                if (appInfo.device != null)
                {
                    StartCoroutine(EnableDevice(appInfo));
                }

                if(OnAppActivated != null)
                    OnAppActivated.Invoke(appName);

                previousApp = appInfo;
            }
            else if (appInfo.isActive)
            {
                appInfo.isActive = false;
                
                // Keep the screen on in backgrpund, if the app is notification
                if(appInfo.appName != AppName.notification)
                {
                    appInfo.rectTransform.gameObject.SetActive(false);
                }
                if (appInfo.device)
                {
                    appInfo.device.ToggleOff();
                }
            }
        }
    }

    IEnumerator EnableScreen(AppInfo appInfo)
    {
        // Wait for previous device to hide
        if (previousApp != null && previousApp.device != null)
        {
            yield return new WaitForSeconds(previousApp.device.GetHideDuration());
        }

        // Wait for current device to show
        if (appInfo.device != null)
        {
            yield return new WaitForSeconds(appInfo.device.GetShowDuration());
        }

        screen.SetActive(true);
    }

    IEnumerator EnableTouchProxy(AppInfo appInfo)
    {
        // Wait for previous device to hide
        if (previousApp != null && previousApp.device != null)
        {
            yield return new WaitForSeconds(previousApp.device.GetHideDuration());
        }

        // Wait for current device to show
        if (appInfo.device != null)
        {
            yield return new WaitForSeconds(appInfo.device.GetShowDuration());
        }

        appInfo.rectTransform.gameObject.SetActive(true);
    }

    IEnumerator EnableDevice(AppInfo appInfo)
    {
        if (previousApp != null && previousApp.device != null)
        {
            yield return new WaitForSeconds(previousApp.device.GetHideDuration());
            appInfo.device.ToggleOn();
        }
        else
        {
            appInfo.device.ToggleOn();
        }
    }

    public void ShowNotification()
    {
        OpenApp(AppName.notification);
    }

    public void OpenMenu()
    {
        OpenApp(AppName.menu);
    }

    public void OpenGallery()
    {
        OpenApp(AppName.gallery);
    }

    public void OpenMessenger()
    {
        OpenApp(AppName.messenger);
    }

    public void OpenSlingshot()
    {
        OpenApp(AppName.slingshot);
    }

    public void OpenLightsaber()
    {
        OpenApp(AppName.lighsaber);
    }

    public void OpenCamera()
    {
        OpenApp(AppName.camera);
    }


}
