using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Networking;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


public class TouchNetworkBehaviour : NetworkBehaviour
{
    Touchscreen m_Touchscreen;


    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        NetworkServer.RegisterHandler(CustomMsgType.Touch, OnTouchMsg);
        m_Touchscreen = InputSystem.AddDevice<Touchscreen>();
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient");
        EnhancedTouchSupport.Enable();
    }

    Rect phoneRes = new Rect(0, 0, 1080, 2160);

    [Server]
    private void OnTouchMsg(NetworkMessage netMsg)
    {
        TouchMessage msg = netMsg.ReadMessage<TouchMessage>();
        InputSystem.QueueStateEvent(m_Touchscreen, new TouchState
        {
            touchId = msg.touchId,
            phase = msg.phase,
            position = new Vector2(msg.position.x * (phoneRes.width / 2), msg.position.y * (phoneRes.height / 2))
        });
    }

    [ClientCallback]
    void Update()
    {
        foreach(Touch touch in Touch.activeTouches)
        {
            TouchMessage msg = new TouchMessage();
            msg.touchId = touch.touchId;
            msg.position = new Vector2(touch.screenPosition.x / Screen.width, touch.screenPosition.y / Screen.height);
            msg.phase = touch.phase;
            NetworkManager.singleton.client.Send(CustomMsgType.Touch, msg);
        }
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    IntegerMessage msg = new IntegerMessage(250);
    //    NetworkManager.singleton.client.Send(CustomMsgType.Camera, msg);
    //    Debug.Log("OnBeginDrag");
    //}

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    IntegerMessage msg = new IntegerMessage(300);
    //    NetworkManager.singleton.client.Send(CustomMsgType.Camera, msg);
    //    Debug.Log("OnPointerClick");
    //}
}
