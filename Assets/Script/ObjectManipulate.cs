// using UnityEngine;
// using ManoMotion; // Ensure you have the ManoGestureLib namespace
// public class ObjectManipulate : MonoBehaviour
// {
//     [SerializeField]
//     private GameObject controlledObject; // Assign this in Unity Inspector
//     public float movementSpeed = 5.0f; // Speed of movement

//     private HandInfo handInfo; // Declare handInfo

//     void Update()
//     {
//         // Initialize handInfo (replace this with the actual method to get handInfo in your project)
//         handInfo = ManoMotionManager.Instance.HandInfos[0];

//         ManoGestureContinuous detectedGesture = handInfo.gestureInfo.manoGestureContinuous;

//         if (detectedGesture == ManoGestureContinuous.GRAB_GESTURE)
//         {
//             MoveObjectWithHandInfo(handInfo);
//         }
//     }

//     void MoveObjectWithHandInfo(HandInfo handInfoParam)
//     {
//         if (this.controlledObject == null)
//         {
//             Debug.LogError("❌ ERROR: No object assigned to 'objectToControl'! Assign it in Unity Inspector.");
//             return;
//         }

//         // Get bounding box info
//         TrackingInfo trackingInfo = handInfo.trackingInfo;
//         BoundingBox boundingBox = trackingInfo.boundingBox;

//         float centerX = boundingBox.topLeft.x + (boundingBox.width / 2);
//         float centerY = boundingBox.topLeft.y + (boundingBox.height / 2);

//         Vector2 handScreenPos = new Vector2(centerX * Screen.width, centerY * Screen.height);

//         // Convert screen position to world position
//         Vector3 handWorldPos;
//         Ray ray = Camera.main.ScreenPointToRay(handScreenPos);

//         if (Physics.Raycast(ray, out RaycastHit hit))
//         {
//             handWorldPos = hit.point;

//             this.controlledObject.transform.position = Vector3.Lerp(
//                 this.controlledObject.transform.position,
//                 handWorldPos,
//                 this.movementSpeed * Time.deltaTime
//             );
//         }
//         else
//         {
//             Debug.LogWarning("⚠️ Warning: Raycast did not hit any surface.");
//         }
//     }
// }
