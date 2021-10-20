using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;
using UnityEngine.Rendering;
using OpenCvSharp;
using System;
using ArucoUnity.Objects.Trackers;
using ArucoUnity.Objects;
using ArucoUnity.Plugin;

public class TakeItOrLeaveIt : MonoBehaviour
{
    Texture2D left;
    public Texture2D leftCPU;
    public Texture2D leftCPU2;

    public List<ArucoObject> arucoObjects = new List<ArucoObject>();

    Texture2D right;
    int frameIndex;
    int timeIndex;
    Matrix4x4 poseLeft;
    Matrix4x4 poseRight;

    AsyncGPUReadbackRequest request;
    private bool initialized;

    double focalLengthLeft;
    double CxLeft;
    double CyLeft;

    RenderTexture r;

    public ArucoMarkerTracker MarkerTracker { get; protected set; }


    void Update()
    {

        if (initialized && request.done)
        {
            ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
            r = RenderTexture.GetTemporary(left.width, left.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(left, r);
            request = AsyncGPUReadback.Request(r, 0, TextureFormat.RGBA32, OnCompleteReadback);
        }

        //leftEye.mainTexture = leftCPU;
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        do
        {
            initialized = true;

            initialized &= ViveSR.FrameworkStatus == FrameworkStatus.WORKING;

            if (initialized) InitTextures();

            ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
            initialized &= left != null;


            if (initialized)
            {
                focalLengthLeft = ViveSR_DualCameraImageCapture.FocalLengthLeft;
                CxLeft = ViveSR_DualCameraImageCapture.UndistortedCxLeft;
                CyLeft = ViveSR_DualCameraImageCapture.UndistortedCyLeft;
                MarkerTracker = new ArucoMarkerTracker();

                mat = new Cv.Mat(left.height, left.width, Cv.Type.CV_8UC4, left.GetRawTextureData());
                leftCPU2 = new Texture2D(left.width, left.height, TextureFormat.RGBA32, false);

                Debug.Log("Started");
            }
            else
            {
                yield return null;
            }

        }
        while (!initialized);
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

        int elemsize = (int)mat.ElemSize();
        int total = (int)mat.Total();

        //Debug.Log(request.height + " " + elemsize + " " + total);

        foreach (ArucoObject aruco in arucoObjects)
        {
            Debug.Log("asd");
            ProcessFrame(mat, aruco.Dictionary);
        }
    }

    private void ProcessFrame(Cv.Mat mat, Aruco.Dictionary dict)
    {
        MarkerTracker.Detect(0, dict, mat);
        MarkerTracker.Draw(0, dict, mat);

        Debug.Log(mat.Cols + " " + mat.Rows + " " + mat.DataByte[10000]);
        //MarkerTracker.Draw(0, dict, mat);
    }

    Cv.Mat mat;
    void InitTextures()
    {
        left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
    
    }
}
