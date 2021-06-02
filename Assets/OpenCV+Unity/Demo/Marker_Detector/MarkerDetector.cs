namespace OpenCvSharp.Demo {

	using UnityEngine;
	using System.Collections;
	using UnityEngine.UI;
	using Aruco;
	using Valve.VR.Extras;
	using Valve.VR;

	public class MarkerDetector : MonoBehaviour {

		public Material leftEye;
		public Texture2D texture;
		public Texture2D outputTexture;

		DetectorParameters detectorParameters;
		Dictionary dictionary;
        private bool initialized;

		void Start () {
			// Create default parameres for detection
			detectorParameters = DetectorParameters.Create();

			// Dictionary holds set of all available markers
			dictionary = CvAruco.GetPredefinedDictionary (PredefinedDictionaryName.Dict6X6_250);
		}

		private void Update()
        {
			if (leftEye.mainTexture != null && !initialized)
            {
				texture = new Texture2D(leftEye.mainTexture.width, leftEye.mainTexture.height / 2, TextureFormat.RGBA32, false, false);
				outputTexture = new Texture2D(leftEye.mainTexture.width, leftEye.mainTexture.height / 2, TextureFormat.RGBA32, false, false);
				initialized = true;
			}

			if (!initialized || !leftEye.mainTexture)
				return;

			// This block with RenderTexture is meant to transfer Texture2D from GPU memory to CPU
			RenderTexture currentRT = RenderTexture.active;
			RenderTexture renderTexture = new RenderTexture(leftEye.mainTexture.width, leftEye.mainTexture.height, 32);
			Graphics.Blit(leftEye.mainTexture, renderTexture);
			RenderTexture.active = renderTexture;
			texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height / 2), 0, 0);
			texture.Apply();
			RenderTexture.active = currentRT;

			// Variables to hold results
			Point2f[][] corners;
			int[] ids;
			Point2f[][] rejectedImgPoints;

			// Create Opencv image from unity texture
			Mat mat = Unity.TextureToMat(texture);

			// Convert image to grasyscale
			Mat grayMat = new Mat();
			Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
			
			// Detect and draw markers
			CvAruco.DetectMarkers(grayMat, dictionary, out corners, out ids, detectorParameters, out rejectedImgPoints);
			CvAruco.DrawDetectedMarkers(mat, corners, ids);

            // Create Unity output texture with detected markers
            outputTexture = Unity.MatToTexture(mat);

			//// Set texture to see the result
			////RawImage rawImage = gameObject.GetComponent<RawImage>();
			////rawImage.texture = outputTexture;
			leftEye.mainTexture = outputTexture;

            //grayMat.Dispose();
            mat.Dispose();
			StartCoroutine(WaitAndDestroy());
            Destroy(renderTexture);
		}

		IEnumerator WaitAndDestroy()
        {
			yield return new WaitForEndOfFrame();
			Destroy(outputTexture);
		}

	}
}