//using UnityEngine;
//using ManoMotion;
//
//public class BeakerRotation  : MonoBehaviour
//{
//    [SerializeField] private GameObject targetModel;
//
//    private float moveSpeed = 5f;
//    private float rotationSpeed = 50f;
//
//    void Start()
//    {
//        ManoMotionManager.Instance.ShouldCalculateGestures(true);
//    }
//
//    void Update()
//    {
//        // Loop through all the hand information
//        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
//        for (int i = 0; i < handInfos.Length; i++)
//        {
//            HandInfo handInfo = handInfos[i];
//
//            // If no found is hand continue to the next handInfo.
//            if (handInfos[i].gestureInfo.manoClass == ManoClass.NO_HAND)
//                continue;
//
//            // Get screen position of hand bounding box.
//            Vector3 handPosition = new Vector3(
//               handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
//               handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
//               0
//           );
//
//            switch (handInfo.gestureInfo.manoGestureContinuous)
//            {
//                case ManoGestureContinuous.OPEN_HAND_GESTURE:
//                    Debug.Log("OPEN");
//                    targetModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
//                ;
//
//                    targetModel.transform.position=new Vector3(targetModel.transform.position.x,targetModel.transform.position.y + 0.2f,targetModel.transform.position.z);
//
//                    break;
//                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                    Debug.Log("CLOSED");
//                    Vector3 targetPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(handPosition, 10);
//                    targetModel.transform.position = Vector3.Lerp(targetModel.transform.position, targetPosition, moveSpeed * Time.deltaTime);
//                    break;
//                    // targetModel.transform.position = new Vector3(
//                    // beakerA.transform.position = new Vector3(
//                    // beakerB.transform.position.x,
//                    // beakerB.transform.position.y + 0.2f,
//                    // beakerB.transform.position.z
//
//
//            }
//        }
//    }
//}