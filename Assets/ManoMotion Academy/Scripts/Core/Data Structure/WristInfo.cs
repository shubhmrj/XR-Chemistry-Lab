using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ManoMotion
{
    /// <summary>
    /// Information of wrist position.
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct WristInfo
    {
        /// <summary>
        ///The normalized left wrist position.
        /// </summary>
        public Vector3 leftPoint;

        /// <summary>
        ///The normalized right wrist position.
        /// </summary>
        public Vector3 rightPoint;

        /// <summary>
        /// Warning flag if wrist info can´t be calculated correctly.
        /// </summary>
        public int wristWarning;
    }
}