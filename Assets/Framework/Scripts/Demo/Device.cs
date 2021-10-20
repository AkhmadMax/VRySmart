using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Device : MonoBehaviour
{
    public abstract float GetHideDuration();
    public abstract float GetShowDuration();
    public abstract void ToggleOn();
    public abstract void ToggleOff();
}
