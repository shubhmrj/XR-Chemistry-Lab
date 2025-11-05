//using UnityEngine;
//using ManoMotion;
//
//public class BeakerTiltController : MonoBehaviour
//{
//    [SerializeField] private GameObject beakerModel;
//
//    private float tiltSpeed = 30f;
//    private float rotationSpeed = 50f;
//    private float moveSpeed = 5f;
//
//
//    void Start()
//    {
//        if (ManoMotionManager.Instance != null)
//        {
//            ManoMotionManager.Instance.ShouldCalculateGestures(true);
//        }
//    }
//
//    void Update()
//    {
//        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
//            return;
//
//        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
//
//        foreach (var handInfo in handInfos)
//        {
//            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
//                continue;
//
//            Vector3 handPosition = new Vector3(
//               handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
//               handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
//               0
//           );
//
//            ManoGestureContinuous gesture = handInfo.gestureInfo.manoGestureContinuous;
//
//            // Approximate palm center using the center of the bounding box
//            BoundingBox boundingBox = handInfo.trackingInfo.boundingBox;
//
//            float centerX = boundingBox.topLeft.x + boundingBox.width / 2f;
//            float centerY = boundingBox.topLeft.y - boundingBox.height / 2f;
//
//            // Normalize coordinates to range (-0.5, 0.5)
//            float normalizedX = centerX - 0.5f;
//            float normalizedY = 0.5f - centerY;
//
//            switch (gesture)
//            {
//                case ManoGestureContinuous.POINTER_GESTURE:
//                    float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
//                    float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;
//
//                    beakerModel.transform.Rotate(tiltX, 0, tiltZ);
//                    break;
//
//                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
//                    // beakerModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
//                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
//                    break;
//
//                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
//
//                    Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
//                    beakerModel.transform.position = Vector3.Lerp(beakerModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
//
//                    Handheld.Vibrate();
//                    break;
//
//            }
//        }
//    }
//}
