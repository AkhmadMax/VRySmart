using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;
using OpenCvSharp;
using UnityEngine.Rendering;
using PaperPlaneTools.AR;
using System;
using System.Linq;

public class OpenCVMat : MonoBehaviour
{
    Texture2D left;
    public Texture2D leftCPU;
    public Material leftEye;
    Texture2D right;
    int frameIndex;
    int timeIndex;
    Matrix4x4 poseLeft;
    Matrix4x4 poseRight;

    AsyncGPUReadbackRequest request;
    private bool initialized;

    private MyMarkerDetector markerDetector;
    public GameObject markerPrefab;

    double focalLengthLeft;
    double CxLeft;
    double CyLeft;

    void Update()
    {
        if(ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
        {
            if (!initialized)
            {
                markerDetector = new MyMarkerDetector();
                markerPrefab.transform.parent = FindObjectsOfType<ViveSR_TrackedCamera>().First(a => a.CameraIndex == DualCameraIndex.LEFT).transform;
                focalLengthLeft = ViveSR_DualCameraImageCapture.FocalLengthLeft;
                CxLeft = ViveSR_DualCameraImageCapture.UndistortedCxLeft;
                CyLeft = ViveSR_DualCameraImageCapture.UndistortedCyLeft;
                InitTextures();
                initialized = true;

            }


            if (initialized && request.done)
            {
                ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
                RenderTexture r = RenderTexture.GetTemporary(left.width, left.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(left, r);
                RenderTexture currentRT = RenderTexture.active;
                RenderTexture.active = r;
                leftCPU.ReadPixels(new UnityEngine.Rect(0, 0, r.width, r.height), 0, 0);
                leftCPU.Apply();
                RenderTexture.active = currentRT;
                RenderTexture.ReleaseTemporary(r);
                //request = AsyncGPUReadback.Request(left, 0, TextureFormat.RGBA32, OnCompleteReadback);



                Mat mat = OpenCvSharp.Unity.TextureToMat(leftCPU);
                mat = mat.Flip(FlipMode.X);
                ProcessFrame(mat, mat.Cols, mat.Rows);
                mat = mat.Flip(FlipMode.X);
                OpenCvSharp.Unity.MatToTexture(mat, leftCPU);
                mat.Dispose();
            }

            leftEye.mainTexture = leftCPU;
        }
    }

    void InitTextures()
    {
        left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
    }

	private void ProcessFrame(Mat mat, int width, int height)
	{
        List<int> markerIds = markerDetector.Detect(mat, width, height, focalLengthLeft, CxLeft, CyLeft);

        if(markerIds.Count > 0)
        {
            for (int i = 0; i < markerIds.Count; i++)
            {
                if (markerIds[i] == 0)
                {
                    StopAllCoroutines();
                    if(!markerPrefab.activeInHierarchy)
                        markerPrefab.SetActive(true);

                    Matrix4x4 transforMatrix = markerDetector.TransfromMatrixForIndex(i);
                    PositionObject(markerPrefab, transforMatrix);
                }
            }
        }
        else if(markerPrefab.activeInHierarchy)
        {
            StartCoroutine(WaitAndDisable());
        }

    }

    IEnumerator WaitAndDisable()
    {
        yield return new WaitForSeconds(0.3f);
        markerPrefab.SetActive(false);
    }

    private void PositionObject(GameObject markerPrefab, Matrix4x4 transformMatrix)
    {
        Matrix4x4 matrixY = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
        Matrix4x4 matrixZ = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
        Matrix4x4 matrix = matrixY * transformMatrix * matrixZ;

        markerPrefab.transform.localPosition = MatrixHelper.GetPosition(matrix);
        markerPrefab.transform.localRotation = MatrixHelper.GetQuaternion(matrix);
        markerPrefab.transform.localScale = MatrixHelper.GetScale(matrix);
    }
}
