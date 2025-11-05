// using UnityEngine;
// using ManoMotion;

// public class ObjectGrabber : MonoBehaviour
// {
//     public Camera mainCamera;
//     public LayerMask grabbableLayer;
//     public float grabRange = 0.2f;

//     private GameObject grabbedObject = null;
//     private ManoGestureTrigger currentGesture;

//     void Update()
//     {
//         currentGesture = ManoMotionManager.Instance.HandInfos[0].gestureInfo.manoGestureTrigger;

//         if (currentGesture == ManoGestureTrigger.GRAB_GESTURE)
//         {
//             if (grabbedObject == null)
//                 TryGrabObject();
//             else
//                 MoveGrabbedObject();
//         }
//         else if (currentGesture == ManoGestureTrigger.RELEASE_GESTURE && grabbedObject != null)
//         {
//             ReleaseObject();
//         }
//     }

//     void TryGrabObject()
//     {
//     Vector3 handWorldPos = ManoUtils.Instance.CalculateNewPosition(
//         ManoMotionManager.Instance.HandInfos[0].trackingInfo.palm_center
//     );

//     RaycastHit hit;
//     if (Physics.Raycast(handWorldPos, mainCamera.transform.forward, out hit, grabRange, grabbableLayer))
//     {
//         grabbedObject = hit.collider.gameObject;
//         grabbedObject.transform.SetParent(transform); // Attach to hand
//     }
//  }


//     void MoveGrabbedObject()
//     {
//         Vector3 handWorldPos = ManoUtils.Instance.CalculateNewPosition(ManoMotionManager.Instance.HandInfos[0].trackingInfo.palm_center);
//         grabbedObject.transform.position = handWorldPos;
//     }

//     void ReleaseObject()
//     {
//         grabbedObject.transform.SetParent(null);
//         grabbedObject = null;
//     }
// }


