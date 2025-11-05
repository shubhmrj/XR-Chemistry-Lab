// using UnityEngine;
// using ManoMotion;
// // using ManoMotion.ManomotionAPI;

// public class MM_ARObjectGrabber : MonoBehaviour
// {
//     [Header("Assign your AR objects here (3 objects)")]
//     public GameObject[] grabbableObjects; // Assign 3 objects in the Inspector

//     [Header("Reference to AR Camera")]
//     public Camera arCamera;

//     private GameObject currentlyGrabbedObject = null;
//     private Vector3 grabOffset;

//     void Update()
//     {
//         // Ensure ManoMotion is initialized and hand is detected
//         var handInfo = ManoMotionManager.Instance.HandInfos[0];
//         if (!handInfo.trackingInfo.is_hand_detected) {
//             ReleaseObject();
//             return;
//         }

//         // Get current gesture
//         var gesture = handInfo.gesture_info.mano_gesture_continuous;

//         // Get palm center (normalized [0,1])
//         Vector3 palmNorm = handInfo.tracking_info.palm_center;
//         Vector3 palmScreen = new Vector3(palmNorm.x * Screen.width, palmNorm.y * Screen.height, 0.5f);
//         Vector3 palmWorld = arCamera.ScreenToWorldPoint(palmScreen);

//         // If hand is grabbing
//         if (gesture == ManoGestureContinuous.GRAB_GESTURE)
//         {
//             if (currentlyGrabbedObject == null)
//             {
//                 // Try to grab nearest object within threshold
//                 float minDist = 0.2f;
//                 foreach (var obj in grabbableObjects)
//                 {
//                     float dist = Vector3.Distance(obj.transform.position, palmWorld);
//                     if (dist < minDist)
//                     {
//                         currentlyGrabbedObject = obj;
//                         grabOffset = obj.transform.position - palmWorld;
//                         minDist = dist;
//                     }
//                 }
//             }
//             else
//             {
//                 // Move grabbed object with hand
//                 currentlyGrabbedObject.transform.position = palmWorld + grabOffset;
//             }
//         }
//         else
//         {
//             // Release object if not grabbing
//             ReleaseObject();
//         }
//     }

//     private void ReleaseObject()
//     {
//         currentlyGrabbedObject = null;
//     }
// }
