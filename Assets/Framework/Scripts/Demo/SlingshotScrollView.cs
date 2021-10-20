using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class OnSlingshotEvent : UnityEvent<float> { }


public class SlingshotScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public OnSlingshotEvent onSlingshotStartEvent;
    public OnSlingshotEvent onSlingshotMoveEvent;
    public OnSlingshotEvent onSlingshotShotEvent;

    ScrollRect scrollrect;


    private void Awake()
    {
        scrollrect = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollrect.movementType = ScrollRect.MovementType.Unrestricted;
        if (onSlingshotStartEvent != null)
            onSlingshotStartEvent.Invoke(0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        scrollrect.movementType = ScrollRect.MovementType.Elastic;
        float force = Mathf.Abs(scrollrect.content.anchoredPosition.y);
        float forceNorm = force / scrollrect.viewport.rect.height;

        if (onSlingshotShotEvent != null)
            onSlingshotShotEvent.Invoke(forceNorm);
        Debug.Log(force);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float force = Mathf.Abs(scrollrect.content.anchoredPosition.y);
        float forceNorm = force / scrollrect.viewport.rect.height;
        if (onSlingshotMoveEvent != null)
            onSlingshotMoveEvent.Invoke(forceNorm);
    }
}
