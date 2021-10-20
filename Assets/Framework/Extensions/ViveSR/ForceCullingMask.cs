using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRWorks automatically sets its defaul culling setting for both cameras. This scripts forces user selected culling setting
/// </summary>
public class ForceCullingMask : MonoBehaviour
{
    public LayerMask layerMask;
    public LayerMask screenRecordingMask;

    bool recordingModeActive;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        GetComponent<Camera>().cullingMask = layerMask;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            ToggleScreenRecordingMode();
        }
    }

    void ToggleScreenRecordingMode()
    {
        if (recordingModeActive)
            DisableScreenRecordingMode();
        else
            EnableScreenRecordingMode();
    }

    void EnableScreenRecordingMode()
    {
        GetComponent<Camera>().cullingMask = screenRecordingMask;
        recordingModeActive = true;
    }

    void DisableScreenRecordingMode()
    {
        GetComponent<Camera>().cullingMask = layerMask;
        recordingModeActive = false;
    }
}
