using UnityEngine;
using System;

namespace ManoMotion
{
    /// <summary>
    /// Tools that can be used to get information adapted to the screen size.
    /// </summary>
    public class ManoUtils : MonoBehaviour
    {
        [SerializeField] SupportedOrientation currentOrientation;
        public bool updateOrientation = true;

        private static ManoUtils instance;
        private Camera cam;
        private Vector3 correctionRatio = Vector3.one;
        bool shouldBackgroundCoverScreen = true;
        float zoomValue = 1f;

        public static ManoUtils Instance => instance;
        public SupportedOrientation Orientation => currentOrientation;

        private float Zoom => shouldBackgroundCoverScreen ? 1f : zoomValue;

        public bool ZoomedIn => shouldBackgroundCoverScreen;

        public static Action OnOrientationChanged;

        protected void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }
            instance = this;
            cam = Camera.main;
        }

        private void Start()
        {
            OnOrientationChanged?.Invoke();
        }

        void Update()
        {
            if (updateOrientation)
            {
                CheckForScreenOrientationChange();
            }
        }

        /// <summary>
        /// Checks for changes on the orientation of the device.
        /// </summary>
        void CheckForScreenOrientationChange()
        {
#if !UNITY_STANDALONE
            DeviceOrientation orientation = Input.deviceOrientation;

            switch (orientation)
            {
                case DeviceOrientation.Unknown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.FaceDown:
                    return;
            }

            if ((int)currentOrientation != (int)orientation)
            {
                int currentInputOrietation = (int)Input.deviceOrientation;
                currentOrientation = (SupportedOrientation)currentInputOrietation;
                OnOrientationChanged?.Invoke();
            }
#endif
        }

        /// <summary>
        /// Calculates the new position in relation to the main camera, for the skleleton joints with clamped depth of -1 and 1.
        /// </summary>
        public Vector3 CalculateNewPositionWithDepth(Vector3 point, float depth)
        {
            Vector3 correctionPoint = point - Vector3.one * 0.5f;
            correctionPoint.Scale(correctionRatio);

            // Match positions even when zoomed in/out
            correctionPoint.x *= Zoom;
            correctionPoint.y *= Zoom;

            correctionPoint = correctionPoint + Vector3.one * 0.5f;
            correctionPoint = new Vector3(Mathf.Clamp(correctionPoint.x, 0, 1), Mathf.Clamp(correctionPoint.y, 0, 1), Mathf.Clamp(correctionPoint.z, -1, 1));
            return cam.ViewportToWorldPoint(correctionPoint + Vector3.forward * depth);
        }

        /// <summary>
        /// Calculates the screen position of a normalized position. Used for bounding box.
        /// </summary>
        public Vector3 CalculateScreenPosition(Vector3 point)
        {
            Vector3 correctionPoint = point - Vector3.one * 0.5f;
            correctionPoint.Scale(correctionRatio);

            // Match positions even when zoomed in/out
            correctionPoint.x *= Zoom;
            correctionPoint.y *= Zoom;

            correctionPoint = correctionPoint + Vector3.one * 0.5f;
            correctionPoint = new Vector3(Mathf.Clamp(correctionPoint.x, 0, 1), Mathf.Clamp(correctionPoint.y, 0, 1), Mathf.Clamp(correctionPoint.z, -1, 1));
            return cam.ViewportToScreenPoint(correctionPoint);
        }

        /// <summary>
        /// Adjust the transform in the received mesh renderer to fit the screen without stretching
        /// </summary>
        internal void AdjustBorders(MeshRenderer meshRenderer, Session session)
        {
            float ratio = CalculateRatio(session);
            float size = CalculateSize(meshRenderer, session, ratio);

            AdjustMeshScale(meshRenderer, session, ratio, size);
            CalculateCorrectionPoint(meshRenderer, session, ratio, size);
        }

        /// <summary>
        /// Calculates the current ratio depending on the device orientation
        /// </summary>
        internal float CalculateRatio(Session session)
        {
            switch (session.orientation)
            {
                case SupportedOrientation.FACE_DOWN:
                case SupportedOrientation.FACE_UP:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                default:
                    return (float)ManoMotionManager.Instance.Height / ManoMotionManager.Instance.Width;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                    return (float)ManoMotionManager.Instance.Width / ManoMotionManager.Instance.Height;
            }
        }

        /// <summary>
        /// Gets the size for the AdjustBorders method.
        /// </summary>
        internal float CalculateSize(MeshRenderer meshRenderer, Session session, float ratio)
        {
            float height = 2f * Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * meshRenderer.transform.localPosition.z * Zoom;

            switch (session.orientation)
            {
                case SupportedOrientation.FACE_DOWN:
                case SupportedOrientation.FACE_UP:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                    return height;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                default:
                    float width = height * (float)Screen.width / Screen.height;
                    return width / ratio;
            }
        }

        /// <summary>
        /// Adjust the scale of the mesh render.
        /// </summary>
        internal void AdjustMeshScale(MeshRenderer meshRenderer, Session session, float ratio, float size)
        {
            switch (session.orientation)
            {
                case SupportedOrientation.FACE_DOWN:
                case SupportedOrientation.FACE_UP:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                default:
                    meshRenderer.transform.localScale = new Vector3(size, size * ratio, 0f);
                    break;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                    meshRenderer.transform.localScale = new Vector3(size * ratio, size, 0f);
                    break;
            }
        }

        /// <summary>
        /// Calculate a correction point depending on the orientation.
        /// </summary>
        internal void CalculateCorrectionPoint(MeshRenderer meshRenderer, Session session, float ratio, float size)
        {
            Vector3 screenRatio;
            Vector3 imageRatio;
            switch (session.orientation)
            {
                case SupportedOrientation.FACE_DOWN:
                case SupportedOrientation.FACE_UP:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                    screenRatio = new Vector3((float)Screen.height / Screen.width, 1, 1);
                    imageRatio = new Vector3(ratio, 1, 1);
                    correctionRatio = Vector3.Scale(screenRatio, imageRatio);
                    zoomValue = 1f / correctionRatio.x;
                    break;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                    screenRatio = new Vector3(1, (float)Screen.width / Screen.height, 1);
                    imageRatio = new Vector3(1, 1 / ratio, 1);
                    correctionRatio = Vector3.Scale(screenRatio, imageRatio);
                    zoomValue = 1f / correctionRatio.y;
                    break;
                default:
                    meshRenderer.transform.localScale = new Vector3(size, size * ratio, 0f);
                    break;
            }
        }

        /// <summary>
        /// Properly orients a MeshRenderer in order to be displayed properly
        /// </summary>
        /// <param name="meshRenderer">Mesh renderer.</param>
        public void OrientMeshRenderer(MeshRenderer meshRenderer)
        {
            if (ManoMotionManager.Instance.ManomotionSession.addOn == AddOn.DEFAULT)
            {
                switch (ManoMotionManager.Instance.ManomotionSession.orientation)
                {
                    case SupportedOrientation.PORTRAIT:
                    case SupportedOrientation.FACE_DOWN:
                    case SupportedOrientation.FACE_UP:
                    case SupportedOrientation.UNKNOWN:
                    case SupportedOrientation.PORTRAIT_FRONT_FACING:
                        meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                    case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                        meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case SupportedOrientation.LANDSCAPE_LEFT:
                    case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                        meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case SupportedOrientation.LANDSCAPE_RIGHT:
                    case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                        meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, 180);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        /// <summary>
        /// Update value that decides how the camera image is displayed.
        /// If true it will cover the entire screen, if false the entire image will be visible.
        /// </summary>
        public void ShouldBackgroundCoverScreen(bool shouldCoverScreen)
        {
            shouldBackgroundCoverScreen = shouldCoverScreen;
        }

        /// <summary>
        /// Set the orientation. Used when locking the screen in certain scenes.
        /// </summary>
        public void SetOrientation(SupportedOrientation orientation)
        {
            currentOrientation = orientation;
            OnOrientationChanged?.Invoke();
        }

        public static Vector3 GetCenter(params Vector3[] points)
        {
            float x = 0;
            float y = 0;
            float z = 0;

            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].x;
                y += points[i].y;
                z += points[i].z;
            }

            return new Vector3(x, y, z) / points.Length;
        }
    }
}