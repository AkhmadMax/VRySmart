using System;
using UnityEngine;

namespace ArucoUnity.Cameras
{
    /// <summary>
    /// Captures images of a webcam.
    /// </summary>
    public class ArucoVivecam : ArucoCamera
    {
        // Constants

        protected const int cameraId = 0;

        // Editor fields

        [SerializeField]
        [Tooltip("The id of the webcam to use.")]
        private int webcamId;

        // IArucoCamera properties

        public override int CameraNumber { get { return 1; } }

        public override string Name { get; protected set; }

        // Properties

        /// <summary>
        /// Gets or set the id of the webcam to use.
        /// </summary>
        public int WebcamId { get { return webcamId; } set { webcamId = value; } }

        /// <summary>
        /// Gets the controller of the webcam to use.
        /// </summary>
        public VivecamController VivecamController { get; private set; }

        // MonoBehaviour methods

        /// <summary>
        /// Initializes <see cref="VivecamController"/> and subscribes to.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            VivecamController = gameObject.AddComponent<VivecamController>();
            VivecamController.Started += VivecamController_Started;
        }

        /// <summary>
        /// Unsubscribes to <see cref="VivecamController"/>.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            VivecamController.Started -= VivecamController_Started;
        }

        // ConfigurableController methods

        /// <summary>
        /// Calls <see cref="WebcamController.Configure"/> and sets <see cref="Name"/>.
        /// </summary>
        protected override void Configuring()
        {
            base.Configuring();

            VivecamController.Ids.Clear();
            VivecamController.Ids.Add(WebcamId);
            VivecamController.Configure();

            Name = "Vive";
        }

        /// <summary>
        /// Calls <see cref="WebcamController.StartWebcams"/>.
        /// </summary>
        protected override void Starting()
        {
            base.Starting();
            VivecamController.StartWebcams();
        }

        /// <summary>
        /// Calls <see cref="WebcamController.StopWebcams"/>.
        /// </summary>
        protected override void Stopping()
        {
            base.Stopping();
            VivecamController.StopWebcams();
        }

        /// <summary>
        /// Blocks <see cref="ArucoCamera.OnStarted"/> until <see cref="WebcamController.IsStarted"/>.
        /// </summary>
        protected override void OnStarted()
        {
        }

        // ArucoCamera methods

        /// <summary>
        /// Copy current webcam images to <see cref="ArucoCamera.NextImages"/>.
        /// </summary>
        protected override bool UpdatingImages()
        {
            Array.Copy(VivecamController.Textures2D.GetRawTextureData(), NextImageDatas[cameraId], ImageDataSizes[cameraId]);
            return true;
        }

        // Methods

        /// <summary>
        /// Configures <see cref="ArucoCamera.Textures"/> and calls <see cref="ArucoCamera.OnStarted"/>.
        /// </summary>
        protected virtual void VivecamController_Started(VivecamController webcamController)
        {
            var webcamTexture = VivecamController.Textures2D;
            Textures[cameraId] = new Texture2D(webcamTexture.width, webcamTexture.height, webcamTexture.format, false);
            base.OnStarted();
            Debug.Log("Started");

        }
    }
}