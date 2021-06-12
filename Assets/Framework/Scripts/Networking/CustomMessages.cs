using UnityEngine.Networking;
using UnityEngine;
public class CustomMsgType
{
    public static short Touches = MsgType.Highest + 1;
    public static short RegNumCmd = MsgType.Highest + 2;
    public static short SendNumCmd = MsgType.Highest + 3;
    public static short Calibration = MsgType.Highest + 4;
}
