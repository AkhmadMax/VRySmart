namespace PaperPlaneTools.AR 
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;
	using OpenCvSharp;

	using Valve.VR;

	// Many ideas are taken from http://answers.unity3d.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html#answer-1155328

	/// <summary>
	/// Base WebCamera class that takes care about video capturing.
	/// Is intended to be sub-classed and partially overridden to get
	/// desired behavior in the user Unity script
	/// </summary>
	public abstract class ViveCamera: MonoBehaviour
	{
		/// <summary>
		/// Target surface to render WebCam stream
		/// </summary>
		public GameObject Surface;

		protected Nullable<WebCamDevice> webCamDevice = null;
		protected WebCamTexture webCamTexture = null;
		protected Texture2D renderedTexture = null;

		/// <summary>
		/// A kind of workaround for macOS issue: MacBook doesn't state it's webcam as frontal
		/// </summary>
		protected bool forceFrontalCamera = false;
		protected bool preferRearCamera = false;

		/// <summary>
		/// WebCam texture parameters to compensate rotations, flips etc.
		/// </summary>
		protected Unity.TextureConversionParams TextureParameters { get; private set; }

		/// <summary>
		/// This method scans source device params (flip, rotation, front-camera status etc.) and
		/// prepares TextureConversionParameters that will compensate all that stuff for OpenCV
		/// </summary>
		private void ReadTextureConversionParameters()
		{
			Unity.TextureConversionParams parameters = new Unity.TextureConversionParams();

			// frontal camera - we must flip around Y axis to make it mirror-like
			parameters.FlipHorizontally = forceFrontalCamera || webCamDevice.Value.isFrontFacing;
			
			// TODO:
			// actually, code below should work, however, on our devices tests every device except iPad
			// returned "false", iPad said "true" but the texture wasn't actually flipped

			// compensate vertical flip
			//parameters.FlipVertically = webCamTexture.videoVerticallyMirrored;
			
			// deal with rotation
			if (0 != webCamTexture.videoRotationAngle)
				parameters.RotationAngle = webCamTexture.videoRotationAngle; // cw -> ccw

			// apply
			TextureParameters = parameters;

			//UnityEngine.Debug.Log (string.Format("front = {0}, vertMirrored = {1}, angle = {2}", webCamDevice.isFrontFacing, webCamTexture.videoVerticallyMirrored, webCamTexture.videoRotationAngle));
		}

		//		protected virtual void Awake()
		//		{
		//		
		//			text.text = "Awake";
		//			int cameraIndex = -1;
		//			for (int i = 0; i < WebCamTexture.devices.Length; i++) {
		//				WebCamDevice webCamDevice = WebCamTexture.devices [i];
		//				if (webCamDevice.isFrontFacing == false) {
		//					cameraIndex = i;
		//					break;
		//				}
		//				if (cameraIndex < 0) {
		//					cameraIndex = i;
		//				}
		//			}
		//
		//			if (cameraIndex >= 0) {
		//				DeviceName = WebCamTexture.devices [cameraIndex].name;
		//				//webCamDevice = WebCamTexture.devices [cameraIndex];
		//			}
		//		}


		public bool undistorted = true;

		private void OnEnable()
		{
			// The video stream must be symmetrically acquired and released in
			// order to properly disable the stream once there are no consumers.
			SteamVR_TrackedCamera.VideoStreamTexture source = SteamVR_TrackedCamera.Source(undistorted);
			source.Acquire();

			// Auto-disable if no camera is present.
			if (!source.hasCamera)
				enabled = false;
		}

		private void OnDisable()
		{
			// The video stream must be symmetrically acquired and released in
			// order to properly disable the stream once there are no consumers.
			SteamVR_TrackedCamera.VideoStreamTexture source = SteamVR_TrackedCamera.Source(undistorted);
			source.Release();
		}


		void OnDestroy() 
		{
			if (webCamTexture != null)
			{
				if (webCamTexture.isPlaying)
				{
					webCamTexture.Stop();
				}
				webCamTexture = null;
			}

			if (webCamDevice != null) 
			{
				webCamDevice = null;
			}
		}

		/// <summary>
		/// Updates web camera texture
		/// </summary>
		private void Update ()
		{
			SteamVR_TrackedCamera.VideoStreamTexture source = SteamVR_TrackedCamera.Source(undistorted);
			Texture2D srcTexture = source.texture;

			if (srcTexture != null)
			{
				Texture2D cpuTexture = new Texture2D(srcTexture.width / 2, srcTexture.height / 4, TextureFormat.RGBA32, false, false);

				// this must be called continuously
				//ReadTextureConversionParameters();

				// This block with RenderTexture is meant to transfer Texture2D from GPU memory to CPU
				RenderTexture currentRT = RenderTexture.active;
				RenderTexture renderTexture = new RenderTexture(srcTexture.width / 2, srcTexture.height / 4, 32);
				Graphics.Blit(srcTexture, renderTexture, new Vector2(0.5f, -0.25f), new Vector2(0.25f, -0.125f));
				RenderTexture.active = renderTexture;
				cpuTexture.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
				cpuTexture.Apply();
				RenderTexture.active = currentRT;


				// process texture with whatever method sub-class might have in mind
				if (ProcessTexture(cpuTexture, ref renderedTexture))
				{
					RenderFrame();
				}

				Destroy(cpuTexture);
				Destroy(renderTexture);
			}

		}

		/// <summary>
		/// Processes current texture
		/// This function is intended to be overridden by sub-classes
		/// </summary>
		/// <param name="input">Input WebCamTexture object</param>
		/// <param name="output">Output Texture2D object</param>
		/// <returns>True if anything has been processed, false if output didn't change</returns>
		protected abstract bool ProcessTexture(Texture2D input, ref Texture2D output);

		/// <summary>
		/// Renders frame onto the surface
		/// </summary>
		private void RenderFrame()
		{
			if (renderedTexture != null)
			{
				// apply
				Surface.GetComponent<RawImage>().texture = renderedTexture;

				// Adjust image ration according to the texture sizes 
				Surface.GetComponent<RectTransform>().sizeDelta = new Vector2(renderedTexture.width, renderedTexture.height);
			}
		}
	}
}