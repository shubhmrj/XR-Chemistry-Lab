// using UnityEngine;
// using ManoMotion;

// public class ManoMotionCameraFix : MonoBehaviour
// {
//     [Header("Debug")]
//     [SerializeField] private bool showDebugInfo = true;
    
//     void Start()
//     {
//         // Ensure ManoMotion is properly initialized
//         if (ManoMotionManager.Instance != null)
//         {
//             // Force camera initialization
//             StartCoroutine(InitializeCameraWithDelay());
//         }
//         else
//         {
//             Debug.LogError("ManoMotionManager.Instance is null! Make sure ManoMotion is properly set up in the scene.");
//         }
//     }
    
//     System.Collections.IEnumerator InitializeCameraWithDelay()
//     {
//         // Wait a frame for everything to initialize
//         yield return new WaitForEndOfFrame();
        
//         try
//         {
//             // Check if camera is available
//             if (ManoMotionManager.Instance.InputManager != null)
//             {
//                 // Try to restart camera if it's not working
//                 ManoMotionManager.Instance.InputManager.SetFrontFacing(true);
//                 yield return new WaitForSeconds(0.5f);
                
//                 // Check camera state
//                 if (showDebugInfo)
//                 {
//                     Debug.Log($"Camera Front Facing: {ManoMotionManager.Instance.InputManager.IsFrontFacing}");
//                     Debug.Log($"Camera Resolution: {ManoMotionManager.Instance.InputManager.GetCameraResolution()}");
//                 }
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"Error initializing ManoMotion camera: {e.Message}");
//         }
//     }
    
//     void Update()
//     {
//         if (showDebugInfo && Input.GetKeyDown(KeyCode.C))
//         {
//             DebugCameraInfo();
//         }
//     }
    
//     void DebugCameraInfo()
//     {
//         if (ManoMotionManager.Instance?.InputManager != null)
//         {
//             Debug.Log("=== ManoMotion Camera Debug ===");
//             Debug.Log($"Is Front Facing: {ManoMotionManager.Instance.InputManager.IsFrontFacing}");
//             Debug.Log($"Camera Resolution: {ManoMotionManager.Instance.InputManager.GetCameraResolution()}");
//             Debug.Log($"Camera Texture: {ManoMotionManager.Instance.InputManager.GetCameraTexture()}");
            
//             // Check if camera texture is null (common cause of yellow screen)
//             var cameraTexture = ManoMotionManager.Instance.InputManager.GetCameraTexture();
//             if (cameraTexture == null)
//             {
//                 Debug.LogWarning("Camera texture is NULL! This causes yellow screen.");
//                 Debug.LogWarning("Try: 1. Check camera permissions, 2. Restart camera, 3. Check device camera");
//             }
//         }
//         else
//         {
//             Debug.LogError("ManoMotionManager or InputManager is null!");
//         }
//     }
    
//     // Public method to manually restart camera
//     public void RestartCamera()
//     {
//         if (ManoMotionManager.Instance?.InputManager != null)
//         {
//             bool currentFacing = ManoMotionManager.Instance.InputManager.IsFrontFacing;
            
//             // Toggle camera to restart it
//             ManoMotionManager.Instance.InputManager.SetFrontFacing(!currentFacing);
//             StartCoroutine(RestoreCameraFacing(currentFacing));
//         }
//     }
    
//     System.Collections.IEnumerator RestoreCameraFacing(bool originalFacing)
//     {
//         yield return new WaitForSeconds(1f);
//         ManoMotionManager.Instance.InputManager.SetFrontFacing(originalFacing);
//     }
// }
