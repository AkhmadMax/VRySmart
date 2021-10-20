using UnityEngine.Networking;
using UnityEngine;
public class CustomMsgType
{
    public static short Touches = MsgType.Highest + 1;
    public static short RegNumCmd = MsgType.Highest + 2;
    public static short SendNumCmd = MsgType.Highest + 3;
    public static short Calibration = MsgType.Highest + 4;
    public static short Camera = MsgType.Highest + 5;
    public static short Touch = MsgType.Highest + 6;
    public static short Haptic = MsgType.Highest + 7;
    public static short TransmissionFinished = MsgType.Highest + 8;
}

public class TouchMessage : MessageBase
{
    public int touchId;
    public Vector2 position;
    public UnityEngine.InputSystem.TouchPhase phase;
}
