using UnityEngine;
using ManoMotion;

public class GrabAnyObject : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float grabDistance = 10f;
    [SerializeField] private LayerMask UI;

    private GameObject grabbedObject = null;

    void Start()
    {
        ManoMotionManager.Instance.ShouldCalculateGestures(true);
    }

    void Update()
    {
        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
        for (int i = 0; i < handInfos.Length; i++)
        {
            HandInfo handInfo = handInfos[i];
            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
                continue;

            Vector3 handScreenPosition = new Vector3(
                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
                handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
                0
            );

            // Convert hand screen position to world position using AR camera
            Camera mainCam = Camera.main;
            Vector3 handWorldPosition = mainCam.ScreenToWorldPoint(new Vector3(handScreenPosition.x, handScreenPosition.y, grabDistance));

            switch (handInfo.gestureInfo.manoGestureContinuous)
            {
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    if (grabbedObject == null)
                    {
                        // Try to grab an object
                        Ray ray = mainCam.ScreenPointToRay(handScreenPosition);
                        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, UI))
                        {
                            grabbedObject = hit.collider.gameObject;
                        }
                    }
                    else
                    {
                        // Move grabbed object
                        grabbedObject.transform.position = Vector3.Lerp(grabbedObject.transform.position, handWorldPosition, moveSpeed * Time.deltaTime);
                    }
                    break;

                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    // Release the object if any
                    grabbedObject = null;
                    break;

                case ManoGestureContinuous.HOLD_GESTURE:
                    if (grabbedObject != null)
                    {
                        // Optionally scale the object based on hand position or gesture
                        float scale = 1 + handScreenPosition.y / Screen.height;
                        grabbedObject.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.5f, 2f);
                    }
                    break;
            }
        }
    }
}