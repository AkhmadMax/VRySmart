using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceProxy : Device
{
    public override float GetHideDuration()
    {
        return 0;
    }

    public override float GetShowDuration()
    {
        return 0;
    }

    public override void ToggleOff()
    {
        gameObject.SetActive(false);
    }

    public override void ToggleOn()
    {
        gameObject.SetActive(true);
    }
}
