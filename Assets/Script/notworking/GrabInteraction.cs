// using UnityEngine;
// using ManoMotion; // ✅ Correct namespace for ManoMotion


// public class HandGestureControl : MonoBehaviour
// {
//     public GameObject objectToControl; // Assign the 3D object in Unity Inspector
//     public float moveSpeed = 3.0f;
//     public float rotateSpeed = 100.0f;

//     void Update()
//     {
//         // Ensure ManoMotionManager instance is valid
//         if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null || ManoMotionManager.Instance.HandInfos.Length == 0) 
//             return;

//         // Get hand tracking data
//         HandInfo handInfo = ManoMotionManager.Instance.HandInfos[0];

//         // ✅ Correct gesture detection
//         ManoGestureContinuous detectedGesture = handInfo.gestureInfo.manoGestureContinuous;

//         if (detectedGesture == ManoGestureContinuous.GRAB_GESTURE)
//         {
//             MoveObject(handInfo);
//         }
//         else if (detectedGesture == ManoGestureContinuous.POINTER_GESTURE)
//         {
//             RotateObject();
//         }
//     }

//     // Function to move object based on hand position
//     void MoveObject(HandInfo handInfo)
//     {
//        Vector3 handScreenPos = new Vector3(
//         (handInfo.trackingInfo.boundingBox.topLeft.x + (handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width)) / 2 * Screen.width,
//         (handInfo.trackingInfo.boundingBox.topLeft.y + (handInfo.trackingInfo.boundingBox.topLeft.y + handInfo.trackingInfo.boundingBox.height)) / 2 * Screen.height,
//         5.0f // Fixed depth
//         );


//         Vector3 handPosition = Camera.main.ScreenToWorldPoint(handScreenPos);
//         objectToControl.transform.position = Vector3.Lerp(objectToControl.transform.position, handPosition, Time.deltaTime * moveSpeed);    
//     }


//     // Function to rotate object
//     void RotateObject()
//     {
//         objectToControl.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
//     }
// }
