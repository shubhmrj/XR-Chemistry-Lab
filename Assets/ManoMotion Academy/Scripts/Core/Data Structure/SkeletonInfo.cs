using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ManoMotion
{
    /// <summary>
    /// Contains information about the skeleton joints.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct SkeletonInfo
    {
        /// <summary>
        /// Skeleton confidence value of 0 or 1. 1 if skeleton is detected.
        /// </summary>
        public float confidence;

        /// <summary>
        /// Position of the joints.
        /// normalized values between 0 and 1
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public Vector3[] jointPositions;
    }
}