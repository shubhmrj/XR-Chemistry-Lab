using UnityEngine;
using ManoMotion;

public class HandGestureInteraction : MonoBehaviour
{
    [SerializeField] private Camera arCamera; // Assign your AR camera here
    [SerializeField] private float grabDistance = 0.5f; // Distance from camera to place held object
    [SerializeField] private LayerMask grabbableLayer; // Layer for grabbable objects

    private GameObject grabbedObject = null;
    private float moveSpeed = 10f;

    void Start()
    {
        ManoMotionManager.Instance.ShouldCalculateGestures(true);
    }

    void Update()
    {
        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
            return;

        foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
        {
            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
                continue;

            Vector3 screenPosition = new Vector3(
                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2f,
                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2f,
                0
            );

            Vector3 worldPosition = arCamera.ViewportToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, grabDistance));

            switch (handInfo.gestureInfo.manoGestureContinuous)
            {
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    if (grabbedObject == null)
                    {
                        Ray ray = arCamera.ViewportPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
                        if (Physics.Raycast(ray, out RaycastHit hit, 2f, grabbableLayer))
                        {
                            grabbedObject = hit.collider.gameObject;
                        }
                    }

                    if (grabbedObject != null)
                    {
                        grabbedObject.transform.position = Vector3.Lerp(grabbedObject.transform.position, worldPosition, Time.deltaTime * moveSpeed);
                    }
                    break;

                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    if (grabbedObject != null)
                    {
                        grabbedObject = null;
                    }
                    break;
            }
        }
    }
}