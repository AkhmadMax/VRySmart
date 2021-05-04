//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Access to SteamVR system (hmd) and compositor (distort) interfaces.
//
//=============================================================================

using UnityEngine;
using Valve.VR;
using System.IO;
using System.Linq;

#if UNITY_2017_2_OR_NEWER
    using UnityEngine.XR;
#else
using XRSettings = UnityEngine.VR.VRSettings;
using XRDevice = UnityEngine.VR.VRDevice;
#endif

namespace Valve.VR
{
    public class SteamVR : System.IDisposable
    {
        // Use this to check if SteamVR is currently active without attempting
        // to activate it in the process.
        public static bool active { get { return _instance != null; } }

        // Set this to false to keep from auto-initializing when calling SteamVR.instance.
        private static bool _enabled = true;
        public static bool enabled
        {
            get
            {
                if (!XRSettings.enabled)
                    enabled = false;
                return _enabled;
            }
            set
            {
                _enabled = value;

                if (_enabled)
                {
                    Initialize();
                }
                else
                {
                    SafeDispose();
                }
            }
        }

        private static SteamVR _instance;
        public static SteamVR instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return null;
#endif
                if (!enabled)
                    return null;

                if (_instance == null)
                {
                    _instance = CreateInstance();

                    // If init failed, then auto-disable so scripts don't continue trying to re-initialize things.
                    if (_instance == null)
                        _enabled = false;
                }

                return _instance;
            }
        }

        public enum InitializedStates
        {
            None,
            Initializing,
            InitializeSuccess,
            InitializeFailure,
        }

        public static InitializedStates initializedState = InitializedStates.None;

        public static void Initialize(bool forceUnityVRMode = false)
        {
            if (_instance == null)
            {
                _instance = CreateInstance();
                if (_instance == null)
                    _enabled = false;
            }
        }

        public static bool usingNativeSupport
        {
            get { return XRDevice.GetNativePtr() != System.IntPtr.Zero; }
        }

        private static void ReportGeneralErrors()
        {
            string errorLog = "<b>[SteamVR]</b> Initialization failed. ";

            if (XRSettings.enabled == false)
                errorLog += "VR may be disabled in player settings. Go to player settings in the editor and check the 'Virtual Reality Supported' checkbox'. ";
            if (XRSettings.supportedDevices != null && XRSettings.supportedDevices.Length > 0)
            {
                if (XRSettings.supportedDevices.Contains("OpenVR") == false)
                    errorLog += "OpenVR is not in your list of supported virtual reality SDKs. Add it to the list in player settings. ";
                else if (XRSettings.supportedDevices.First().Contains("OpenVR") == false)
                    errorLog += "OpenVR is not first in your list of supported virtual reality SDKs. <b>This is okay, but if you have an Oculus device plugged in, and Oculus above OpenVR in this list, it will try and use the Oculus SDK instead of OpenVR.</b> ";
            }
            else
            {
                errorLog += "You have no SDKs in your Player Settings list of supported virtual reality SDKs. Add OpenVR to it. ";
            }

            errorLog += "To force OpenVR initialization call SteamVR.Initialize(true). ";

            Debug.LogWarning(errorLog);
        }

        private static SteamVR CreateInstance()
        {
            initializedState = InitializedStates.Initializing;

            try
            {
                var error = EVRInitError.None;
                if (!SteamVR.usingNativeSupport)
                {
                    ReportGeneralErrors();
                    initializedState = InitializedStates.InitializeFailure;
                    //SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                // Verify common interfaces are valid.

                OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    ReportGeneralErrors();
                    //SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    //SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVRInput_Version, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    //SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                //settings = SteamVR_Settings.instance;

                //if (Application.isEditor)
                //    IdentifyEditorApplication();

                //SteamVR_Input.IdentifyActionsFile();

//                if (SteamVR_Settings.instance.inputUpdateMode != SteamVR_UpdateModes.Nothing || SteamVR_Settings.instance.poseUpdateMode != SteamVR_UpdateModes.Nothing)
//                {
//                    SteamVR_Input.Initialize();

//#if UNITY_EDITOR
//                    if (SteamVR_Input.IsOpeningSetup())
//                        return null;
//#endif
//                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("<b>[SteamVR]</b> " + e);
                //SteamVR_Events.Initialized.Send(false);
                return null;
            }

            _enabled = true;
            initializedState = InitializedStates.InitializeSuccess;
            //SteamVR_Events.Initialized.Send(true);
            return new SteamVR();
        }

        static void ReportError(EVRInitError error)
        {
            switch (error)
            {
                case EVRInitError.None:
                    break;
                case EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime:
                    Debug.LogWarning("<b>[SteamVR]</b> Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
                    break;
                case EVRInitError.Init_VRClientDLLNotFound:
                    Debug.LogWarning("<b>[SteamVR]</b> Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
                    break;
                case EVRInitError.Driver_RuntimeOutOfDate:
                    Debug.LogWarning("<b>[SteamVR]</b> Initialization Failed!  Make sure device's runtime is up to date.");
                    break;
                default:
                    Debug.LogWarning("<b>[SteamVR]</b> " + OpenVR.GetStringForHmdError(error));
                    break;
            }
        }

        // native interfaces
        public CVRSystem hmd { get; private set; }
        public CVRCompositor compositor { get; private set; }
        public CVROverlay overlay { get; private set; }

        // tracking status
        static public bool initializing { get; private set; }
        static public bool calibrating { get; private set; }
        static public bool outOfRange { get; private set; }

        static public bool[] connected = new bool[OpenVR.k_unMaxTrackedDeviceCount];

        // render values
        public float sceneWidth { get; private set; }
        public float sceneHeight { get; private set; }
        public float aspect { get; private set; }
        public float fieldOfView { get; private set; }
        public Vector2 tanHalfFov { get; private set; }
        public VRTextureBounds_t[] textureBounds { get; private set; }
        public SteamVR_Utils.RigidTransform[] eyes { get; private set; }
        public ETextureType textureType;

        // hmd properties
        public string hmd_TrackingSystemName { get { return GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String); } }
        public string hmd_ModelNumber { get { return GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String); } }
        public string hmd_SerialNumber { get { return GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String); } }

        public float hmd_SecondsFromVsyncToPhotons { get { return GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float); } }
        public float hmd_DisplayFrequency { get { return GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float); } }

        public EDeviceActivityLevel GetHeadsetActivityLevel()
        {
            return OpenVR.System.GetTrackedDeviceActivityLevel(OpenVR.k_unTrackedDeviceIndex_Hmd);
        }

        public string GetTrackedDeviceString(uint deviceId)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capacity = hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0, ref error);
            if (capacity > 1)
            {
                var result = new System.Text.StringBuilder((int)capacity);
                hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, result, capacity, ref error);
                return result.ToString();
            }
            return null;
        }

        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var capactiy = hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            if (capactiy > 1)
            {
                var result = new System.Text.StringBuilder((int)capactiy);
                hmd.GetStringTrackedDeviceProperty(deviceId, prop, result, capactiy, ref error);
                return result.ToString();
            }
            return (error != ETrackedPropertyError.TrackedProp_Success) ? error.ToString() : "<unknown>";
        }

        public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            return hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref error);
        }


        private static bool runningTemporarySession = false;
       

        public static void ExitTemporarySession()
        {
            if (runningTemporarySession)
            {
                OpenVR.Shutdown();
                runningTemporarySession = false;
            }
        }

        public const string defaultUnityAppKeyTemplate = "application.generated.unity.{0}.exe";
        public const string defaultAppKeyTemplate = "application.generated.{0}";

        public static string GenerateAppKey()
        {
            string productName = GenerateCleanProductName();

            return string.Format(defaultUnityAppKeyTemplate, productName);
        }

        public static string GenerateCleanProductName()
        {
            string productName = Application.productName;
            if (string.IsNullOrEmpty(productName))
                productName = "unnamed_product";
            else
            {
                productName = System.Text.RegularExpressions.Regex.Replace(Application.productName, "[^\\w\\._]", "");
                productName = productName.ToLower();
            }

            return productName;
        }


        #region Event callbacks

        private void OnInitializing(bool initializing)
        {
            SteamVR.initializing = initializing;
        }

        private void OnCalibrating(bool calibrating)
        {
            SteamVR.calibrating = calibrating;
        }

        private void OnOutOfRange(bool outOfRange)
        {
            SteamVR.outOfRange = outOfRange;
        }

        private void OnDeviceConnected(int i, bool connected)
        {
            SteamVR.connected[i] = connected;
        }


        #endregion

        private SteamVR()
        {
            hmd = OpenVR.System;
            Debug.Log("<b>[SteamVR]</b> Initialized. Connected to " + hmd_TrackingSystemName + ":" + hmd_SerialNumber);

            compositor = OpenVR.Compositor;
            overlay = OpenVR.Overlay;

            // Setup render values
            uint w = 0, h = 0;
            hmd.GetRecommendedRenderTargetSize(ref w, ref h);
            sceneWidth = (float)w;
            sceneHeight = (float)h;

            float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

            float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

            tanHalfFov = new Vector2(
                Mathf.Max(-l_left, l_right, -r_left, r_right),
                Mathf.Max(-l_top, l_bottom, -r_top, r_bottom));

            textureBounds = new VRTextureBounds_t[2];

            textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.x;
            textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.x;
            textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.y;
            textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.y;

            textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.x;
            textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.x;
            textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.y;
            textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.y;

            // Grow the recommended size to account for the overlapping fov
            sceneWidth = sceneWidth / Mathf.Max(textureBounds[0].uMax - textureBounds[0].uMin, textureBounds[1].uMax - textureBounds[1].uMin);
            sceneHeight = sceneHeight / Mathf.Max(textureBounds[0].vMax - textureBounds[0].vMin, textureBounds[1].vMax - textureBounds[1].vMin);

            aspect = tanHalfFov.x / tanHalfFov.y;
            fieldOfView = 2.0f * Mathf.Atan(tanHalfFov.y) * Mathf.Rad2Deg;

            eyes = new SteamVR_Utils.RigidTransform[] {
            new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
            new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right)) };

            switch (SystemInfo.graphicsDeviceType)
            {
#if (UNITY_5_4)
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGL2:
#endif
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                    textureType = ETextureType.OpenGL;
                    break;
#if !(UNITY_5_4)
			case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
				textureType = ETextureType.Vulkan;
				break;
#endif
                default:
                    textureType = ETextureType.DirectX;
                    break;
            }
        }

        ~SteamVR()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _instance = null;
        }

        // Use this interface to avoid accidentally creating the instance in the process of attempting to dispose of it.
        public static void SafeDispose()
        {
            if (_instance != null)
                _instance.Dispose();
        }
    }
}