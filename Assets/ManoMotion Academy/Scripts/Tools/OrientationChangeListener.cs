using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion.Tools
{
    /// <summary>
    /// Listens to orientation changes and broadcasts events.
    /// </summary>
    public class OrientationChangeListener : MonoBehaviour
    {
        [SerializeField] UnityEvent OnPortrait, OnLandscape;

        private void OnEnable()
        {
            ManoUtils.OnOrientationChanged += OrientationChanged;
        }

        private void OnDisable()
        {
            ManoUtils.OnOrientationChanged -= OrientationChanged;
        }

        private void OrientationChanged()
        {
            switch (ManoUtils.Instance.Orientation)
            {
                case SupportedOrientation.FACE_DOWN:
                case SupportedOrientation.FACE_UP:
                case SupportedOrientation.PORTRAIT:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN:
                case SupportedOrientation.PORTRAIT_FRONT_FACING:
                case SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING:
                    OnPortrait?.Invoke();
                    break;
                case SupportedOrientation.LANDSCAPE_LEFT:
                case SupportedOrientation.LANDSCAPE_RIGHT:
                case SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING:
                case SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING:
                    OnLandscape?.Invoke();
                    break;
            }
        }
    }
}