using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class LightsaberDevice : Device
{
    public AppsManager appsManager;
    public Transform blade;
    public AudioClip saberOn;
    public AudioClip saberOff;
    public AudioClip saberAmbient;

    public AudioSource audioSource;
    public UnityEvent OnToggleOffFinished;

    bool isBusy;

    public override void ToggleOn()
    {
        if(!isBusy)
        {
            audioSource.pitch = 1;
            isBusy = true;
            gameObject.SetActive(true);
            blade.DOScaleZ(1, saberOn.length).OnComplete(() => { isBusy = false; });
            audioSource.PlayOneShot(saberOn);
            audioSource.clip = saberAmbient;
            ulong delay = (ulong)(saberOn.length * 44000);
            audioSource.loop = true;
            audioSource.Play(delay);
        }
    }

    public override void ToggleOff()
    {
        if(!isBusy)
        {
            isBusy = true;
            audioSource.Stop();
            audioSource.clip = saberOff;
            audioSource.pitch = 1.5f;
            audioSource.loop = false;
            audioSource.Play();
            blade.DOScaleZ(0, GetHideDuration() * 0.5f).OnComplete(() => FinishToggleOff());
        }

    }

    void FinishToggleOff()
    {
        if (OnToggleOffFinished != null) OnToggleOffFinished.Invoke();
        gameObject.SetActive(false);
        isBusy = false;
    }

    public override float GetHideDuration()
    {
        return saberOff.length;
    }

    public override float GetShowDuration()
    {
        return saberOn.length;
    }
}
