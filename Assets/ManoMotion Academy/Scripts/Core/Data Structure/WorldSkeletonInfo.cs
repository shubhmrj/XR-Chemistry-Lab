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
    public struct WorldSkeletonInfo
    {
        /// <summary>
        /// Normalized positions of the joints.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public Vector3[] jointPositions;

        /// <summary>
        /// Orientation of the joints.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public Quaternion[] jointNormalizedRotations;
    }
}