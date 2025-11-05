using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARObjectPlacer : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;

    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;

    private static List<ARRaycastHit> hits = new();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        // Only act on new touch input
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // Raycast from touch to detected planes
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            TrackableId planeId = hits[0].trackableId;

            // Get the plane hit
            ARPlane hitPlane = planeManager.GetPlane(planeId);

            // Attach anchor to the plane at hit position
            ARAnchor anchor = anchorManager.AttachAnchor(hitPlane, hitPose);
            if (anchor == null)
            {
                Debug.LogWarning("Failed to create anchor.");
                return;
            }

            // Instantiate the object and parent it to the anchor
            Instantiate(objectPrefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
        }
    }
}
