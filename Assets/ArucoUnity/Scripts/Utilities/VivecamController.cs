using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;
using OpenCvSharp;
using PaperPlaneTools.AR;
using UnityEngine.Rendering;


namespace ArucoUnity.Cameras
{
    /// <summary>
    /// Get images from multiple webcams. Based on: http://answers.unity3d.com/answers/1155328/view.html
    /// </summary>
    public class VivecamController : MonoBehaviour
    {
        // Events

        /// <summary>
        /// Called when the webcams started.
        /// </summary>
        public Action<VivecamController> Started = delegate { };

        // Properties

        /// <summary>
        /// Gets or sets the ids of the webcams to use.
        /// </summary>
        public List<int> Ids { get; private set; }

        /// <summary>
        /// Gets the textures of the used webcams.
        /// </summary>
        public WebCamTexture Texture { get; private set; }

        AsyncGPUReadbackRequest request;
        RenderTexture r;



        /// <summary>
        /// Gets <see cref="Textures"/> converted in Texture2D.
        /// </summary>
        public Texture2D Textures2D
        {
            get
            {

                ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
                r = RenderTexture.GetTemporary(left.width, left.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(left, r);
                request = AsyncGPUReadback.Request(r, 0, TextureFormat.RGBA32, OnCompleteReadback);

                textures2D.SetPixels32(leftCPU.GetPixels32());
                textures2D.Apply();
                //Graphics.CopyTexture(leftCPU, textures2D);
                return textures2D;
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

            //var tex = new Texture2D(left.width, left.height, TextureFormat.RGBA32, false);
            leftCPU.LoadRawTextureData(request.GetData<uint>());
            leftCPU.Apply();

            //Mat mat = OpenCvSharp.Unity.TextureToMat(tex);
            //OpenCvSharp.Unity.MatToTexture(mat, leftCPU);
            //mat.Dispose();
            //Destroy(tex);
        }


        /// <summary>
        /// Gets or sets the format of <see cref="Textures2D"/>, by default <see cref="TextureFormat.RGB24"/>.
        /// </summary>
        public TextureFormat Textures2DFormat { get { return textures2DFormat; } set { textures2DFormat = value; } }

        /// <summary>
        /// Gets if the controller is configured.
        /// </summary>
        public bool IsConfigured { get; private set; }

        /// <summary>
        /// Gets if the webcams started.
        /// </summary>
        public bool IsStarted { get; private set; }

        // Variables

        protected bool starting = false;
        private Texture2D textures2D;// = new Texture2D();
        private TextureFormat textures2DFormat = TextureFormat.RGB24;

        // MonoBehaviour methods

        /// <summary>
        /// Initializes the properties.
        /// </summary>
        protected void Awake()
        {
            IsStarted = false;
            IsConfigured = false;

            Ids = new List<int>();
            Texture = new WebCamTexture();
        }

        // Methods

        /// <summary>
        /// Configures <see cref="Devices"/> and <see cref="Textures"/> from <see cref="Ids"/>.
        /// </summary>
        public void Configure()
        {
            Debug.Log("Configure");

            IsStarted = false;
            IsConfigured = true;

            //Destroy(Texture);
            //Destroy(Textures2D);


            //foreach (var webcamId in Ids)
            //{
            //  var webcamDevice = WebCamTexture.devices[webcamId];
            //  Devices.Add(webcamDevice);
            //  Textures.Add(new WebCamTexture(webcamDevice.name));
            //}
        }

        Texture2D left;
        public Texture2D leftCPU;
        public Material leftEye;
        Texture2D right;

        void InitTextures()
        {
            left = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
            leftCPU = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
            right = new Texture2D(ViveSR_DualCameraImageCapture.UndistortedImageWidth, ViveSR_DualCameraImageCapture.UndistortedImageHeight, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// Starts the webcams.
        /// </summary>
        public void StartWebcams()
        {
            if (!IsConfigured || starting || IsStarted)
            {
                throw new Exception("Configure the controller, wait the webcams to start or stop the controller.");
            }
            StartCoroutine(StartingAsync());
        }

        /// <summary>
        /// Stops the webcams.
        /// </summary>
        public void StopWebcams()
        {
            if (!IsConfigured || !IsStarted)
            {
                throw new Exception("Configure the controller and start the controller.");
            }

            IsStarted = false;
            if (starting)
            {
                StopCoroutine(StartingAsync());
            }
        }

        int frameIndex;
        int timeIndex;
        Matrix4x4 poseLeft;
        Matrix4x4 poseRight;

        // Methods

        /// <summary>
        /// Waits for Unity to start the webcams to set <see cref="Textures2D"/>, <see cref="Textures"/> and call
        /// <see cref="ConfigurableController.OnStarted"/>.
        /// </summary>
        protected IEnumerator StartingAsync()
        {
            Debug.Log("StartingAsync");


            starting = true;

            bool vivecamStarted;
            do
            {
                vivecamStarted = true;

                vivecamStarted &= ViveSR.FrameworkStatus == FrameworkStatus.WORKING;

                ViveSR_DualCameraImageCapture.GetUndistortedTexture(out left, out right, out frameIndex, out timeIndex, out poseLeft, out poseRight);
                //vivecamStarted &= left.width > 100;
                vivecamStarted &= left != null;
                
                if(left != null)
                    Debug.Log(left.width);

                if(ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
                {
                    InitTextures();
                }

                if (vivecamStarted)
                {
                    r = RenderTexture.GetTemporary(left.width, left.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(left, r);
                    request = AsyncGPUReadback.Request(r, 0, TextureFormat.RGBA32, OnCompleteReadback);
                    Debug.Log(left.width);
                    yield return new WaitForSeconds(0.5f);
                    Debug.Log(leftCPU.width);

                    textures2D = new Texture2D(left.width, left.height, Textures2DFormat, false);
                    starting = false;
                    IsStarted = true;
                    Debug.Log("Started");
                    Started(this);
                }
                else
                {
                    yield return null;
                }
            }
            while (!vivecamStarted);

            starting = false;
        }
    }
}