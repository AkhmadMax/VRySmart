using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vive.Plugin.SR;
using UnityEngine.Rendering;


public class ViveCamToMatHelper : MonoBehaviour
{
    /// <summary>
    /// UnityEvent that is triggered when this instance is initialized.
    /// </summary>
    public UnityEvent onInitialized;

    /// <summary>
    /// UnityEvent that is triggered when this instance is disposed.
    /// </summary>
    public UnityEvent onDisposed;

    /// <summary>
    /// UnityEvent that is triggered when this instance is error Occurred.
    /// </summary>
    public ErrorUnityEvent onErrorOccurred;

    /// <summary>
    /// The initialization coroutine.
    /// </summary>
    protected IEnumerator initCoroutine;

    /// <summary>
    /// Indicates whether this instance has been initialized.
    /// </summary>
    protected bool hasInitDone = false;

    /// <summary>
    /// Indicates whether this instance is waiting for initialization to complete.
    /// </summary>
    private bool isInitWaiting;

    public enum ErrorCode : int
    {
        UNKNOWN = 0,
        CAMERA_DEVICE_NOT_EXIST,
        CAMERA_PERMISSION_DENIED,
        TIMEOUT,
    }

    [Serializable]
    public class ErrorUnityEvent : UnityEvent<ErrorCode>
    {

    }

    Texture2D left;
    Texture2D right;

    public Texture2D leftCPU;

    public Texture2D leftCPU2;
    int frameIndex;
    int timeIndex;
    Matrix4x4 poseLeft;
    Matrix4x4 poseRight;
    double focalLengthLeft;
    double CxLeft;
    double CyLeft;
    private AsyncGPUReadbackRequest request;
    private RenderTexture r;

    /// <summary>
    /// Indicates whether this instance has been initialized.
    /// </summary>
    /// <returns><c>true</c>, if this instance has been initialized, <c>false</c> otherwise.</returns>
    public virtual bool IsInitialized()
    {
        return hasInitDone;
    }

    /// <summary>
    /// Indicates whether the active camera is currently playing.
    /// </summary>
    /// <returns><c>true</c>, if the active camera is playing, <c>false</c> otherwise.</returns>
    public virtual bool IsPlaying()
    {
        return true; //hasInitDone ? FrameworkStatus.WORKING : false;
    }

    /// <summary>
    /// Indicates whether the video buffer of the frame has been updated.
    /// </summary>
    /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
    public virtual bool DidUpdateThisFrame()
    {
        if (!hasInitDone)
            return false;

        return true; // webCamTexture.didUpdateThisFrame;
    }


    public void Initialize()
    {
        if (onInitialized == null)
            onInitialized = new UnityEvent();
        if (onDisposed == null)
            onDisposed = new UnityEvent();
        if (onErrorOccurred == null)
            onErrorOccurred = new ErrorUnityEvent();

        initCoroutine = _Initialize();
        StartCoroutine(initCoroutine);
    }

    IEnumerator _Initialize()
    {
        Debug.Log("Starting");
        isInitWaiting = true;

        do
        {
            hasInitDone = true;

            hasInitDone &= ViveSR.FrameworkStatus == FrameworkStatus.WORKING;

            if (hasInitDone) InitTextures();

            ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
            hasInitDone &= left != null;


            if (hasInitDone)
            {
                focalLengthLeft = ViveSR_DualCameraImageCapture.FocalLengthLeft;
                CxLeft = ViveSR_DualCameraImageCapture.UndistortedCxLeft;
                CyLeft = ViveSR_DualCameraImageCapture.UndistortedCyLeft;
                frameMat = new Mat(left.height, left.width, CvType.CV_8UC4);


                hasInitDone = true;

                if (onInitialized != null)
                    onInitialized.Invoke();

                isInitWaiting = false;
                initCoroutine = null;

                Debug.Log("Started");
            }
            else
            {
                yield return null;
            }

        }
        while (!hasInitDone);
    }

    void Update()
    {

        if (hasInitDone && request.done)
        {
            ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
            r = RenderTexture.GetTemporary(left.width, left.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(left, r);
            request = AsyncGPUReadback.Request(r, 0, TextureFormat.RGBA32, OnCompleteReadback);
        }
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        RenderTexture.ReleaseTemporary(r);

        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        leftCPU.LoadRawTextureData(request.GetData<uint>());
        leftCPU.Apply();
    }

    /// <summary>
    /// Cancel Init Coroutine.
    /// </summary>
    protected virtual void CancelInitCoroutine()
    {
        if (initCoroutine != null)
        {
            StopCoroutine(initCoroutine);
            ((IDisposable)initCoroutine).Dispose();
            initCoroutine = null;
        }
    }

    /// <summary>
    /// Starts the camera.
    /// </summary>
    public virtual void Play()
    {
        //if (hasInitDone)
        //    webCamTexture.Play();
    }

    /// <summary>
    /// Stops the active camera.
    /// </summary>
    public virtual void Stop()
    {
        //if (hasInitDone)
        //    webCamTexture.Stop();
    }

    /// <summary>
    /// Pauses the active camera.
    /// </summary>
    public virtual void Pause()
    {
        //if (hasInitDone)
        //    webCamTexture.Pause();
    }

    public virtual void Dispose()
    {
        //if (colors != null)
        //    colors = null;

        if (isInitWaiting)
        {
            CancelInitCoroutine();
            ReleaseResources();
        }
        else if (hasInitDone)
        {
            //ReleaseResources();

            if (onDisposed != null)
                onDisposed.Invoke();
        }
    }

    /// <summary>
    /// The frame mat.
    /// </summary>
    protected Mat frameMat;

    ///// <summary>
    ///// The base mat.
    ///// </summary>
    //protected Mat baseMat;

    public Mat GetMat()
    {
        Utils.fastTexture2DToMat(leftCPU, frameMat, false);
        return frameMat;

    }

    public OpenCVForUnityExample.CameraParameters GetCameraParameters()
    {
        OpenCVForUnityExample.CameraParameters param = new OpenCVForUnityExample.CameraParameters();
        param.camera_matrix = new double[9];
        param.camera_matrix[0] = focalLengthLeft;
        param.camera_matrix[4] = focalLengthLeft;
        param.camera_matrix[2] = CxLeft;
        param.camera_matrix[5] = CyLeft;
        param.camera_matrix[8] = 1d;
        param.distortion_coefficients = new double[] { 0, 0, 0, 0 };
        param.image_width = leftCPU.width;
        param.image_height = leftCPU.height;
        return param;
    }

    void InitTextures()
    {
        left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);

    }

    /// <summary>
    /// To release the resources.
    /// </summary>
    protected virtual void ReleaseResources()
    {
        isInitWaiting = false;
        hasInitDone = false;

        //if (webCamTexture != null)
        //{
        //    webCamTexture.Stop();
        //    WebCamTexture.Destroy(webCamTexture);
        //    webCamTexture = null;
        //}
        //if (frameMat != null)
        //{
        //    frameMat.Dispose();
        //    frameMat = null;
        //}
        //if (baseMat != null)
        //{
        //    baseMat.Dispose();
        //    baseMat = null;
        //}
        //if (rotatedFrameMat != null)
        //{
        //    rotatedFrameMat.Dispose();
        //    rotatedFrameMat = null;
        //}
    }
}
