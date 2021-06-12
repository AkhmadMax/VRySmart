using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TouchMessages : NetworkBehaviour
{
    public class TouchMessage : MessageBase
    {
        private Touch[] touches;

        public TouchMessage(Touch[] touches)
        {
            this.touches = touches;
        }
    }

    //[ClientCallback]
    //void Update()
    //{
    //    if(Input.touchCount > 0)
    //    {
    //        CmdSendTouches(new TouchMessage(Input.touches));
    //    }
    //}

    //[Command]
    //public void CmdSendTouches(MessageBase msg)
    //{
    //    NetworkServer.SendToAll(CustomMsgType.Touches, msg);
    //}
}
