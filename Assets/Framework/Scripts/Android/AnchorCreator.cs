using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class AnchorCreator : MonoBehaviour
{
    [SerializeField]
    GameObject m_Prefab;

    public GameObject prefab
    {
        get => m_Prefab;
        set => m_Prefab = value;
    }

    public ARAnchor lastAnchor
    {
        get => m_Anchors[m_Anchors.Count - 1];
    }

    public void RemoveAllAnchors()
    {
        Debug.Log($"Removing all anchors ({m_Anchors.Count})");
        foreach (var anchor in m_Anchors)
        {
            Destroy(anchor.gameObject);
        }
        m_Anchors.Clear();
    }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
    }

    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor = null;

        // If we hit a plane, try to "attach" the anchor to the plane
        if (hit.trackable is ARPlane plane)
        {
            var planeManager = GetComponent<ARPlaneManager>();
            if (planeManager)
            {
                Debug.Log("Creating anchor attachment.");
                var oldPrefab = m_AnchorManager.anchorPrefab;
                m_AnchorManager.anchorPrefab = prefab;
                anchor = m_AnchorManager.AttachAnchor(plane, hit.pose);
                m_AnchorManager.anchorPrefab = oldPrefab;
                //(anchor, $"Attached to plane {plane.trackableId}");
                return anchor;
            }
        }

        // Otherwise, just create a regular anchor at the hit pose
        Debug.Log("Creating regular anchor.");

        // Note: the anchor can be anywhere in the scene hierarchy
        var gameObject = Instantiate(prefab, hit.pose.position, hit.pose.rotation);

        // Make sure the new GameObject has an ARAnchor component
        anchor = gameObject.GetComponent<ARAnchor>();
        if (anchor == null)
        {
            anchor = gameObject.AddComponent<ARAnchor>();
        }

        //SetAnchorText(anchor, $"Anchor (from {hit.hitType})");

        return anchor;
    }

    void Update()
    {
        if (m_Anchors.Count > 0)
            return;

        if (Input.touchCount == 0)
            return;

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // Raycast against planes and feature points
        const TrackableType trackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        // Perform the raycast
        if (m_RaycastManager.Raycast(touch.position, s_Hits, trackableTypes))
        {
            // Remove previous anchor
            //RemoveAllAnchors();

            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            var hit = s_Hits[0];

            // Create a new anchor
            var anchor = CreateAnchor(hit);
            if (anchor)
            {
                // Remember the anchor so we can remove it later.
                m_Anchors.Add(anchor);
            }
            else
            {
                Debug.Log("Error creating anchor");
            }
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    List<ARAnchor> m_Anchors = new List<ARAnchor>();

    ARRaycastManager m_RaycastManager;

    ARAnchorManager m_AnchorManager;
}