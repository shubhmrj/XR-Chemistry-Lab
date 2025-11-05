using UnityEngine;

namespace ManoMotion
{
    /// <summary>
    /// Bounding box of a hand.
    /// </summary>
    [System.Serializable]
    public struct BoundingBox
    {
        public Vector3 topLeft;
        public float width;
        public float height;
    }
}