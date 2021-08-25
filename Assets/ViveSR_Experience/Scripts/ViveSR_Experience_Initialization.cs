using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Initialization : MonoBehaviour
    {
        public UnityEvent postInitEvent = new UnityEvent();
        ViveSR_Experience_ActionSequence actionSequence;

        void Awake()
        {
            CheckBasicStatus();
        }

        void CheckBasicStatus()
        {
            actionSequence = ViveSR_Experience_ActionSequence.CreateActionSequence(gameObject);
            actionSequence.AddAction(() => CheckViveSRStatus(actionSequence.ActionFinished));
            actionSequence.AddAction(() => Play());
            actionSequence.StartSequence();
        }

        void CheckViveSRStatus(Action done)
        {
            StartCoroutine(_CheckViveSRStatus(done));
        }

        IEnumerator _CheckViveSRStatus(Action done = null)
        {
            float initTime = Time.timeSinceLevelLoad;

            while (ViveSR.FrameworkStatus == FrameworkStatus.INITIAL || ViveSR.FrameworkStatus == FrameworkStatus.START)
            {
                if (Time.timeSinceLevelLoad - initTime > 10f)
                {
                    string errorMsg = "SRWorks initialization timeout.\nPlease quit the game, restart the SRWorks runtime, and try again.";
                    Debug.Log("[ViveSR Experience] " + errorMsg);
                    actionSequence.StopSequence();
                    ViveSR_DualCameraRig.Instance.SetMode(DualCameraDisplayMode.VIRTUAL);   // Set to virtual mode to show the error panel.
                }

                Debug.Log("[ViveSR Experience] Waiting for ViveSR");
                yield return new WaitForEndOfFrame();
            }

            if (ViveSR.FrameworkStatus == FrameworkStatus.ERROR || ViveSR.FrameworkStatus == FrameworkStatus.STOP)
            {
                string errorMsg = string.Format("SRWorks initialization failed on {0}: {1}\nPlease quit the game, restart the SRWorks runtime, and try again.",
                    ViveSR.InitialError.FailedModule.ToString(), ViveSR.InitialError.ErrorCode);
                Debug.Log("[ViveSR Experience] " + errorMsg);
                actionSequence.StopSequence();
                ViveSR_DualCameraRig.Instance.SetMode(DualCameraDisplayMode.VIRTUAL);   // Set to virtual mode to show the error panel.
            }
            else if (ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
            {
                ModuleStatus moduleStatus = ModuleStatus.IDLE;

                int result = SRWorkModule_API.GetStatus(ModuleType.RIGIDRECONSTRUCTION, out moduleStatus);

                if (done != null) done();
            }
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }


        void Play()
        {
            FindObjectOfType<Sample10_HumanCut>().Init();
            //postInitEvent.Invoke();
        }
    }
}