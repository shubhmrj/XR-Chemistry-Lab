using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ManoMotion
{
    /// <summary>
    /// Contains information about position and tracking of the hand
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct TrackingInfo
    {
        /// <summary>
        /// Provides tracking information regarding the bounding box that contains the hand.
        /// it yields normalized values between 0 and 1
        /// </summary>
        public BoundingBox boundingBox;

        /// <summary>
        /// Information about wrist points.
        /// </summary>
        public WristInfo wristInfo;

        /// ### Example
        /// @code
        ///private float depthValue;
        ///private float maxDepth = 0.8f;
        ///
        ///// <summary>
        ///// Runs every frame, gets the float value of the depth_estimation.
        ///// </summary>
        ///void Update()
        ///{
        ///	depthValue = ManomotionManager.Instance.HandInfos[0].trackingInfo.depthEstimation;
        ///	CheckDepth(depthValue);
        ///}
        ///
        ///// <summary>
        ///// Checks if current depth is greater than our maxDepth value, if so the phone will vibrate
        ///// </summary>
        ///// <param name="depth">the current depthValue from depthEstimation</param>
        ///void CheckDepth(float depth)
        ///{
        ///	if (depth > maxDepth)
        ///	{
        ///		// Your code here
        ///		Handheld.Vibrate();
        ///	}
        ///}
        /// 
        /// @endcode
        /// 
        /// <summary>
        /// Provides tracking information regarding an estimation  of the hand depth. 
        /// it yields a 0-1 float value depending based on the distance of the hand from the camera.
        /// </summary>
        public float depthEstimation;

        /// <summary>
        /// The amount of contour points the contour uses for each frame.
        /// </summary>
        public int numberOfContourPoints;

        /// <summary>
        /// 200 normalized contour points to get the contour of the hand. 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public Vector3[] contourPoints;

        /// <summary>
        /// Contains the positions of the 21 joints
        /// </summary>
        public SkeletonInfo skeleton;

        public WorldSkeletonInfo worldSkeleton;

        /// <summary>
        /// Information about winger width
        /// </summary>
        public FingerInfo fingerInfo;
    }
}