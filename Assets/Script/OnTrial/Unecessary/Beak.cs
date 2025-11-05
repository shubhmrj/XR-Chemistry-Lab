using UnityEngine;
using ManoMotion;

public class Beak : MonoBehaviour
{
    [SerializeField] private GameObject beakerModel;
    
    private float tiltSpeed = 30f;
    private float rotationSpeed = 50f;
    private float moveSpeed = 5f;
    private Vector3 originalScale;
    private bool isGrabbing = false;
    private Vector3 lastValidPosition;

    void Start()
    {
        if (ManoMotionManager.Instance != null)
        {
            ManoMotionManager.Instance.ShouldCalculateGestures(true);
        }
        
        if (beakerModel != null)
        {
            originalScale = beakerModel.transform.localScale;
            lastValidPosition = beakerModel.transform.position;
        }
    }

    void LateUpdate() // Using LateUpdate to ensure scale is applied after all other transformations
    {
        // Force scale to stay constant every frame
        if (beakerModel != null)
        {
            beakerModel.transform.localScale = originalScale;
        }
    }

    void Update()
    {
        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null || beakerModel == null)
            return;

        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
        bool handDetected = false;

        foreach (var handInfo in handInfos)
        {
            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
                continue;
            
            handDetected = true;
            
            Vector3 handPosition = new Vector3(
               handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
               handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
               0
            );

            ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;

            // Approximate palm center using the center of the bounding box
            BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;

            float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
            float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;

            // Normalize coordinates to range (-0.5, 0.5)
            float normalizedX = centerX - 0.5f;
            float normalizedY = 0.5f - centerY;

            switch (gesture)
            {
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    isGrabbing = false;
                    float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
                    float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;

                    beakerModel.transform.Rotate(tiltX, 0, tiltZ);
                    break;

                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    isGrabbing = false;
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    break;

                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
                    
                    Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
                    
                    // Store the position before moving for validation
                    Vector3 previousPosition = beakerModel.transform.position;
                    
                    // Move the object
                    beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    
                    // Check if scale changed after moving
                    if (beakerModel.transform.localScale != originalScale)
                    {
                        // Reset scale and store this as a valid position
                        beakerModel.transform.localScale = originalScale;
                        lastValidPosition = beakerModel.transform.position;
                    }
                    
                    isGrabbing = true;
                    Handheld.Vibrate();
                    break;
            }
        }

        // If no hand is detected and we were grabbing, ensure scale is maintained
        if (!handDetected && isGrabbing)
        {
            isGrabbing = false;
            beakerModel.transform.localScale = originalScale;
        }
    }
}