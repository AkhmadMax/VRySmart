using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GalleryApp : MonoBehaviour
{
    public CameraApp cameraApp;
    public RawImage photoHolder;
    public RectTransform cameraRoll;

    private List<RawImage> photoHolders = new List<RawImage>();
    private void Awake()
    {
        for(int i = 0; i < 20; i++)
        {
            RawImage holder = Instantiate(photoHolder, cameraRoll);
            holder.gameObject.SetActive(false);
            photoHolders.Add(holder);
        }
    }

    private void OnEnable()
    {
        int i = 0;
        foreach(Texture2D photo in cameraApp.photos)
        {
            photoHolders[i].texture = photo;
            photoHolders[i].gameObject.SetActive(true);
            i++;
        }
    }
}
