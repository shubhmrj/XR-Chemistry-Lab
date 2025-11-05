using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ManoMotion
{
    /// <summary>
    /// Gives information about the width of the fingers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct FingerInfo
    {
        /// <summary>
        ///The normalized left position.
        /// </summary>
        public Vector3 leftPoint;

        /// <summary>
        ///The normalized right position.
        /// </summary>
        public Vector3 rightPoint;

        /// <summary>
        /// Warning flag if finger info can´t be calculated correctly.
        /// </summary>
        public int fingerWarning;
    }
}