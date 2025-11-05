using UnityEngine;
using ManoMotion;

public class ARObjectGrabberPro : MonoBehaviour
{
    [Header("Assign your AR Camera here")]
    public Camera arCamera;

    [Header("Assign grabbable objects here")]
    public GameObject[] grabbableObjects; // Drag your 3D objects here in Inspector

    private GameObject grabbedObject = null;
    private Vector3 grabOffset;

    // Adjust this to control how close the hand must be to grab an object
    private float grabDistanceThreshold = 0.2f;

    void Update()
    {
        // Ensure ManoMotion is initialized and hand is detected
        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
            return;

        var handInfo = ManoMotionManager.Instance.HandInfos[0];
        // Use bounding box center as palm proxy
        var boundingBox = handInfo.trackingInfo.boundingBox;
        float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
        float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

        // Normalized screen coordinates
        Vector3 handScreenPos = new Vector3(centerX * Screen.width, centerY * Screen.height, 0.5f);
        Vector3 handWorldPos = arCamera.ScreenToWorldPoint(handScreenPos);

        var gesture = handInfo.gestureInfo.manoGestureContinuous;

        if (gesture == ManoGestureContinuous.CLOSED_HAND_GESTURE)
        {
            // If the hand is open, we can grab or move objects
            if (grabbedObject == null)
            {
                // Try to grab the nearest object within threshold
                float minDist = grabDistanceThreshold;
                foreach (var obj in grabbableObjects)
                {
                    float dist = Vector3.Distance(obj.transform.position, handWorldPos);
                    if (dist < minDist)
                    {
                        grabbedObject = obj;
                        grabOffset = obj.transform.position - handWorldPos;
                        minDist = dist;
                    }
                }
            }
            else
            {
                // Move grabbed object with hand
                grabbedObject.transform.position = handWorldPos + grabOffset;
            }
        }
        else
        {
            ReleaseObject();
        }
    }

    private void ReleaseObject()
    {
        grabbedObject = null;
    }
}
