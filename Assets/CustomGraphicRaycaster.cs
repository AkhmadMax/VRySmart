using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomGraphicRaycaster : MonoBehaviour
{
    public Camera cam;
    private GameObject currentGameObject;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    public void ProcessTouchMsg(TouchMessage touchMsg)
    {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = new Vector2(cam.pixelWidth * touchMsg.position.x, cam.pixelHeight * touchMsg.position.y);

            EventSystem.current.RaycastAll(eventData, raycastResults);
            eventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);
            
            currentGameObject = eventData.pointerCurrentRaycast.gameObject;
            Debug.Log(currentGameObject);
            //raycastResults.Clear();

            if (currentGameObject)
            {
                if (touchMsg.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    ProcessTouchBegan(eventData);
                }

                if (touchMsg.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    ProcessTouchEnded(eventData);
                }
            }
    }

    private RaycastResult FindFirstRaycast(List<RaycastResult> results)
    {
        foreach(RaycastResult result in results)
        {
            if (!result.gameObject)
                continue;
            return result;
        }

        return new RaycastResult();
    }

    private void ProcessTouchBegan(PointerEventData eventData)
    {
        Debug.Log("ProcessTouchBegan");
        eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(currentGameObject, eventData, ExecuteEvents.pointerDownHandler);

        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGameObject);
        }

        eventData.pressPosition = eventData.position;
        eventData.pointerPress = newPointerPress;
    }

    private void ProcessTouchEnded(PointerEventData eventData)
    {
        Debug.Log("ProcessTouchBegan");
        eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(currentGameObject, eventData, ExecuteEvents.pointerUpHandler);

        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGameObject);
        }

        eventData.pressPosition = eventData.position;
        eventData.pointerPress = newPointerPress;
    }
}
