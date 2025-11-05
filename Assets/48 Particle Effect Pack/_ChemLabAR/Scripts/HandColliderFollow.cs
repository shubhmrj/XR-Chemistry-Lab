// using UnityEngine;
// using ManoMotion;

// public class HandColliderFollow : MonoBehaviour
// {
//     void Update()
//     {
//         if (ManoMotionManager.Instance != null && ManoMotionManager.Instance.HandInfos.Length > 0)
//         {
//             Vector3 handCenter = ManoMotionManager.Instance.HandInfos[0].handCenter;
//             transform.position = Camera.main.ViewportToWorldPoint(new Vector3(handCenter.x, handCenter.y, 0.5f)); // 0.5f is depth
//         }
//     }
// }
